using CadTool.Core.Math;
using CadTool.Core.Curves;

namespace CadTool.Core.Tests.Curves;

public class CurveTests
{
    [Fact]
    public void LineCurve_Length()
    {
        var line = new LineCurve3D(Vector3D.Zero, new Vector3D(3, 4, 0));
        Assert.Equal(5.0, line.Length, 1e-10);
    }

    [Fact]
    public void LineCurve_PointAtMidpoint()
    {
        var line = new LineCurve3D(Vector3D.Zero, new Vector3D(10, 0, 0));
        Assert.Equal(new Vector3D(5, 0, 0), line.PointAt(0.5));
    }

    [Fact]
    public void LineCurve_PointAtBounds()
    {
        var line = new LineCurve3D(Vector3D.Zero, new Vector3D(10, 0, 0));
        Assert.Equal(Vector3D.Zero, line.PointAt(0.0));
        Assert.Equal(new Vector3D(10, 0, 0), line.PointAt(1.0));
    }

    [Fact]
    public void LineCurve_PointAtClamped()
    {
        var line = new LineCurve3D(Vector3D.Zero, new Vector3D(10, 0, 0));
        Assert.Equal(Vector3D.Zero, line.PointAt(-1.0));
        Assert.Equal(new Vector3D(10, 0, 0), line.PointAt(2.0));
    }

    [Fact]
    public void ArcCurve_FullCircle_Length()
    {
        var arc = new ArcCurve3D(Vector3D.Zero, 5.0, Vector3D.UnitZ, 0, 2 * System.Math.PI);
        Assert.Equal(2 * System.Math.PI * 5.0, arc.Length, 1e-10);
    }

    [Fact]
    public void ArcCurve_QuarterCircle_Length()
    {
        var arc = new ArcCurve3D(Vector3D.Zero, 10.0, Vector3D.UnitZ, 0, System.Math.PI / 2);
        Assert.Equal(System.Math.PI * 10.0 / 2.0, arc.Length, 1e-10);
    }

    [Fact]
    public void ArcCurve_InvalidRadius_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ArcCurve3D(Vector3D.Zero, 0, Vector3D.UnitZ, 0, System.Math.PI));
    }

    [Fact]
    public void Polyline_Length_TwoSegments()
    {
        var points = new[]
        {
            new Vector3D(0, 0, 0),
            new Vector3D(3, 4, 0),
            new Vector3D(6, 4, 0)
        };
        var polyline = new PolylineCurve3D(points);
        Assert.Equal(8.0, polyline.Length, 1e-10); // 5 + 3
    }

    [Fact]
    public void Polyline_Closed_IncludesReturnSegment()
    {
        var points = new[]
        {
            new Vector3D(0, 0, 0),
            new Vector3D(3, 0, 0),
            new Vector3D(3, 4, 0)
        };
        var polyline = new PolylineCurve3D(points, isClosed: true);
        Assert.Equal(12.0, polyline.Length, 1e-10); // 3 + 4 + 5
    }

    [Fact]
    public void Polyline_InsufficientPoints_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            new PolylineCurve3D(new[] { Vector3D.Zero }));
    }

    [Fact]
    public void SplineCurve_StartAndEnd()
    {
        var controlPoints = new[]
        {
            new Vector3D(0, 0, 0),
            new Vector3D(1, 2, 0),
            new Vector3D(3, 2, 0),
            new Vector3D(4, 0, 0)
        };
        var spline = new SplineCurve3D(controlPoints, 3);

        Assert.Equal(new Vector3D(0, 0, 0), spline.PointAt(0));
        Assert.Equal(new Vector3D(4, 0, 0), spline.PointAt(1));
    }

    [Fact]
    public void SplineCurve_InsufficientControlPoints_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            new SplineCurve3D(new[] { Vector3D.Zero }, 1));
    }

    [Fact]
    public void SplineCurve_DegreeTooHigh_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            new SplineCurve3D(new[] { Vector3D.Zero, Vector3D.UnitX }, 2));
    }

    [Fact]
    public void LineCurve_Tessellate_CorrectCount()
    {
        var line = new LineCurve3D(Vector3D.Zero, Vector3D.UnitX);
        var points = line.Tessellate(10);
        Assert.Equal(11, points.Count);
    }

    [Fact]
    public void Polyline_Tessellate_ReturnsSamePoints()
    {
        var inputPoints = new[]
        {
            new Vector3D(0, 0, 0),
            new Vector3D(1, 0, 0),
            new Vector3D(2, 0, 0)
        };
        var polyline = new PolylineCurve3D(inputPoints);
        var tessellated = polyline.Tessellate();
        Assert.Equal(inputPoints.Length, tessellated.Count);
    }
}
