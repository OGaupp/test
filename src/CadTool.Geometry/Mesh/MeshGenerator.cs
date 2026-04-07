using CadTool.Core.Math;
using CadTool.Core.Mesh;
using CadTool.Core.Primitives;

namespace CadTool.Geometry.Mesh;

/// <summary>
/// Erzeugt Dreiecksnetze (TriangleMesh) aus geometrischen Grundkoerpern.
/// Die Aufloesung (Segmentanzahl) bestimmt die Genauigkeit der Approximation.
/// </summary>
public static class MeshGenerator
{
    private const int DefaultSegments = 24;

    /// <summary>Erzeugt ein Mesh fuer ein beliebiges Primitiv.</summary>
    public static TriangleMesh GenerateMesh(IPrimitive3D primitive, int segments = DefaultSegments)
    {
        ArgumentNullException.ThrowIfNull(primitive);

        var mesh = primitive.Type switch
        {
            PrimitiveType.Box => GenerateBox((BoxPrimitive)primitive),
            PrimitiveType.Sphere => GenerateSphere((SpherePrimitive)primitive, segments),
            PrimitiveType.Cylinder => GenerateCylinder((CylinderPrimitive)primitive, segments),
            PrimitiveType.Torus => GenerateTorus((TorusPrimitive)primitive, segments),
            _ => throw new ArgumentOutOfRangeException(nameof(primitive), $"Unbekannter Primitiv-Typ: {primitive.Type}")
        };

        mesh.Transform(primitive.Transform);
        return mesh;
    }

    /// <summary>Erzeugt ein Quader-Mesh (12 Dreiecke, 8 Vertices).</summary>
    public static TriangleMesh GenerateBox(BoxPrimitive box)
    {
        var mesh = new TriangleMesh();
        var hw = box.Width / 2.0;
        var hd = box.Depth / 2.0;
        var hh = box.Height / 2.0;

        // 8 Eckpunkte
        var v0 = mesh.AddVertex(new Vector3D(-hw, -hd, -hh));
        var v1 = mesh.AddVertex(new Vector3D(hw, -hd, -hh));
        var v2 = mesh.AddVertex(new Vector3D(hw, hd, -hh));
        var v3 = mesh.AddVertex(new Vector3D(-hw, hd, -hh));
        var v4 = mesh.AddVertex(new Vector3D(-hw, -hd, hh));
        var v5 = mesh.AddVertex(new Vector3D(hw, -hd, hh));
        var v6 = mesh.AddVertex(new Vector3D(hw, hd, hh));
        var v7 = mesh.AddVertex(new Vector3D(-hw, hd, hh));

        // 6 Flaechen, je 2 Dreiecke (CCW von aussen gesehen)
        // Unten (Z-)
        mesh.AddTriangle(v0, v2, v1);
        mesh.AddTriangle(v0, v3, v2);
        // Oben (Z+)
        mesh.AddTriangle(v4, v5, v6);
        mesh.AddTriangle(v4, v6, v7);
        // Vorne (Y-)
        mesh.AddTriangle(v0, v1, v5);
        mesh.AddTriangle(v0, v5, v4);
        // Hinten (Y+)
        mesh.AddTriangle(v2, v3, v7);
        mesh.AddTriangle(v2, v7, v6);
        // Links (X-)
        mesh.AddTriangle(v0, v4, v7);
        mesh.AddTriangle(v0, v7, v3);
        // Rechts (X+)
        mesh.AddTriangle(v1, v2, v6);
        mesh.AddTriangle(v1, v6, v5);

        return mesh;
    }

