using CadTool.Core.Math;

namespace CadTool.Core.Tests.Math;

public class Plane3DTests
{
    [Fact]
    public void FromPointAndNormal_XYPlane()
    {
        var plane = Plane3D.FromPointAndNormal(Vector3D.Zero, Vector3D.UnitZ);
        Assert.Equal(Vector3D.UnitZ, plane.Normal);
        Assert.Equal(0.0, plane.Distance, 1e-10);
    }

    [Fact]
    public void SignedDistance_PointAbovePlane_Positive()
    {
        var plane = Plane3D.FromPointAndNormal(Vector3D.Zero, Vector3D.UnitZ);
        var point = new Vector3D(0, 0, 5);
        Assert.Equal(5.0, plane.SignedDistanceTo(point), 1e-10);
    }

    [Fact]
    public void SignedDistance_PointBelowPlane_Negative()
    {
        var plane = Plane3D.FromPointAndNormal(Vector3D.Zero, Vector3D.UnitZ);
        var point = new Vector3D(0, 0, -3);
        Assert.Equal(-3.0, plane.SignedDistanceTo(point), 1e-10);
    }

    [Fact]
    public void ProjectPoint_OntoXYPlane()
    {
        var plane = Plane3D.FromPointAndNormal(Vector3D.Zero, Vector3D.UnitZ);
        var point = new Vector3D(5, 3, 10);
        var projected = plane.ProjectPoint(point);
        Assert.Equal(new Vector3D(5, 3, 0), projected);
    }

    [Fact]
    public void FromThreePoints_CreatesCorrectPlane()
    {
        var a = new Vector3D(0, 0, 0);
        var b = new Vector3D(1, 0, 0);
        var c = new Vector3D(0, 1, 0);
        var plane = Plane3D.FromThreePoints(a, b, c);
        Assert.Equal(Vector3D.UnitZ, plane.Normal);
    }

    [Fact]
    public void FromThreePoints_CollinearPoints_Throws()
    {
        var a = new Vector3D(0, 0, 0);
        var b = new Vector3D(1, 0, 0);
        var c = new Vector3D(2, 0, 0);
        Assert.Throws<ArgumentException>(() => Plane3D.FromThreePoints(a, b, c));
    }
}
