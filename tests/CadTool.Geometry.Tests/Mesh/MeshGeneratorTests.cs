using CadTool.Core.Math;
using CadTool.Core.Primitives;
using CadTool.Geometry.Mesh;

namespace CadTool.Geometry.Tests.Mesh;

public class MeshGeneratorTests
{
    [Fact]
    public void GenerateBox_Has12Triangles()
    {
        var box = new BoxPrimitive(2, 3, 4);
        var mesh = MeshGenerator.GenerateBox(box);
        Assert.Equal(12, mesh.TriangleCount);
        Assert.Equal(8, mesh.VertexCount);
    }

    [Fact]
    public void GenerateBox_ClosedMesh_HasPositiveVolume()
    {
        var box = new BoxPrimitive(2, 3, 4);
        var mesh = MeshGenerator.GenerateBox(box);
        var volume = mesh.CalculateVolume();
        Assert.True(volume > 0, "Box-Mesh muss ein positives Volumen haben.");
        Assert.Equal(24.0, volume, 1e-4); // 2 * 3 * 4
    }

    [Fact]
    public void GenerateSphere_HasExpectedTriangleCount()
    {
        var sphere = new SpherePrimitive(5);
        var mesh = MeshGenerator.GenerateSphere(sphere, 12);
        Assert.True(mesh.TriangleCount > 0);
        // UV-Sphere: 2 * segments (Pole) + 2 * segments * (rings-2) (Mitte)
        // Bei 12 Segmenten, 6 Ringen: 2*12 + 2*12*4 = 24 + 96 = 120
        Assert.Equal(120, mesh.TriangleCount);
    }

    [Fact]
    public void GenerateCylinder_HasExpectedTriangleCount()
    {
        var cyl = new CylinderPrimitive(3, 10);
        var mesh = MeshGenerator.GenerateCylinder(cyl, 12);
        // Pro Segment: 1 Boden + 1 Deckel + 2 Mantel = 4
        Assert.Equal(48, mesh.TriangleCount);
    }

    [Fact]
    public void GenerateTorus_HasExpectedTriangleCount()
    {
        var torus = new TorusPrimitive(10, 2);
        var mesh = MeshGenerator.GenerateTorus(torus, 12);
        // segments * tubeSegments * 2
        Assert.Equal(288, mesh.TriangleCount);
    }

    [Fact]
    public void GenerateMesh_AppliesTransform()
    {
        var offset = new Vector3D(10, 0, 0);
        var box = new BoxPrimitive(2, 2, 2, Matrix4x4.CreateTranslation(offset));
        var mesh = MeshGenerator.GenerateMesh(box);

        var bbox = mesh.GetBoundingBox();
        Assert.Equal(new Vector3D(9, -1, -1), bbox.Min);
        Assert.Equal(new Vector3D(11, 1, 1), bbox.Max);
    }

    [Fact]
    public void GenerateMesh_NullPrimitive_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => MeshGenerator.GenerateMesh(null!));
    }
}
