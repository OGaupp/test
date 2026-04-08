using CadTool.Core.Math;
using CadTool.Core.Mesh;

namespace CadTool.Geometry.Mesh;

/// <summary>
/// Raeumlicher Index fuer schnelle Punkt-in-Mesh-Pruefungen.
/// Unterteilt den Bounding-Box-Raum in ein gleichmaessiges 3D-Gitter (Uniform Grid)
/// und ordnet Dreiecke den Zellen zu, die ihre AABB ueberlappt.
///
/// Dadurch reduziert sich der Aufwand fuer Ray-Triangle-Schnittests
/// von O(n) auf O(k), wobei k die Anzahl der Dreiecke entlang des Strahlpfades ist.
/// </summary>
internal sealed class MeshSpatialIndex
{
    private readonly TriangleMesh _mesh;
    private readonly BoundingBox3D _expandedBounds;
    private readonly int _resX;
    private readonly int _resY;
    private readonly int _resZ;
    private readonly double _cellSizeX;
    private readonly double _cellSizeY;
    private readonly double _cellSizeZ;
    private readonly List<int>?[] _cells;

    /// <summary>Strahlrichtungen fuer den Majority-Vote-Innentest (identisch mit MeshBooleanOperations).</summary>
    private static readonly Vector3D[] RayDirections =
    [
        new(1.0, 0.00123, 0.00456),
        new(0.00789, 1.0, 0.00234),
        new(0.00567, 0.00891, 1.0)
    ];

    /// <summary>
    /// Erzeugt einen raeumlichen Index fuer das gegebene Mesh.
    /// </summary>
    public MeshSpatialIndex(TriangleMesh mesh)
    {
        ArgumentNullException.ThrowIfNull(mesh);
        _mesh = mesh;

        var bounds = mesh.GetBoundingBox();
        var size = bounds.Size;

        // Degenerate Dimensionen leicht aufweiten, um Division durch Null zu vermeiden
        const double minExtent = 1e-6;
        var expandX = size.X < minExtent ? minExtent : 0.0;
        var expandY = size.Y < minExtent ? minExtent : 0.0;
        var expandZ = size.Z < minExtent ? minExtent : 0.0;

        _expandedBounds = new BoundingBox3D(
            new Vector3D(bounds.Min.X - expandX, bounds.Min.Y - expandY, bounds.Min.Z - expandZ),
            new Vector3D(bounds.Max.X + expandX, bounds.Max.Y + expandY, bounds.Max.Z + expandZ));

        var expandedSize = _expandedBounds.Size;

        // Gitteraufloesung basierend auf Dreiecksanzahl
        var triCount = mesh.TriangleCount;
        var targetRes = System.Math.Max(1, (int)System.Math.Ceiling(System.Math.Cbrt(triCount)));
        _resX = targetRes;
        _resY = targetRes;
        _resZ = targetRes;

        _cellSizeX = expandedSize.X / _resX;
        _cellSizeY = expandedSize.Y / _resY;
        _cellSizeZ = expandedSize.Z / _resZ;

        // Zellen initialisieren
        _cells = new List<int>?[_resX * _resY * _resZ];

        // Dreiecke in Zellen einsortieren
        for (var i = 0; i < triCount; i++)
        {
            var tri = mesh.GetTriangle(i);
            var triBounds = ComputeTriangleBounds(tri);

            var (minCx, minCy, minCz) = WorldToCell(triBounds.Min);
            var (maxCx, maxCy, maxCz) = WorldToCell(triBounds.Max);

            for (var cz = minCz; cz <= maxCz; cz++)
            for (var cy = minCy; cy <= maxCy; cy++)
            for (var cx = minCx; cx <= maxCx; cx++)
            {
                var cellIdx = CellIndex(cx, cy, cz);
                _cells[cellIdx] ??= [];
                _cells[cellIdx]!.Add(i);
            }
        }
    }

    /// <summary>
    /// Prueft ob ein Punkt innerhalb des Mesh liegt (Majority-Vote-Ray-Casting mit Gitter-Beschleunigung).
    /// </summary>
    public bool IsPointInside(Vector3D point)
    {
        // Schneller BBox-Abbruch
        if (!_expandedBounds.Contains(point))
            return false;

        var insideVotes = 0;
        foreach (var rayDir in RayDirections)
        {
            var intersectionCount = CountRayIntersections(point, rayDir);
            if (intersectionCount % 2 == 1)
                insideVotes++;
        }

        return insideVotes >= 2;
    }

