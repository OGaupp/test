using CadTool.Core.Math;
using CadTool.Core.Curves;
using CadTool.Geometry.Curves;

namespace CadTool.Geometry.Tests.Curves;

public class DxfCurveConverterTests
{
    [Fact]
    public void ConvertToLine_ReturnsCorrectEndpoints()
    {
        var start = new Vector3D(1, 2, 3);
        var end = new Vector3D(4, 5, 6);
        var line = DxfCurveConverter.ConvertToLine(start, end);

        Assert.Equal(CurveType.Line, line.Type);
        Assert.Equal(start, line.StartPoint);
        Assert.Equal(end, line.EndPoint);
    }

    [Fact]
    public void ConvertToPolyline_SetsPointsCorrectly()
    {
        var points = new[]
        {
            new Vector3D(0, 0, 0),
            new Vector3D(1, 0, 0),
            new Vector3D(1, 1, 0)
        };
        var polyline = DxfCurveConverter.ConvertToPolyline(points);

        Assert.Equal(CurveType.Polyline, polyline.Type);
        Assert.Equal(3, polyline.Points.Count);
        Assert.False(polyline.IsClosed);
    }

    [Fact]
    public void ConvertToPolyline_Closed()
    {
        var points = new[]
        {
            new Vector3D(0, 0, 0),
            new Vector3D(1, 0, 0),
            new Vector3D(1, 1, 0)
        };
        var polyline = DxfCurveConverter.ConvertToPolyline(points, isClosed: true);

        Assert.True(polyline.IsClosed);
        Assert.Equal(polyline.Points[0], polyline.EndPoint);
    }

    [Fact]
    public void ConvertToArc_DegreesToRadians()
    {
        var arc = DxfCurveConverter.ConvertToArc(
            Vector3D.Zero, 10.0, 0, 90);

        Assert.Equal(CurveType.Arc, arc.Type);
        Assert.Equal(0.0, arc.StartAngle, 1e-10);
        Assert.Equal(System.Math.PI / 2.0, arc.EndAngle, 1e-10);
    }

    [Fact]
    public void ConvertCircleToArc_FullCircle()
    {
        var arc = DxfCurveConverter.ConvertCircleToArc(Vector3D.Zero, 5.0);

        Assert.Equal(0.0, arc.StartAngle, 1e-10);
        Assert.Equal(2.0 * System.Math.PI, arc.EndAngle, 1e-10);
        Assert.Equal(5.0, arc.Radius);
    }

    [Fact]
    public void ConvertToSpline_CreatesValidSpline()
    {
        var controlPoints = new[]
        {
            new Vector3D(0, 0, 0),
            new Vector3D(1, 2, 0),
            new Vector3D(3, 2, 0),
            new Vector3D(4, 0, 0)
        };
        var spline = DxfCurveConverter.ConvertToSpline(controlPoints, 3);

        Assert.Equal(CurveType.Spline, spline.Type);
        Assert.Equal(4, spline.ControlPoints.Count);
        Assert.Equal(3, spline.Degree);
    }

    [Fact]
    public void ConvertToPolyline_InsufficientPoints_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            DxfCurveConverter.ConvertToPolyline(new[] { Vector3D.Zero }));
    }
}
