using CadTool.Core.Math;
using CadTool.Core.Mesh;

namespace CadTool.Core.Tests.Mesh;

public class Triangle3DTests
{
    [Fact]
    public void Normal_RightHandRule()
    {
        var tri = new Triangle3D(Vector3D.Zero, Vector3D.UnitX, Vector3D.UnitY);
        var normal = tri.UnitNormal;
        Assert.Equal(Vector3D.UnitZ, normal);
    }

    [Fact]
    public void Area_UnitTriangle()
    {
        var tri = new Triangle3D(Vector3D.Zero, Vector3D.UnitX, Vector3D.UnitY);
        Assert.Equal(0.5, tri.Area, 1e-10);
    }

    [Fact]
    public void Centroid_IsAverageOfVertices()
    {
        var tri = new Triangle3D(
            new Vector3D(0, 0, 0),
            new Vector3D(3, 0, 0),
            new Vector3D(0, 3, 0));
        Assert.Equal(new Vector3D(1, 1, 0), tri.Centroid);
    }
}