    /// <summary>
    /// Zaehlt die Anzahl der Ray-Triangle-Schnitte fuer einen gegebenen Strahl.
    /// Nutzt 3D-DDA-Gitter-Traversierung fuer fruehes Aussortieren.
    /// </summary>
    internal int CountRayIntersections(Vector3D origin, Vector3D direction)
    {
        // Strahl gegen Gitter-BBox pruefen
        if (!IntersectRayBox(origin, direction, out var tEntry, out _))
            return 0;

        tEntry = System.Math.Max(tEntry, 0.0);

        // Kandidaten per DDA-Traversierung sammeln
        var candidates = CollectCandidatesViaDda(origin, direction, tEntry);

        // Moeller-Trumbore auf Kandidaten ausfuehren
        var count = 0;
        foreach (var triIdx in candidates)
        {
            var tri = _mesh.GetTriangle(triIdx);
            if (MeshBooleanOperations.RayIntersectsTriangle(origin, direction, tri))
                count++;
        }

        return count;
    }

    /// <summary>
    /// 3D-DDA-Traversierung (Amanatides & Woo) durch das Gitter.
    /// Sammelt alle Dreiecksindizes in Zellen entlang des Strahlpfades.
    /// </summary>
    private HashSet<int> CollectCandidatesViaDda(Vector3D origin, Vector3D direction, double tStart)
    {
        var candidates = new HashSet<int>();

        // Startpunkt leicht versetzt, um sicher innerhalb des Gitters zu starten
        var startPoint = origin + direction * (tStart + 1e-10);
        var (cx, cy, cz) = WorldToCell(startPoint);

        // Schrittrichtung pro Achse
        var stepX = direction.X >= 0 ? 1 : -1;
        var stepY = direction.Y >= 0 ? 1 : -1;
        var stepZ = direction.Z >= 0 ? 1 : -1;

        // tDelta: Strecke entlang des Strahls fuer eine Zellbreite
        var tDeltaX = System.Math.Abs(direction.X) > 1e-15
            ? System.Math.Abs(_cellSizeX / direction.X)
            : double.PositiveInfinity;
        var tDeltaY = System.Math.Abs(direction.Y) > 1e-15
            ? System.Math.Abs(_cellSizeY / direction.Y)
            : double.PositiveInfinity;
        var tDeltaZ = System.Math.Abs(direction.Z) > 1e-15
            ? System.Math.Abs(_cellSizeZ / direction.Z)
            : double.PositiveInfinity;

        // tMaxX/Y/Z: t-Wert, bei dem der Strahl die naechste Zellgrenze in der jeweiligen Achse erreicht
        var tMaxX = ComputeInitialTMax(origin.X, direction.X, _expandedBounds.Min.X, _cellSizeX, cx, stepX);
        var tMaxY = ComputeInitialTMax(origin.Y, direction.Y, _expandedBounds.Min.Y, _cellSizeY, cy, stepY);
        var tMaxZ = ComputeInitialTMax(origin.Z, direction.Z, _expandedBounds.Min.Z, _cellSizeZ, cz, stepZ);

        // Sicherheitsgrenze gegen Endlosschleife
        var maxSteps = _resX + _resY + _resZ;
        var steps = 0;

        while (cx >= 0 && cx < _resX && cy >= 0 && cy < _resY && cz >= 0 && cz < _resZ && steps < maxSteps * 3)
        {
            steps++;
            var cellIdx = CellIndex(cx, cy, cz);
            var cellTriangles = _cells[cellIdx];
            if (cellTriangles is not null)
            {
                foreach (var triIdx in cellTriangles)
                {
                    candidates.Add(triIdx);
                }
            }

            // Zur naechsten Zelle vorrücken (kleinster tMax bestimmt die Achse)
            if (tMaxX < tMaxY)
            {
                if (tMaxX < tMaxZ)
                {
                    cx += stepX;
                    tMaxX += tDeltaX;
                }
                else
                {
                    cz += stepZ;
                    tMaxZ += tDeltaZ;
                }
            }
            else
            {
                if (tMaxY < tMaxZ)
                {
                    cy += stepY;
                    tMaxY += tDeltaY;
                }
                else
                {
                    cz += stepZ;
                    tMaxZ += tDeltaZ;
                }
            }
        }

        return candidates;
    }

