using CadTool.Core.Domain;
using CadTool.Core.Math;

namespace CadTool.Core.Interfaces;

/// <summary>
/// Service fuer Point-to-Point-Transformationen von Koerpern.
/// Implementiert Verschieben und Drehen basierend auf Referenzpunkten.
/// </summary>
public interface ITransformService
{
    /// <summary>
    /// Verschiebt einen Koerper von einem Punkt zu einem anderen.
    /// </summary>
    void MoveByPoints(CadBody body, Vector3D fromPoint, Vector3D toPoint);

    /// <summary>
    /// Dreht einen Koerper um eine Achse durch einen Punkt.
    /// </summary>
    /// <param name="body">Zu drehender Koerper.</param>
    /// <param name="axisOrigin">Ursprung der Rotationsachse.</param>
    /// <param name="axisDirection">Richtung der Rotationsachse.</param>
    /// <param name="angleRadians">Drehwinkel in Radiant.</param>
    void RotateAroundAxis(CadBody body, Vector3D axisOrigin, Vector3D axisDirection, double angleRadians);
}
