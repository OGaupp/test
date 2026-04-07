using CadTool.Core.Math;
using CadTool.Core.Viewport;
using CadTool.Geometry.Viewport;

namespace CadTool.Geometry.Tests.Viewport;

public class OrbitalCameraControllerTests
{
    [Fact]
    public void Orbit_ChangesPosition()
    {
        var camera = new Camera3D
        {
            Position = new Vector3D(100, 0, 0),
            Target = Vector3D.Zero
        };
        var controller = new OrbitalCameraController(camera);

        var originalPos = camera.Position;
        controller.Orbit(10, 0);

        Assert.NotEqual(originalPos, camera.Position);
    }

    [Fact]
    public void Orbit_PreservesDistance()
    {
        var camera = new Camera3D
        {
            Position = new Vector3D(100, 0, 0),
            Target = Vector3D.Zero
        };
        var controller = new OrbitalCameraController(camera);

        var originalDist = camera.DistanceToTarget;
        controller.Orbit(10, 5);

        Assert.Equal(originalDist, camera.DistanceToTarget, 1e-6);
    }

    [Fact]
    public void Pan_ShiftsPositionAndTarget()
    {
        var camera = new Camera3D
        {
            Position = new Vector3D(100, 0, 0),
            Target = Vector3D.Zero
        };
        var controller = new OrbitalCameraController(camera);

        var originalTarget = camera.Target;
        controller.Pan(100, 0);

        Assert.NotEqual(originalTarget, camera.Target);
    }

    [Fact]
    public void Pan_PreservesDistance()
    {
        var camera = new Camera3D
        {
            Position = new Vector3D(100, 0, 0),
            Target = Vector3D.Zero
        };
        var controller = new OrbitalCameraController(camera);

        var originalDist = camera.DistanceToTarget;
        controller.Pan(50, 30);

        Assert.Equal(originalDist, camera.DistanceToTarget, 1e-6);
    }

    [Fact]
    public void Zoom_ReducesDistance()
    {
        var camera = new Camera3D
        {
            Position = new Vector3D(100, 0, 0),
            Target = Vector3D.Zero
        };
        var controller = new OrbitalCameraController(camera);

        controller.Zoom(1); // Zoom in
        Assert.True(camera.DistanceToTarget < 100.0);
    }

    [Fact]
    public void Zoom_IncreasesDistance()
    {
        var camera = new Camera3D
        {
            Position = new Vector3D(100, 0, 0),
            Target = Vector3D.Zero
        };
        var controller = new OrbitalCameraController(camera);

        controller.Zoom(-1); // Zoom out
        Assert.True(camera.DistanceToTarget > 100.0);
    }

    [Fact]
    public void Zoom_RespectsMinDistance()
    {
        var camera = new Camera3D
        {
            Position = new Vector3D(1, 0, 0),
            Target = Vector3D.Zero
        };
        var controller = new OrbitalCameraController(camera) { MinDistance = 0.5 };

        for (var i = 0; i < 100; i++) controller.Zoom(1);

        Assert.True(camera.DistanceToTarget >= 0.5);
    }

    [Fact]
    public void ResetToIsometric_SetsKnownPosition()
    {
        var camera = new Camera3D
        {
            Position = new Vector3D(999, 999, 999),
            Target = new Vector3D(50, 50, 50)
        };
        var controller = new OrbitalCameraController(camera);

        controller.ResetToIsometric(100);

        Assert.Equal(Vector3D.Zero, camera.Target);
        Assert.Equal(100.0, camera.DistanceToTarget, 1e-4);
    }

    [Fact]
    public void SetStandardView_Front()
    {
        var camera = new Camera3D();
        var controller = new OrbitalCameraController(camera);

        controller.SetStandardView(StandardView.Front, 100);

        Assert.Equal(new Vector3D(0, -100, 0), camera.Position);
        Assert.Equal(Vector3D.Zero, camera.Target);
    }

    [Fact]
    public void SetStandardView_Top_SetsUpToY()
    {
        var camera = new Camera3D();
        var controller = new OrbitalCameraController(camera);

        controller.SetStandardView(StandardView.Top, 100);

        Assert.Equal(new Vector3D(0, 0, 100), camera.Position);
        Assert.Equal(Vector3D.UnitY, camera.UpDirection);
    }

    [Fact]
    public void NullCamera_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new OrbitalCameraController(null!));
    }
}
