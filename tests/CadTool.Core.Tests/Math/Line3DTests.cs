using CadTool.Core.Math;

namespace CadTool.Core.Tests.Math;

public class Line3DTests
{
    [Fact]
    public void PointAt_ReturnsCorrectPoint()
    {
        var line = new Line3D(Vector3D.Zero, Vector3D.UnitX);
        Assert.Equal(new Vector3D(5, 0, 0), line.PointAt(5));
    }

    [Fact]
    public void FromTwoPoints_CreatesCorrectLine()
    {
        var a = new Vector3D(1, 0, 0);
        var b = new Vector3D(4, 0, 0);
        var line = Line3D.FromTwoPoints(a, b);
        Assert.Equal(a, line.Origin);
        Assert.Equal(new Vector3D(3, 0, 0), line.Direction);
    }

    [Fact]
    public void ClosestPointTo_PointOnLine()
    {
        var line = new Line3D(Vector3D.Zero, Vector3D.UnitX);
        var closest = line.ClosestPointTo(new Vector3D(5, 0, 0));
        Assert.Equal(new Vector3D(5, 0, 0), closest);
    }

    [Fact]
    public void ClosestPointTo_PointOffLine()
    {
        var line = new Line3D(Vector3D.Zero, Vector3D.UnitX);
        var closest = line.ClosestPointTo(new Vector3D(3, 4, 0));
        Assert.Equal(new Vector3D(3, 0, 0), closest);
    }

    [Fact]
    public void DistanceTo_PointOnLine_IsZero()
    {
        var line = new Line3D(Vector3D.Zero, Vector3D.UnitX);
        Assert.Equal(0.0, line.DistanceTo(new Vector3D(5, 0, 0)), 1e-10);
    }

    [Fact]
    public void DistanceTo_PointOffLine()
    {
        var line = new Line3D(Vector3D.Zero, Vector3D.UnitX);
        Assert.Equal(4.0, line.DistanceTo(new Vector3D(3, 4, 0)), 1e-10);
    }
}
