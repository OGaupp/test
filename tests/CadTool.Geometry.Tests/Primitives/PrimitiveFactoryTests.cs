using CadTool.Core.Math;
using CadTool.Core.Primitives;
using CadTool.Geometry.Primitives;

namespace CadTool.Geometry.Tests.Primitives;

public class PrimitiveFactoryTests
{
    [Fact]
    public void CreateBox_AtOrigin()
    {
        var box = PrimitiveFactory.CreateBox(2, 3, 4);
        Assert.Equal(PrimitiveType.Box, box.Type);
        Assert.Equal(2.0, box.Width);
        Assert.Equal(3.0, box.Depth);
        Assert.Equal(4.0, box.Height);
    }

    [Fact]
    public void CreateBox_AtPosition()
    {
        var pos = new Vector3D(10, 20, 30);
        var box = PrimitiveFactory.CreateBox(1, 1, 1, pos);
        var center = box.Transform.TransformPoint(Vector3D.Zero);
        Assert.Equal(pos, center);
    }

    [Fact]
    public void CreateSphere_AtOrigin()
    {
        var sphere = PrimitiveFactory.CreateSphere(5);
        Assert.Equal(PrimitiveType.Sphere, sphere.Type);
        Assert.Equal(5.0, sphere.Radius);
    }

    [Fact]
    public void CreateCylinder_AtOrigin()
    {
        var cyl = PrimitiveFactory.CreateCylinder(3, 10);
        Assert.Equal(PrimitiveType.Cylinder, cyl.Type);
        Assert.Equal(3.0, cyl.Radius);
        Assert.Equal(10.0, cyl.Height);
    }

    [Fact]
    public void CreateTorus_AtOrigin()
    {
        var torus = PrimitiveFactory.CreateTorus(10, 2);
        Assert.Equal(PrimitiveType.Torus, torus.Type);
        Assert.Equal(10.0, torus.MajorRadius);
        Assert.Equal(2.0, torus.MinorRadius);
    }

    [Fact]
    public void CreateSphere_AtPosition()
    {
        var pos = new Vector3D(5, 5, 5);
        var sphere = PrimitiveFactory.CreateSphere(3, pos);
        var center = sphere.Transform.TransformPoint(Vector3D.Zero);
        Assert.Equal(pos, center);
    }
}
