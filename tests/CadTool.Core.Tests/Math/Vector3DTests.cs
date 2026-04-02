using CadTool.Core.Math;

namespace CadTool.Core.Tests.Math;

public class Vector3DTests
{
    [Fact]
    public void Constructor_SetsComponents()
    {
        var v = new Vector3D(1.0, 2.0, 3.0);
        Assert.Equal(1.0, v.X);
        Assert.Equal(2.0, v.Y);
        Assert.Equal(3.0, v.Z);
    }

    [Fact]
    public void Zero_IsOrigin()
    {
        var zero = Vector3D.Zero;
        Assert.Equal(0.0, zero.X);
        Assert.Equal(0.0, zero.Y);
        Assert.Equal(0.0, zero.Z);
    }

    [Fact]
    public void Length_OfUnitVector_IsOne()
    {
        Assert.Equal(1.0, Vector3D.UnitX.Length, 1e-10);
        Assert.Equal(1.0, Vector3D.UnitY.Length, 1e-10);
        Assert.Equal(1.0, Vector3D.UnitZ.Length, 1e-10);
    }

    [Fact]
    public void Length_Of345_Is5()
    {
        var v = new Vector3D(3, 4, 0);
        Assert.Equal(5.0, v.Length, 1e-10);
    }

    [Fact]
    public void Normalized_ReturnsUnitLength()
    {
        var v = new Vector3D(3, 4, 0);
        var n = v.Normalized();
        Assert.Equal(1.0, n.Length, 1e-10);
        Assert.Equal(0.6, n.X, 1e-10);
        Assert.Equal(0.8, n.Y, 1e-10);
    }

    [Fact]
    public void Normalized_ZeroVector_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => Vector3D.Zero.Normalized());
    }

    [Fact]
    public void DotProduct_Orthogonal_IsZero()
    {
        Assert.Equal(0.0, Vector3D.UnitX.Dot(Vector3D.UnitY), 1e-10);
    }

    [Fact]
    public void DotProduct_Parallel_IsProduct()
    {
        var a = new Vector3D(2, 0, 0);
        var b = new Vector3D(3, 0, 0);
        Assert.Equal(6.0, a.Dot(b), 1e-10);
    }

    [Fact]
    public void CrossProduct_XcrossY_IsZ()
    {
        var result = Vector3D.UnitX.Cross(Vector3D.UnitY);
        Assert.Equal(Vector3D.UnitZ, result);
    }

    [Fact]
    public void CrossProduct_YcrossX_IsNegZ()
    {
        var result = Vector3D.UnitY.Cross(Vector3D.UnitX);
        Assert.Equal(-Vector3D.UnitZ, result);
    }

    [Fact]
    public void Addition_Works()
    {
        var a = new Vector3D(1, 2, 3);
        var b = new Vector3D(4, 5, 6);
        Assert.Equal(new Vector3D(5, 7, 9), a + b);
    }

    [Fact]
    public void Subtraction_Works()
    {
        var a = new Vector3D(5, 7, 9);
        var b = new Vector3D(1, 2, 3);
        Assert.Equal(new Vector3D(4, 5, 6), a - b);
    }

    [Fact]
    public void ScalarMultiplication_Works()
    {
        var v = new Vector3D(1, 2, 3);
        Assert.Equal(new Vector3D(2, 4, 6), v * 2);
        Assert.Equal(new Vector3D(2, 4, 6), 2 * v);
    }

    [Fact]
    public void DistanceTo_SamePoint_IsZero()
    {
        var p = new Vector3D(1, 2, 3);
        Assert.Equal(0.0, p.DistanceTo(p), 1e-10);
    }

    [Fact]
    public void DistanceTo_KnownPoints()
    {
        var a = new Vector3D(0, 0, 0);
        var b = new Vector3D(3, 4, 0);
        Assert.Equal(5.0, a.DistanceTo(b), 1e-10);
    }

    [Fact]
    public void Equality_WithinTolerance()
    {
        var a = new Vector3D(1.0, 2.0, 3.0);
        var b = new Vector3D(1.0 + 1e-11, 2.0, 3.0);
        Assert.Equal(a, b);
    }

    [Fact]
    public void Inequality_OutsideTolerance()
    {
        var a = new Vector3D(1.0, 2.0, 3.0);
        var b = new Vector3D(1.1, 2.0, 3.0);
        Assert.NotEqual(a, b);
    }

    [Fact]
    public void Negation_Works()
    {
        var v = new Vector3D(1, -2, 3);
        Assert.Equal(new Vector3D(-1, 2, -3), -v);
    }
}