    /// <summary>
    /// Berechnet den initialen tMax-Wert fuer eine Achse in der DDA-Traversierung.
    /// </summary>
    private static double ComputeInitialTMax(
        double originComponent, double dirComponent,
        double gridMin, double cellSize, int cellIndex, int step)
    {
        if (System.Math.Abs(dirComponent) < 1e-15)
            return double.PositiveInfinity;

        // Naechste Zellgrenze in Schrittrichtung
        var nextBoundary = gridMin + (cellIndex + (step > 0 ? 1 : 0)) * cellSize;
        return (nextBoundary - originComponent) / dirComponent;
    }

    /// <summary>
    /// Slab-basierter Ray-AABB-Schnitttest gegen die Gitter-BBox.
    /// </summary>
    private bool IntersectRayBox(Vector3D origin, Vector3D direction, out double tMin, out double tMax)
    {
        tMin = double.NegativeInfinity;
        tMax = double.PositiveInfinity;

        // X-Slab
        if (System.Math.Abs(direction.X) > 1e-15)
        {
            var t1 = (_expandedBounds.Min.X - origin.X) / direction.X;
            var t2 = (_expandedBounds.Max.X - origin.X) / direction.X;
            if (t1 > t2) (t1, t2) = (t2, t1);
            tMin = System.Math.Max(tMin, t1);
            tMax = System.Math.Min(tMax, t2);
        }
        else if (origin.X < _expandedBounds.Min.X || origin.X > _expandedBounds.Max.X)
        {
            tMin = 0;
            tMax = -1;
            return false;
        }

        // Y-Slab
        if (System.Math.Abs(direction.Y) > 1e-15)
        {
            var t1 = (_expandedBounds.Min.Y - origin.Y) / direction.Y;
            var t2 = (_expandedBounds.Max.Y - origin.Y) / direction.Y;
            if (t1 > t2) (t1, t2) = (t2, t1);
            tMin = System.Math.Max(tMin, t1);
            tMax = System.Math.Min(tMax, t2);
        }
        else if (origin.Y < _expandedBounds.Min.Y || origin.Y > _expandedBounds.Max.Y)
        {
            tMin = 0;
            tMax = -1;
            return false;
        }

        // Z-Slab
        if (System.Math.Abs(direction.Z) > 1e-15)
        {
            var t1 = (_expandedBounds.Min.Z - origin.Z) / direction.Z;
            var t2 = (_expandedBounds.Max.Z - origin.Z) / direction.Z;
            if (t1 > t2) (t1, t2) = (t2, t1);
            tMin = System.Math.Max(tMin, t1);
            tMax = System.Math.Min(tMax, t2);
        }
        else if (origin.Z < _expandedBounds.Min.Z || origin.Z > _expandedBounds.Max.Z)
        {
            tMin = 0;
            tMax = -1;
            return false;
        }

        return tMin <= tMax;
    }

    /// <summary>Konvertiert eine Weltkoordinate in Zellindizes (geclampt auf Gittergrenzen).</summary>
    private (int x, int y, int z) WorldToCell(Vector3D point)
    {
        var x = (int)((point.X - _expandedBounds.Min.X) / _cellSizeX);
        var y = (int)((point.Y - _expandedBounds.Min.Y) / _cellSizeY);
        var z = (int)((point.Z - _expandedBounds.Min.Z) / _cellSizeZ);

        x = System.Math.Clamp(x, 0, _resX - 1);
        y = System.Math.Clamp(y, 0, _resY - 1);
        z = System.Math.Clamp(z, 0, _resZ - 1);

        return (x, y, z);
    }

    /// <summary>Berechnet den flachen Array-Index fuer eine Gitterzelle.</summary>
    private int CellIndex(int x, int y, int z) => x + y * _resX + z * _resX * _resY;

    /// <summary>Berechnet die AABB eines Dreiecks.</summary>
    private static BoundingBox3D ComputeTriangleBounds(Triangle3D tri) =>
        new(
            new Vector3D(
                System.Math.Min(tri.V0.X, System.Math.Min(tri.V1.X, tri.V2.X)),
                System.Math.Min(tri.V0.Y, System.Math.Min(tri.V1.Y, tri.V2.Y)),
                System.Math.Min(tri.V0.Z, System.Math.Min(tri.V1.Z, tri.V2.Z))),
            new Vector3D(
                System.Math.Max(tri.V0.X, System.Math.Max(tri.V1.X, tri.V2.X)),
                System.Math.Max(tri.V0.Y, System.Math.Max(tri.V1.Y, tri.V2.Y)),
                System.Math.Max(tri.V0.Z, System.Math.Max(tri.V1.Z, tri.V2.Z))));
}
