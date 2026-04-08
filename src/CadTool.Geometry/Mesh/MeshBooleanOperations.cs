using CadTool.Core.Math;
using CadTool.Core.Mesh;

namespace CadTool.Geometry.Mesh;

/// <summary>
/// Boole'sche Operationen auf Dreiecksnetzen (CSG: Constructive Solid Geometry).
///
/// Implementierung basiert auf einem vereinfachten Ansatz fuer Primitiv-Subtraktionen:
/// - Klassifizierung der Dreiecke nach Innen/Aussen relativ zum anderen Koerper
/// - Verwendung von Bounding-Box-Tests fuer schnelle Vorfilterung
/// - Punkt-in-Mesh-Test via Ray-Casting (gerade Anzahl Schnitte = aussen)
///
/// Fuer komplexe Freiformflaechen waere eine vollstaendige BSP-Tree- oder
/// Exact-Arithmetic-Implementierung noetig, was aber laut Anforderung nicht vorgesehen ist.
/// </summary>
public static class MeshBooleanOperations
{
    /// <summary>Vereinigung zweier Meshes: A + B.</summary>
    public static TriangleMesh Union(TriangleMesh meshA, TriangleMesh meshB)
    {
        ArgumentNullException.ThrowIfNull(meshA);
        ArgumentNullException.ThrowIfNull(meshB);

        // Raeumliche Indizes einmal vorberechnen und wiederverwenden
        var indexB = new MeshSpatialIndex(meshB);
        var indexA = new MeshSpatialIndex(meshA);
        var result = new TriangleMesh();

        // Dreiecke von A behalten, die ausserhalb von B liegen
        CopyTrianglesOutside(meshA, indexB, result);
        // Dreiecke von B behalten, die ausserhalb von A liegen
        CopyTrianglesOutside(meshB, indexA, result);

        return result;
    }

    /// <summary>Differenz zweier Meshes: A - B.</summary>
    public static TriangleMesh Subtract(TriangleMesh meshA, TriangleMesh meshB)
    {
        ArgumentNullException.ThrowIfNull(meshA);
        ArgumentNullException.ThrowIfNull(meshB);

        // Raeumliche Indizes einmal vorberechnen und wiederverwenden
        var indexB = new MeshSpatialIndex(meshB);
        var indexA = new MeshSpatialIndex(meshA);
        var result = new TriangleMesh();

        // Dreiecke von A behalten, die ausserhalb von B liegen
        CopyTrianglesOutside(meshA, indexB, result);

        // Dreiecke von B, die innerhalb von A liegen, invertiert hinzufuegen
        CopyTrianglesInsideInverted(meshB, indexA, result);

        return result;
    }

    /// <summary>Schnittmenge zweier Meshes: A ∩ B.</summary>
    public static TriangleMesh Intersect(TriangleMesh meshA, TriangleMesh meshB)
    {
        ArgumentNullException.ThrowIfNull(meshA);
        ArgumentNullException.ThrowIfNull(meshB);

        // Raeumliche Indizes einmal vorberechnen und wiederverwenden
        var indexB = new MeshSpatialIndex(meshB);
        var indexA = new MeshSpatialIndex(meshA);
        var result = new TriangleMesh();

        // Dreiecke von A behalten, die innerhalb von B liegen
        CopyTrianglesInside(meshA, indexB, result);
        // Dreiecke von B behalten, die innerhalb von A liegen
        CopyTrianglesInside(meshB, indexA, result);

        return result;
    }

    private static void CopyTrianglesOutside(TriangleMesh source, MeshSpatialIndex referenceIndex, TriangleMesh target)
    {
        for (var i = 0; i < source.TriangleCount; i++)
        {
            var tri = source.GetTriangle(i);
            var centroid = tri.Centroid;

            // Raeumlicher Index prueft BBox und Majority-Vote intern
            if (!referenceIndex.IsPointInside(centroid))
            {
                AddTriangleToMesh(target, tri);
            }
        }
    }

