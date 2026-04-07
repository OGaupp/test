using CadTool.Core.Math;
using CadTool.Core.Viewport;

namespace CadTool.Core.Tests.Viewport;

public class Camera3DTests
{
    [Fact]
    public void DefaultCamera_HasIsometricPosition()
    {
        var camera = new Camera3D();
        Assert.Equal(new Vector3D(100, 100, 100), camera.Position);
        Assert.Equal(Vector3D.Zero, camera.Target);
        Assert.Equal(Vector3D.UnitZ, camera.UpDirection);
    }

    [Fact]
    public void LookDirection_IsNormalized()
    {
        var camera = new Camera3D();
        var lookDir = camera.LookDirection;
        Assert.Equal(1.0, lookDir.Length, 1e-10);
    }

    [Fact]
    public void DistanceToTarget_Correct()
    {
        var camera = new Camera3D
        {
            Position = new Vector3D(10, 0, 0),
            Target = Vector3D.Zero
        };
        Assert.Equal(10.0, camera.DistanceToTarget, 1e-10);
    }

    [Fact]
    public void GetViewMatrix_LookingAlongNegativeZ()
    {
        var camera = new Camera3D
        {
            Position = new Vector3D(0, 0, 10),
            Target = Vector3D.Zero,
            UpDirection = Vector3D.UnitY
        };

        var viewMatrix = camera.GetViewMatrix();
        // Der Ursprung sollte bei (0, 0, -10) in der View-Matrix liegen
        var origin = viewMatrix.TransformPoint(Vector3D.Zero);
        Assert.Equal(0.0, origin.X, 1e-10);
        Assert.Equal(0.0, origin.Y, 1e-10);
        Assert.Equal(-10.0, origin.Z, 1e-10);
    }

    [Fact]
    public void ProjectionType_DefaultIsPerspective()
    {
        var camera = new Camera3D();
        Assert.Equal(ProjectionType.Perspective, camera.Projection);
    }
}
