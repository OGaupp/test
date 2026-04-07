using CadTool.Core.Math;

namespace CadTool.Core.Primitives;

/// <summary>
/// Kugel definiert durch ihren Radius, zentriert am Ursprung in lokalen Koordinaten.
/// </summary>
public sealed class SpherePrimitive : IPrimitive3D
{
    public Guid Id { get; }
    public PrimitiveType Type => PrimitiveType.Sphere;
    public Matrix4x4 Transform { get; }

    /// <summary>Radius der Kugel.</summary>
    public double Radius { get; }

    public SpherePrimitive(double radius, Matrix4x4? transform = null)
    {
        if (radius <= 0) throw new ArgumentOutOfRangeException(nameof(radius), "Radius muss positiv sein.");

        Id = Guid.NewGuid();
        Radius = radius;
        Transform = transform ?? Matrix4x4.Identity;
    }

    public BoundingBox3D GetBoundingBox()
    {
        var center = Transform.TransformPoint(Vector3D.Zero);
        var r = new Vector3D(Radius, Radius, Radius);
        return new BoundingBox3D(center - r, center + r);
    }
}
