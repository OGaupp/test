using CadTool.Core.Domain;
using CadTool.Core.Interfaces;
using CadTool.Core.Math;
using CadTool.Core.Primitives;
using CadTool.Geometry.BooleanOps;

namespace CadTool.Geometry.Tests.BooleanOps;

public class BooleanOperationServiceTests
{
    private readonly BooleanOperationService _sut = new();

    [Fact]
    public void Union_CreatesNamedResult()
    {
        var bodyA = new CadBody("A", new BoxPrimitive(2, 2, 2));
        var bodyB = new CadBody("B", new SpherePrimitive(1));

        var result = _sut.Execute(bodyA, bodyB, BooleanOperationType.Union);

        Assert.Contains("A", result.Name);
        Assert.Contains("B", result.Name);
        Assert.Contains("Union", result.Name);
    }

    [Fact]
    public void Subtract_CreatesNamedResult()
    {
        var bodyA = new CadBody("Block", new BoxPrimitive(10, 10, 10));
        var bodyB = new CadBody("Bohrung", new CylinderPrimitive(2, 12));

        var result = _sut.Execute(bodyA, bodyB, BooleanOperationType.Subtract);

        Assert.Contains("Subtract", result.Name);
    }

    [Fact]
    public void Intersect_NonOverlapping_Throws()
    {
        var bodyA = new CadBody("A", new BoxPrimitive(1, 1, 1));
        var bodyB = new CadBody("B", new BoxPrimitive(1, 1, 1,
            Matrix4x4.CreateTranslation(new Vector3D(100, 0, 0))));

        Assert.Throws<InvalidOperationException>(() =>
            _sut.Execute(bodyA, bodyB, BooleanOperationType.Intersect));
    }

    [Fact]
    public void Intersect_Overlapping_Succeeds()
    {
        var bodyA = new CadBody("A", new BoxPrimitive(4, 4, 4));
        var bodyB = new CadBody("B", new BoxPrimitive(4, 4, 4,
            Matrix4x4.CreateTranslation(new Vector3D(1, 0, 0))));

        var result = _sut.Execute(bodyA, bodyB, BooleanOperationType.Intersect);
        Assert.Contains("Intersect", result.Name);
    }

    [Fact]
    public void Execute_NullBody_Throws()
    {
        var body = new CadBody("A", new BoxPrimitive(1, 1, 1));
        Assert.Throws<ArgumentNullException>(() =>
            _sut.Execute(null!, body, BooleanOperationType.Union));
        Assert.Throws<ArgumentNullException>(() =>
            _sut.Execute(body, null!, BooleanOperationType.Union));
    }
}
