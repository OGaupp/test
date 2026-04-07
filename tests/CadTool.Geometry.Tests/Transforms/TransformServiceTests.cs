using CadTool.Core.Domain;
using CadTool.Core.Math;
using CadTool.Core.Primitives;
using CadTool.Geometry.Transforms;

namespace CadTool.Geometry.Tests.Transforms;

public class TransformServiceTests
{
    private readonly TransformService _sut = new();

    [Fact]
    public void MoveByPoints_TranslatesBody()
    {
        var box = new BoxPrimitive(2, 2, 2);
        var body = new CadBody("TestBox", box);

        _sut.MoveByPoints(body, Vector3D.Zero, new Vector3D(10, 0, 0));

        var center = body.WorldTransform.TransformPoint(Vector3D.Zero);
        Assert.Equal(new Vector3D(10, 0, 0), center);
    }

    [Fact]
    public void MoveByPoints_TwoSteps_Cumulative()
    {
        var box = new BoxPrimitive(1, 1, 1);
        var body = new CadBody("TestBox", box);

        _sut.MoveByPoints(body, Vector3D.Zero, new Vector3D(5, 0, 0));
        _sut.MoveByPoints(body, Vector3D.Zero, new Vector3D(0, 3, 0));

        var center = body.WorldTransform.TransformPoint(Vector3D.Zero);
        Assert.Equal(new Vector3D(5, 3, 0), center);
    }

    [Fact]
    public void MoveByPoints_NullBody_Throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
            _sut.MoveByPoints(null!, Vector3D.Zero, Vector3D.UnitX));
    }

    [Fact]
    public void RotateAroundAxis_90DegreesAroundZ()
    {
        var box = new BoxPrimitive(1, 1, 1, Matrix4x4.CreateTranslation(new Vector3D(1, 0, 0)));
        var body = new CadBody("TestBox", box);

        _sut.RotateAroundAxis(body, Vector3D.Zero, Vector3D.UnitZ, System.Math.PI / 2);

        var center = body.WorldTransform.TransformPoint(Vector3D.Zero);
        Assert.Equal(0.0, center.X, 1e-10);
        Assert.Equal(1.0, center.Y, 1e-10);
        Assert.Equal(0.0, center.Z, 1e-10);
    }

    [Fact]
    public void RotateAroundAxis_NullBody_Throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
            _sut.RotateAroundAxis(null!, Vector3D.Zero, Vector3D.UnitZ, 1.0));
    }

    [Fact]
    public void RotateAroundAxis_ZeroAxis_Throws()
    {
        var body = new CadBody("Test", new BoxPrimitive(1, 1, 1));
        Assert.Throws<ArgumentException>(() =>
            _sut.RotateAroundAxis(body, Vector3D.Zero, Vector3D.Zero, 1.0));
    }
}
