using CadTool.Core.Math;

namespace CadTool.Core.Tests.Math;

public class Matrix4x4Tests
{
    [Fact]
    public void Identity_TransformPoint_ReturnsUnchanged()
    {
        var p = new Vector3D(1, 2, 3);
        Assert.Equal(p, Matrix4x4.Identity.TransformPoint(p));
    }

    [Fact]
    public void Translation_MovesPoint()
    {
        var t = Matrix4x4.CreateTranslation(new Vector3D(10, 20, 30));
        var result = t.TransformPoint(new Vector3D(1, 2, 3));
        Assert.Equal(new Vector3D(11, 22, 33), result);
    }

    [Fact]
    public void Translation_DoesNotAffectDirection()
    {
        var t = Matrix4x4.CreateTranslation(new Vector3D(10, 20, 30));
        var dir = new Vector3D(1, 0, 0);
        Assert.Equal(dir, t.TransformDirection(dir));
    }

    [Fact]
    public void Scale_ScalesPoint()
    {
        var s = Matrix4x4.CreateScale(2.0);
        var result = s.TransformPoint(new Vector3D(1, 2, 3));
        Assert.Equal(new Vector3D(2, 4, 6), result);
    }

    [Fact]
    public void RotationZ_90Degrees_RotatesXToY()
    {
        var r = Matrix4x4.CreateRotationZ(System.Math.PI / 2);
        var result = r.TransformPoint(Vector3D.UnitX);
        Assert.Equal(0.0, result.X, 1e-10);
        Assert.Equal(1.0, result.Y, 1e-10);
        Assert.Equal(0.0, result.Z, 1e-10);
    }

    [Fact]
    public void RotationX_90Degrees_RotatesYToZ()
    {
        var r = Matrix4x4.CreateRotationX(System.Math.PI / 2);
        var result = r.TransformPoint(Vector3D.UnitY);
        Assert.Equal(0.0, result.X, 1e-10);
        Assert.Equal(0.0, result.Y, 1e-10);
        Assert.Equal(1.0, result.Z, 1e-10);
    }

    [Fact]
    public void RotationY_90Degrees_RotatesZToX()
    {
        var r = Matrix4x4.CreateRotationY(System.Math.PI / 2);
        var result = r.TransformPoint(Vector3D.UnitZ);
        Assert.Equal(1.0, result.X, 1e-10);
        Assert.Equal(0.0, result.Y, 1e-10);
        Assert.Equal(0.0, result.Z, 1e-10);
    }

    [Fact]
    public void RotationAxis_AroundZ_SameAsRotationZ()
    {
        var angle = System.Math.PI / 4;
        var r1 = Matrix4x4.CreateRotationZ(angle);
        var r2 = Matrix4x4.CreateRotationAxis(Vector3D.UnitZ, angle);

        var p = new Vector3D(1, 0, 0);
        var result1 = r1.TransformPoint(p);
        var result2 = r2.TransformPoint(p);
        Assert.Equal(result1, result2);
    }

    [Fact]
    public void Multiplication_Identity_ReturnsOriginal()
    {
        var t = Matrix4x4.CreateTranslation(new Vector3D(1, 2, 3));
        Assert.Equal(t, t * Matrix4x4.Identity);
        Assert.Equal(t, Matrix4x4.Identity * t);
    }

    [Fact]
    public void Multiplication_TranslationThenScale()
    {
        var t = Matrix4x4.CreateTranslation(new Vector3D(1, 0, 0));
        var s = Matrix4x4.CreateScale(2.0);
        var combined = s * t; // Erst Translation, dann Scale
        var result = combined.TransformPoint(Vector3D.Zero);
        Assert.Equal(new Vector3D(2, 0, 0), result);
    }

    [Fact]
    public void Equality_IdenticalMatrices()
    {
        Assert.Equal(Matrix4x4.Identity, Matrix4x4.Identity);
    }
}
