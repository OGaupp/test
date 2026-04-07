using CadTool.Core.Math;

namespace CadTool.Core.Viewport;

/// <summary>
/// Kamera-Projektionstyp.
/// </summary>
public enum ProjectionType
{
    /// <summary>Perspektivische Projektion (natuerliche Tiefenwirkung).</summary>
    Perspective,

    /// <summary>Orthographische Projektion (keine Verzerrung, typisch fuer technische Zeichnungen).</summary>
    Orthographic
}

/// <summary>
/// Abstrakte 3D-Kamera fuer den Viewport.
/// Plattformunabhaengig – enthaelt nur die mathematische Kamera-Definition.
/// Die konkrete Rendering-Integration erfolgt im WinUI-Projekt.
/// </summary>
public sealed class Camera3D
{
    /// <summary>Position der Kamera im Weltkoordinatensystem.</summary>
    public Vector3D Position { get; set; }

    /// <summary>Blickziel (LookAt-Punkt).</summary>
    public Vector3D Target { get; set; }

    /// <summary>Up-Vektor der Kamera.</summary>
    public Vector3D UpDirection { get; set; }

    /// <summary>Oeffnungswinkel (Field of View) in Grad (nur fuer Perspective).</summary>
    public double FieldOfView { get; set; }

    /// <summary>Projektionstyp.</summary>
    public ProjectionType Projection { get; set; }

    /// <summary>Nahe Clipping-Ebene.</summary>
    public double NearPlane { get; set; }

    /// <summary>Ferne Clipping-Ebene.</summary>
    public double FarPlane { get; set; }

    /// <summary>Zoom-Faktor fuer orthographische Projektion.</summary>
    public double OrthographicWidth { get; set; }

    public Camera3D()
    {
        Position = new Vector3D(100, 100, 100);
        Target = Vector3D.Zero;
        UpDirection = Vector3D.UnitZ;
        FieldOfView = 45.0;
        Projection = ProjectionType.Perspective;
        NearPlane = 0.1;
        FarPlane = 10000.0;
        OrthographicWidth = 200.0;
    }

    /// <summary>Blickrichtung (normalisiert).</summary>
    public Vector3D LookDirection => (Target - Position).Normalized();

    /// <summary>Abstand zwischen Kamera und Ziel.</summary>
    public double DistanceToTarget => Position.DistanceTo(Target);

    /// <summary>Berechnet die View-Matrix (LookAt).</summary>
    public Matrix4x4 GetViewMatrix()
    {
        var zAxis = (Position - Target).Normalized(); // Kamera schaut in -Z
        var xAxis = UpDirection.Cross(zAxis).Normalized();
        var yAxis = zAxis.Cross(xAxis);

        return new Matrix4x4(
            xAxis.X, xAxis.Y, xAxis.Z, -xAxis.Dot(Position),
            yAxis.X, yAxis.Y, yAxis.Z, -yAxis.Dot(Position),
            zAxis.X, zAxis.Y, zAxis.Z, -zAxis.Dot(Position),
            0, 0, 0, 1);
    }
}
