using CadTool.Core.Domain;
using CadTool.Core.Interfaces;
using CadTool.Core.Math;

namespace CadTool.Geometry.Transforms;

/// <summary>
/// Implementierung des Transformations-Service fuer Point-to-Point-Operationen.
/// </summary>
public sealed class TransformService : ITransformService
{
    /// <inheritdoc />
    public void MoveByPoints(CadBody body, Vector3D fromPoint, Vector3D toPoint)
    {
        ArgumentNullException.ThrowIfNull(body);
        var delta = toPoint - fromPoint;
        var translation = Matrix4x4.CreateTranslation(delta);
        body.WorldTransform = translation * body.WorldTransform;
    }

    /// <inheritdoc />
    public void RotateAroundAxis(CadBody body, Vector3D axisOrigin, Vector3D axisDirection, double angleRadians)
    {
        ArgumentNullException.ThrowIfNull(body);
        if (axisDirection.LengthSquared < 1e-15)
            throw new ArgumentException("Achsenrichtung darf nicht Null sein.", nameof(axisDirection));

        // Transformation: Verschieben zum Ursprung → Drehen → Zurueck verschieben
        var toOrigin = Matrix4x4.CreateTranslation(-axisOrigin);
        var rotation = Matrix4x4.CreateRotationAxis(axisDirection, angleRadians);
        var fromOrigin = Matrix4x4.CreateTranslation(axisOrigin);

        var combined = fromOrigin * rotation * toOrigin;
        body.WorldTransform = combined * body.WorldTransform;
    }
}
