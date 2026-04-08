using CadTool.Core.Math;
using CadTool.Core.Mesh;
using CadTool.Core.Primitives;
using CadTool.Geometry.Mesh;

namespace CadTool.Geometry.Tests.Mesh;

public class MeshSpatialIndexTests
{
    [Fact]
    public void IsPointInside_InsideCube_ReturnsTrue()
    {
        var mesh = MeshGenerator.GenerateMesh(new BoxPrimitive(4, 4, 4));
        var index = CreateIndex(mesh);

        Assert.True(index.IsPointInside(new Vector3D(0.5, 0.5, 0.5)));
    }

    [Fact]
    public void IsPointInside_OutsideCube_ReturnsFalse()
    {
        var mesh = MeshGenerator.GenerateMesh(new BoxPrimitive(4, 4, 4));
        var index = CreateIndex(mesh);

        Assert.False(index.IsPointInside(new Vector3D(10, 10, 10)));
    }

    [Fact]
    public void IsPointInside_Origin_ReturnsTrue()
    {
        var mesh = MeshGenerator.GenerateMesh(new BoxPrimitive(10, 10, 10));
        var index = CreateIndex(mesh);

        Assert.True(index.IsPointInside(Vector3D.Zero));
    }

    [Fact]
    public void IsPointInside_ConsistentWithBruteForce()
    {
        var mesh = MeshGenerator.GenerateMesh(new BoxPrimitive(10, 10, 10));
        var index = CreateIndex(mesh);

        // Verschiedene Testpunkte: innen und aussen
        Vector3D[] testPoints =
        [
            new(0, 0, 0),       // Zentrum
            new(4, 0, 0),       // Knapp innen
            new(0, 4.5, 0),     // Knapp innen
            new(0, 0, -4),      // Knapp innen
            new(6, 0, 0),       // Ausserhalb
            new(0, 6, 0),       // Ausserhalb
            new(0, 0, 6),       // Ausserhalb
            new(10, 10, 10),    // Weit ausserhalb
            new(-3, -3, -3),    // Innen (Ecke)
            new(1, 2, 3),       // Innen (beliebig)
        ];

        foreach (var point in testPoints)
        {
            var bruteForce = MeshBooleanOperations.IsPointInsideMesh(point, mesh);
            var spatial = index.IsPointInside(point);
            Assert.Equal(bruteForce, spatial);
        }
    }

    [Fact]
    public void IsPointInside_Sphere_ConsistentWithBruteForce()
    {
        var mesh = MeshGenerator.GenerateMesh(new SpherePrimitive(5), 16);
        var index = CreateIndex(mesh);

        Vector3D[] testPoints =
        [
            new(0, 0, 0),       // Zentrum
            new(3, 0, 0),       // Innen
            new(0, -3, 0),      // Innen
            new(0, 0, 3),       // Innen
            new(6, 0, 0),       // Ausserhalb
            new(0, 6, 0),       // Ausserhalb
            new(0, 0, 6),       // Ausserhalb
            new(2, 2, 2),       // Innen (Diagonale)
            new(4, 4, 4),       // Ausserhalb (Diagonale)
        ];

        foreach (var point in testPoints)
        {
            var bruteForce = MeshBooleanOperations.IsPointInsideMesh(point, mesh);
            var spatial = index.IsPointInside(point);
            Assert.Equal(bruteForce, spatial);
        }
    }

    [Fact]
    public void IsPointInside_TranslatedMesh_ConsistentWithBruteForce()
    {
        var mesh = MeshGenerator.GenerateMesh(
            new BoxPrimitive(4, 4, 4, Matrix4x4.CreateTranslation(new Vector3D(10, 0, 0))));
        var index = CreateIndex(mesh);

        Vector3D[] testPoints =
        [
            new(10, 0, 0),      // Zentrum des verschobenen Quaders
            new(11, 1, 1),      // Innen
            new(0, 0, 0),       // Ursprung (weit entfernt)
            new(13, 0, 0),      // Ausserhalb (rechts)
        ];

        foreach (var point in testPoints)
        {
            var bruteForce = MeshBooleanOperations.IsPointInsideMesh(point, mesh);
            var spatial = index.IsPointInside(point);
            Assert.Equal(bruteForce, spatial);
        }
    }