    private static void CopyTrianglesInside(TriangleMesh source, MeshSpatialIndex referenceIndex, TriangleMesh target)
    {
        for (var i = 0; i < source.TriangleCount; i++)
        {
            var tri = source.GetTriangle(i);
            var centroid = tri.Centroid;

            if (referenceIndex.IsPointInside(centroid))
            {
                AddTriangleToMesh(target, tri);
            }
        }
    }

    private static void CopyTrianglesInsideInverted(TriangleMesh source, MeshSpatialIndex referenceIndex, TriangleMesh target)
    {
        for (var i = 0; i < source.TriangleCount; i++)
        {
            var tri = source.GetTriangle(i);
            var centroid = tri.Centroid;

            if (referenceIndex.IsPointInside(centroid))
            {
                // Windungsordnung umkehren (Normalen invertieren)
                AddTriangleToMesh(target, new Triangle3D(tri.V0, tri.V2, tri.V1));
            }
        }
    }

    /// <summary>
    /// Prueft, ob ein Punkt innerhalb eines geschlossenen Mesh liegt (Ray-Casting-Methode).
    /// Mehrere Strahlrichtungen werden getestet, um Probleme bei Kanten-Treffern zu vermeiden (Majority Vote).
    ///
    /// Hinweis: Fuer wiederholte Aufrufe gegen dasselbe Mesh ist MeshSpatialIndex.IsPointInside() deutlich schneller.
    /// Diese Methode bleibt fuer Einzeltests und Kompatibilitaet erhalten.
    /// </summary>
    internal static bool IsPointInsideMesh(Vector3D point, TriangleMesh mesh)
    {
        // Drei leicht unterschiedliche Strahlrichtungen fuer robusteres Ergebnis
        ReadOnlySpan<Vector3D> rayDirections =
        [
            new Vector3D(1.0, 0.00123, 0.00456),
            new Vector3D(0.00789, 1.0, 0.00234),
            new Vector3D(0.00567, 0.00891, 1.0)
        ];

        var insideVotes = 0;
        foreach (var rayDir in rayDirections)
        {
            var intersectionCount = 0;
            for (var i = 0; i < mesh.TriangleCount; i++)
            {
                var tri = mesh.GetTriangle(i);
                if (RayIntersectsTriangle(point, rayDir, tri))
                {
                    intersectionCount++;
                }
            }

            if (intersectionCount % 2 == 1)
                insideVotes++;
        }

        return insideVotes >= 2; // Majority vote
    }

    /// <summary>
    /// Moeller-Trumbore Ray-Triangle Intersection Test.
    /// </summary>
    internal static bool RayIntersectsTriangle(Vector3D rayOrigin, Vector3D rayDir, Triangle3D triangle)
    {
        const double epsilon = 1e-10;

        var edge1 = triangle.V1 - triangle.V0;
        var edge2 = triangle.V2 - triangle.V0;
        var h = rayDir.Cross(edge2);
        var a = edge1.Dot(h);

        if (System.Math.Abs(a) < epsilon)
            return false; // Strahl parallel zum Dreieck

        var f = 1.0 / a;
        var s = rayOrigin - triangle.V0;
        var u = f * s.Dot(h);

        if (u < 0.0 || u > 1.0)
            return false;

        var q = s.Cross(edge1);
        var v = f * rayDir.Dot(q);

        if (v < 0.0 || u + v > 1.0)
            return false;

        var t = f * edge2.Dot(q);
        return t > epsilon; // Schnitt nur in positiver Strahlrichtung
    }

    private static void AddTriangleToMesh(TriangleMesh mesh, Triangle3D tri)
    {
        var i0 = mesh.AddVertex(tri.V0);
        var i1 = mesh.AddVertex(tri.V1);
        var i2 = mesh.AddVertex(tri.V2);
        mesh.AddTriangle(i0, i1, i2);
    }
}
