using CadTool.Core.Math;
using CadTool.Core.Primitives;

namespace CadTool.Core.Tests.Primitives;

public class PrimitiveTests
{
    [Fact]
    public void BoxPrimitive_HasCorrectType()
    {
        var box = new BoxPrimitive(2, 3, 4);
        Assert.Equal(PrimitiveType.Box, box.Type);
    }

    [Fact]
    public void BoxPrimitive_BoundingBox_AtOrigin()
    {
        var box = new BoxPrimitive(2, 4, 6);
        var bbox = box.GetBoundingBox();
        Assert.Equal(new Vector3D(-1, -2, -3), bbox.Min);
        Assert.Equal(new Vector3D(1, 2, 3), bbox.Max);
    }

    [Fact]
    public void BoxPrimitive_InvalidDimensions_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new BoxPrimitive(0, 1, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => new BoxPrimitive(1, -1, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => new BoxPrimitive(1, 1, 0));
    }

    [Fact]
    public void SpherePrimitive_BoundingBox_AtOrigin()
    {
        var sphere = new SpherePrimitive(5);
        var bbox = sphere.GetBoundingBox();
        Assert.Equal(new Vector3D(-5, -5, -5), bbox.Min);
        Assert.Equal(new Vector3D(5, 5, 5), bbox.Max);
    }

    [Fact]
    public void SpherePrimitive_InvalidRadius_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new SpherePrimitive(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => new SpherePrimitive(-1));
    }

    [Fact]
    public void CylinderPrimitive_BoundingBox_AtOrigin()
    {
        var cyl = new CylinderPrimitive(3, 10);
        var bbox = cyl.GetBoundingBox();
        Assert.Equal(new Vector3D(-3, -3, -5), bbox.Min);
        Assert.Equal(new Vector3D(3, 3, 5), bbox.Max);
    }

    [Fact]
    public void CylinderPrimitive_InvalidParams_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new CylinderPrimitive(0, 5));
        Assert.Throws<ArgumentOutOfRangeException>(() => new CylinderPrimitive(5, 0));
    }

    [Fact]
    public void TorusPrimitive_BoundingBox_AtOrigin()
    {
        var torus = new TorusPrimitive(10, 2);
        var bbox = torus.GetBoundingBox();
        Assert.Equal(new Vector3D(-12, -12, -2), bbox.Min);
        Assert.Equal(new Vector3D(12, 12, 2), bbox.Max);
    }

    [Fact]
    public void TorusPrimitive_InvalidRadii_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new TorusPrimitive(0, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => new TorusPrimitive(5, 0));
        Assert.Throws<ArgumentException>(() => new TorusPrimitive(5, 5));
        Assert.Throws<ArgumentException>(() => new TorusPrimitive(5, 6));
    }

    [Fact]
    public void SpherePrimitive_Translated_BoundingBoxShifts()
    {
        var transform = Matrix4x4.CreateTranslation(new Vector3D(10, 0, 0));
        var sphere = new SpherePrimitive(2, transform);
        var bbox = sphere.GetBoundingBox();
        Assert.Equal(new Vector3D(8, -2, -2), bbox.Min);
        Assert.Equal(new Vector3D(12, 2, 2), bbox.Max);
    }

    [Fact]
    public void Primitives_HaveUniqueIds()
    {
        var a = new BoxPrimitive(1, 1, 1);
        var b = new BoxPrimitive(1, 1, 1);
        Assert.NotEqual(a.Id, b.Id);
    }
}
