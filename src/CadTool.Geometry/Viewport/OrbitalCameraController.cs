using CadTool.Core.Math;
using CadTool.Core.Viewport;

namespace CadTool.Geometry.Viewport;

/// <summary>
/// Orbitale Kamera-Steuerung im AutoCAD-Style.
///
/// Maus-Mapping:
/// - Mittlere Maustaste (Drag): Pan
/// - Shift + Mittlere Maustaste (Drag): Orbit
/// - Mausrad: Zoom
///
/// Die Steuerung arbeitet rein mathematisch auf der Camera3D-Abstraktion
/// und hat keine Abhaengigkeit zu einem konkreten UI-Framework.
/// </summary>
public sealed class OrbitalCameraController
{
    private readonly Camera3D _camera;

    /// <summary>Empfindlichkeit der Orbit-Rotation (Grad pro Pixel).</summary>
    public double OrbitSensitivity { get; set; } = 0.5;

    /// <summary>Empfindlichkeit der Pan-Verschiebung (Einheiten pro Pixel).</summary>
    public double PanSensitivity { get; set; } = 0.5;

    /// <summary>Zoom-Faktor pro Mausrad-Schritt.</summary>
    public double ZoomFactor { get; set; } = 1.1;

    /// <summary>Minimaler Abstand zum Ziel (verhindert Kamera-Flip).</summary>
    public double MinDistance { get; set; } = 0.1;

    /// <summary>Maximaler Abstand zum Ziel.</summary>
    public double MaxDistance { get; set; } = 100000.0;

    public OrbitalCameraController(Camera3D camera)
    {
        _camera = camera ?? throw new ArgumentNullException(nameof(camera));
    }

    /// <summary>
    /// Orbits die Kamera um das Ziel.
    /// deltaX: Horizontale Mausbewegung (Pixel), deltaY: Vertikale Mausbewegung (Pixel).
    /// </summary>
    public void Orbit(double deltaX, double deltaY)
    {
        var offset = _camera.Position - _camera.Target;
        var distance = offset.Length;

        if (distance < 1e-10) return;

        // Azimuth (horizontale Rotation um Z-Achse)
        var azimuthAngle = -deltaX * OrbitSensitivity * System.Math.PI / 180.0;
        var rotationZ = Matrix4x4.CreateRotationZ(azimuthAngle);
        offset = rotationZ.TransformDirection(offset);

        // Elevation (vertikale Rotation um die lokale Rechts-Achse)
        var elevationAngle = -deltaY * OrbitSensitivity * System.Math.PI / 180.0;
        var forward = (-offset).Normalized();
        var right = forward.Cross(_camera.UpDirection);
        if (right.LengthSquared < 1e-10) return; // Singularitaet vermeiden
        right = right.Normalized();

        var rotationRight = Matrix4x4.CreateRotationAxis(right, elevationAngle);
        var newOffset = rotationRight.TransformDirection(offset);

        // Verhindern, dass die Kamera den Pol ueberquert (Gimbal Lock)
        var newForward = (-newOffset).Normalized();
        var dotUp = System.Math.Abs(newForward.Dot(_camera.UpDirection));
        if (dotUp > 0.99) return; // Zu nah am Pol

        _camera.Position = _camera.Target + newOffset;
    }

    /// <summary>
    /// Verschiebt Kamera und Ziel in der Bildebene.
    /// deltaX: Horizontale Mausbewegung (Pixel), deltaY: Vertikale Mausbewegung (Pixel).
    /// </summary>
    public void Pan(double deltaX, double deltaY)
    {
        var forward = _camera.LookDirection;
        var right = forward.Cross(_camera.UpDirection);
        if (right.LengthSquared < 1e-10) return;
        right = right.Normalized();
        var up = right.Cross(forward).Normalized();

        // Skalierung basierend auf Abstand zum Ziel
        var scaleFactor = _camera.DistanceToTarget * PanSensitivity * 0.001;
        var translation = right * (-deltaX * scaleFactor) + up * (deltaY * scaleFactor);

        _camera.Position = _camera.Position + translation;
        _camera.Target = _camera.Target + translation;
    }

    /// <summary>
    /// Zoomt die Kamera (bewegt sie naeher/weiter vom Ziel weg).
    /// delta: Positiv = hineinzoomen, negativ = herauszoomen.
    /// </summary>
    public void Zoom(double delta)
    {
        var offset = _camera.Position - _camera.Target;
        var distance = offset.Length;

        var factor = delta > 0 ? 1.0 / ZoomFactor : ZoomFactor;
        var newDistance = distance * factor;

        // Clamping
        newDistance = System.Math.Max(MinDistance, System.Math.Min(MaxDistance, newDistance));

        _camera.Position = _camera.Target + offset.Normalized() * newDistance;

        // Orthographische Breite anpassen
        if (_camera.Projection == ProjectionType.Orthographic)
        {
            _camera.OrthographicWidth *= factor;
        }
    }

    /// <summary>
    /// Setzt die Kamera auf die Standardansicht (isometrisch) zurueck.
    /// </summary>
    public void ResetToIsometric(double distance = 200.0)
    {
        var isoAngle = System.Math.PI / 4.0; // 45 Grad
        var elevation = System.Math.Atan(1.0 / System.Math.Sqrt(2.0)); // ~35.26 Grad

        var x = distance * System.Math.Cos(elevation) * System.Math.Cos(isoAngle);
        var y = distance * System.Math.Cos(elevation) * System.Math.Sin(isoAngle);
        var z = distance * System.Math.Sin(elevation);

        _camera.Position = new Vector3D(x, y, z);
        _camera.Target = Vector3D.Zero;
        _camera.UpDirection = Vector3D.UnitZ;
    }

    /// <summary>
    /// Richtet die Kamera auf eine bestimmte Standardansicht aus.
    /// </summary>
    public void SetStandardView(StandardView view, double distance = 200.0)
    {
        _camera.Target = Vector3D.Zero;
        _camera.UpDirection = Vector3D.UnitZ;

        _camera.Position = view switch
        {
            StandardView.Top => new Vector3D(0, 0, distance),
            StandardView.Bottom => new Vector3D(0, 0, -distance),
            StandardView.Front => new Vector3D(0, -distance, 0),
            StandardView.Back => new Vector3D(0, distance, 0),
            StandardView.Left => new Vector3D(-distance, 0, 0),
            StandardView.Right => new Vector3D(distance, 0, 0),
            StandardView.Isometric => throw new InvalidOperationException("Verwende ResetToIsometric() stattdessen."),
            _ => throw new ArgumentOutOfRangeException(nameof(view))
        };

        // Fuer Top/Bottom muss die Up-Direction angepasst werden
        if (view is StandardView.Top or StandardView.Bottom)
        {
            _camera.UpDirection = Vector3D.UnitY;
        }
    }
}

/// <summary>
/// Vordefinierte Standard-Ansichten.
/// </summary>
public enum StandardView
{
    /// <summary>Draufsicht (Z+).</summary>
    Top,
    /// <summary>Untersicht (Z-).</summary>
    Bottom,
    /// <summary>Vorderansicht (Y-).</summary>
    Front,
    /// <summary>Rueckansicht (Y+).</summary>
    Back,
    /// <summary>Linke Seitenansicht (X-).</summary>
    Left,
    /// <summary>Rechte Seitenansicht (X+).</summary>
    Right,
    /// <summary>Isometrische Ansicht.</summary>
    Isometric
}
