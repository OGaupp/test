using CadTool.Core.Math;

namespace CadTool.Core.Viewport;

/// <summary>
/// Interface fuer den 3D-Viewport.
/// Definiert die Schnittstelle zwischen Geometrie-Engine und Rendering-Backend.
/// Implementierungen erfolgen im plattformspezifischen UI-Projekt (z. B. WinUI, MAUI).
/// </summary>
public interface IViewport3D
{
    /// <summary>Aktive Kamera des Viewports.</summary>
    Camera3D Camera { get; }

    /// <summary>Aktualisiert die Darstellung (Render-Request).</summary>
    void Invalidate();

    /// <summary>Setzt die Kamera auf die Standard-Ansicht zurueck.</summary>
    void ResetView();

    /// <summary>Zoomt auf einen bestimmten Bereich (Fit to BoundingBox).</summary>
    void ZoomToFit(BoundingBox3D boundingBox);
}
