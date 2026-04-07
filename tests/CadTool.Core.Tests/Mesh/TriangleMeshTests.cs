using CadTool.Core.Math;
using CadTool.Core.Mesh;

namespace CadTool.Core.Tests.Mesh;

public class TriangleMeshTests
{
    [Fact]
    public void AddVertex_ReturnsIncrementingIndex()
    {
        var mesh = new TriangleMesh();
        Assert.Equal(0, mesh.AddVertex(Vector3D.Zero));
        Assert.Equal(1, mesh.AddVertex(Vector3D.UnitX));
        Assert.Equal(2, mesh.AddVertex(Vector3D.UnitY));
    }

    [Fact]
    public void AddTriangle_IncrementsCount()
    {
        var mesh = CreateSimpleTriangleMesh();
        Assert.Equal(1, mesh.TriangleCount);
    }

    [Fact]
    public void AddTriangle_InvalidIndex_Throws()
    {
        var mesh = new TriangleMesh();
        mesh.AddVertex(Vector3D.Zero);
        mesh.AddVertex(Vector3D.UnitX);
        Assert.Throws<ArgumentOutOfRangeException>(() => mesh.AddTriangle(0, 1, 5));
    }

    [Fact]
    public void GetTriangle_ReturnsCorrectVertices()
    {
        var mesh = new TriangleMesh();
        var v0 = new Vector3D(0, 0, 0);
        var v1 = new Vector3D(1, 0, 0);
        var v2 = new Vector3D(0, 1, 0);
        mesh.AddVertex(v0);
        mesh.AddVertex(v1);
        mesh.AddVertex(v2);
        mesh.AddTriangle(0, 1, 2);

        var tri = mesh.GetTriangle(0);
        Assert.Equal(v0, tri.V0);
        Assert.Equal(v1, tri.V1);
        Assert.Equal(v2, tri.V2);
    }

    [Fact]
    public void GetBoundingBox_EmptyMesh_ReturnsZero()
    {
        var mesh = new TriangleMesh();
        var bbox = mesh.GetBoundingBox();
        Assert.Equal(Vector3D.Zero, bbox.Min);
        Assert.Equal(Vector3D.Zero, bbox.Max);
    }

    [Fact]
    public void GetBoundingBox_ContainsAllVertices()
    {
        var mesh = new TriangleMesh();
        mesh.AddVertex(new Vector3D(-5, -3, -1));
        mesh.AddVertex(new Vector3D(5, 3, 1));
        mesh.AddVertex(new Vector3D(0, 0, 0));
        mesh.AddTriangle(0, 1, 2);

        var bbox = mesh.GetBoundingBox();
        Assert.Equal(new Vector3D(-5, -3, -1), bbox.Min);
        Assert.Equal(new Vector3D(5, 3, 1), bbox.Max);
    }

    [Fact]
    public void CalculateVolume_UnitCube_IsOne()
    {
        var mesh = CreateUnitCubeMesh();
        var volume = mesh.CalculateVolume();
        Assert.Equal(1.0, volume, 1e-6);
    }

    [Fact]
    public void Clone_CreatesIndependentCopy()
    {
        var mesh = CreateSimpleTriangleMesh();
        var clone = mesh.Clone();

        Assert.Equal(mesh.VertexCount, clone.VertexCount);
        Assert.Equal(mesh.TriangleCount, clone.TriangleCount);

        // Aendere Original, Clone bleibt unberuehrt
        mesh.AddVertex(new Vector3D(99, 99, 99));
        Assert.NotEqual(mesh.VertexCount, clone.VertexCount);
    }

    [Fact]
    public void Transform_TranslatesAllVertices()
    {
        var mesh = CreateSimpleTriangleMesh();
        var offset = new Vector3D(10, 20, 30);
        mesh.Transform(Matrix4x4.CreateTranslation(offset));

        Assert.Equal(offset, mesh.Vertices[0]);
    }

    [Fact]
    public void InvertNormals_ReversesWindingOrder()
    {
        var mesh = CreateSimpleTriangleMesh();
        var originalTri = mesh.GetTriangle(0);

        mesh.InvertNormals();
        var invertedTri = mesh.GetTriangle(0);

        // V0 bleibt gleich, V1 und V2 werden getauscht
        Assert.Equal(originalTri.V0, invertedTri.V0);
        Assert.Equal(originalTri.V1, invertedTri.V2);
        Assert.Equal(originalTri.V2, invertedTri.V1);
    }

    private static TriangleMesh CreateSimpleTriangleMesh()
    {
        var mesh = new TriangleMesh();
        mesh.AddVertex(new Vector3D(0, 0, 0));
        mesh.AddVertex(new Vector3D(1, 0, 0));
        mesh.AddVertex(new Vector3D(0, 1, 0));
        mesh.AddTriangle(0, 1, 2);
        return mesh;
    }

    /// <summary>Erstellt einen Einheitswuerfel als geschlossenes Mesh (12 Dreiecke).</summary>
    private static TriangleMesh CreateUnitCubeMesh()
    {
        var mesh = new TriangleMesh();
        // 8 Eckpunkte (0,0,0) bis (1,1,1)
        mesh.AddVertex(new Vector3D(0, 0, 0)); // 0
        mesh.AddVertex(new Vector3D(1, 0, 0)); // 1
        mesh.AddVertex(new Vector3D(1, 1, 0)); // 2
        mesh.AddVertex(new Vector3D(0, 1, 0)); // 3
        mesh.AddVertex(new Vector3D(0, 0, 1)); // 4
        mesh.AddVertex(new Vector3D(1, 0, 1)); // 5
        mesh.AddVertex(new Vector3D(1, 1, 1)); // 6
        mesh.AddVertex(new Vector3D(0, 1, 1)); // 7

        // 12 Dreiecke (CCW von aussen)
        mesh.AddTriangle(0, 2, 1); mesh.AddTriangle(0, 3, 2); // Unten
        mesh.AddTriangle(4, 5, 6); mesh.AddTriangle(4, 6, 7); // Oben
        mesh.AddTriangle(0, 1, 5); mesh.AddTriangle(0, 5, 4); // Vorne
        mesh.AddTriangle(2, 3, 7); mesh.AddTriangle(2, 7, 6); // Hinten
        mesh.AddTriangle(0, 4, 7); mesh.AddTriangle(0, 7, 3); // Links
        mesh.AddTriangle(1, 2, 6); mesh.AddTriangle(1, 6, 5); // Rechts

        return mesh;
    }
}