    /// <summary>Erzeugt ein Kugel-Mesh (UV-Sphere).</summary>
    public static TriangleMesh GenerateSphere(SpherePrimitive sphere, int segments = DefaultSegments)
    {
        var mesh = new TriangleMesh();
        var rings = segments / 2;
        var r = sphere.Radius;

        // Vertices: Nord- und Suedpol + Ring-Vertices
        var northPole = mesh.AddVertex(new Vector3D(0, 0, r));
        var vertexGrid = new int[rings - 1, segments];

        for (var ring = 1; ring < rings; ring++)
        {
            var phi = System.Math.PI * ring / rings;
            var sinPhi = System.Math.Sin(phi);
            var cosPhi = System.Math.Cos(phi);

            for (var seg = 0; seg < segments; seg++)
            {
                var theta = 2.0 * System.Math.PI * seg / segments;
                var x = r * sinPhi * System.Math.Cos(theta);
                var y = r * sinPhi * System.Math.Sin(theta);
                var z = r * cosPhi;
                vertexGrid[ring - 1, seg] = mesh.AddVertex(new Vector3D(x, y, z));
            }
        }

        var southPole = mesh.AddVertex(new Vector3D(0, 0, -r));

        // Dreiecke: Nordpol-Fan
        for (var seg = 0; seg < segments; seg++)
        {
            var next = (seg + 1) % segments;
            mesh.AddTriangle(northPole, vertexGrid[0, seg], vertexGrid[0, next]);
        }

        // Dreiecke: Ringe
        for (var ring = 0; ring < rings - 2; ring++)
        {
            for (var seg = 0; seg < segments; seg++)
            {
                var next = (seg + 1) % segments;
                mesh.AddTriangle(vertexGrid[ring, seg], vertexGrid[ring + 1, seg], vertexGrid[ring + 1, next]);
                mesh.AddTriangle(vertexGrid[ring, seg], vertexGrid[ring + 1, next], vertexGrid[ring, next]);
            }
        }

        // Dreiecke: Suedpol-Fan
        var lastRing = rings - 2;
        for (var seg = 0; seg < segments; seg++)
        {
            var next = (seg + 1) % segments;
            mesh.AddTriangle(vertexGrid[lastRing, seg], southPole, vertexGrid[lastRing, next]);
        }

        return mesh;
    }

    /// <summary>Erzeugt ein Zylinder-Mesh mit Deckel und Boden.</summary>
    public static TriangleMesh GenerateCylinder(CylinderPrimitive cylinder, int segments = DefaultSegments)
    {
        var mesh = new TriangleMesh();
        var r = cylinder.Radius;
        var hh = cylinder.Height / 2.0;

        // Mittelpunkte fuer Deckel und Boden
        var bottomCenter = mesh.AddVertex(new Vector3D(0, 0, -hh));
        var topCenter = mesh.AddVertex(new Vector3D(0, 0, hh));

        var bottomRing = new int[segments];
        var topRing = new int[segments];

        for (var i = 0; i < segments; i++)
        {
            var theta = 2.0 * System.Math.PI * i / segments;
            var x = r * System.Math.Cos(theta);
            var y = r * System.Math.Sin(theta);
            bottomRing[i] = mesh.AddVertex(new Vector3D(x, y, -hh));
            topRing[i] = mesh.AddVertex(new Vector3D(x, y, hh));
        }

        for (var i = 0; i < segments; i++)
        {
            var next = (i + 1) % segments;

            // Boden (CCW von unten)
            mesh.AddTriangle(bottomCenter, bottomRing[next], bottomRing[i]);
            // Deckel (CCW von oben)
            mesh.AddTriangle(topCenter, topRing[i], topRing[next]);
            // Mantelflaeche
            mesh.AddTriangle(bottomRing[i], bottomRing[next], topRing[next]);
            mesh.AddTriangle(bottomRing[i], topRing[next], topRing[i]);
        }

        return mesh;
    }

    /// <summary>Erzeugt ein Torus-Mesh.</summary>
    public static TriangleMesh GenerateTorus(TorusPrimitive torus, int segments = DefaultSegments)
    {
        var mesh = new TriangleMesh();
        var majorR = torus.MajorRadius;
        var minorR = torus.MinorRadius;
        var tubeSegments = segments;

        var vertexGrid = new int[segments, tubeSegments];

        for (var i = 0; i < segments; i++)
        {
            var theta = 2.0 * System.Math.PI * i / segments;
            var cosTheta = System.Math.Cos(theta);
            var sinTheta = System.Math.Sin(theta);

            for (var j = 0; j < tubeSegments; j++)
            {
                var phi = 2.0 * System.Math.PI * j / tubeSegments;
                var cosPhi = System.Math.Cos(phi);
                var sinPhi = System.Math.Sin(phi);

                var x = (majorR + minorR * cosPhi) * cosTheta;
                var y = (majorR + minorR * cosPhi) * sinTheta;
                var z = minorR * sinPhi;

                vertexGrid[i, j] = mesh.AddVertex(new Vector3D(x, y, z));
            }
        }

        for (var i = 0; i < segments; i++)
        {
            var nextI = (i + 1) % segments;
            for (var j = 0; j < tubeSegments; j++)
            {
                var nextJ = (j + 1) % tubeSegments;
                mesh.AddTriangle(vertexGrid[i, j], vertexGrid[nextI, j], vertexGrid[nextI, nextJ]);
                mesh.AddTriangle(vertexGrid[i, j], vertexGrid[nextI, nextJ], vertexGrid[i, nextJ]);
            }
        }

        return mesh;
    }
}