    [Fact]
    public void Union_WithSpatialIndex_ProducesSameResult()
    {
        var meshA = MeshGenerator.GenerateMesh(new BoxPrimitive(10, 10, 10));
        var meshB = MeshGenerator.GenerateMesh(new BoxPrimitive(4, 4, 4));

        var result = MeshBooleanOperations.Union(meshA, meshB);

        // Alle Dreiecke von A sind aussen (B liegt komplett in A) → alle A-Dreiecke bleiben
        // Alle Dreiecke von B sind innen → keine B-Dreiecke
        Assert.Equal(meshA.TriangleCount, result.TriangleCount);
    }

    [Fact]
    public void Subtract_WithSpatialIndex_ProducesSameResult()
    {
        var meshA = MeshGenerator.GenerateMesh(new BoxPrimitive(10, 10, 10));
        var meshB = MeshGenerator.GenerateMesh(new BoxPrimitive(4, 4, 4));

        var result = MeshBooleanOperations.Subtract(meshA, meshB);

        // A behält alle Dreiecke (B-Schwerpunkte sind innen, also B-Dreiecke bleiben in A)
        // B-Dreiecke innerhalb A werden invertiert hinzugefuegt
        Assert.True(result.TriangleCount > 0);
        Assert.True(result.TriangleCount > meshA.TriangleCount);
    }

    [Fact]
    public void Intersect_WithSpatialIndex_ProducesSameResult()
    {
        var meshA = MeshGenerator.GenerateMesh(new BoxPrimitive(10, 10, 10));
        var meshB = MeshGenerator.GenerateMesh(new BoxPrimitive(4, 4, 4));

        var result = MeshBooleanOperations.Intersect(meshA, meshB);

        // B liegt komplett in A → alle B-Dreiecke im Ergebnis, plus A-Dreiecke die in B liegen
        Assert.True(result.TriangleCount > 0);
    }

    [Fact]
    public void EmptyMesh_DoesNotThrow()
    {
        var emptyMesh = new TriangleMesh();
        var index = CreateIndex(emptyMesh);

        Assert.False(index.IsPointInside(Vector3D.Zero));
        Assert.False(index.IsPointInside(new Vector3D(1, 1, 1)));
    }

    [Fact]
    public void Null_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => CreateIndex(null!));
    }

    [Fact]
    public void CountRayIntersections_ThroughCube_ReturnsEvenCount()
    {
        var mesh = MeshGenerator.GenerateMesh(new BoxPrimitive(4, 4, 4));
        var index = CreateIndex(mesh);

        // Strahl von ausserhalb durch den gesamten Quader
        // Gerade Anzahl → Punkt liegt ausserhalb (korrekt fuer Startpunkt ausserhalb)
        var count = index.CountRayIntersections(new Vector3D(-10, 0, 0), Vector3D.UnitX);
        Assert.True(count > 0, "Strahl muss den Quader treffen.");
        Assert.True(count % 2 == 0, "Gerade Schnittanzahl erwartet (Punkt ausserhalb).");
    }

    [Fact]
    public void CountRayIntersections_MissesCube_ReturnsZero()
    {
        var mesh = MeshGenerator.GenerateMesh(new BoxPrimitive(4, 4, 4));
        var index = CreateIndex(mesh);

        // Strahl verfehlt den Quader komplett
        var count = index.CountRayIntersections(new Vector3D(-10, 10, 10), Vector3D.UnitX);
        Assert.Equal(0, count);
    }

    /// <summary>Erzeugt einen MeshSpatialIndex (internal-Klasse, sichtbar via InternalsVisibleTo).</summary>
    private static MeshSpatialIndex CreateIndex(TriangleMesh mesh)
    {
        return new MeshSpatialIndex(mesh);
    }
}
