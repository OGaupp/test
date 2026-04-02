using CadTool.Core.Math;

namespace CadTool.Core.Primitives;

/// <summary>
/// Zylinder definiert durch Radius und Hoehe.
/// Die Achse verlaeuft entlang der lokalen Z-Achse, zentriert am Ursprung.
/// </summary>
public sealed class CylinderPrimitive : IPrimitive3D
{
    public Guid Id { get; }
    public PrimitiveType Type => PrimitiveType.Cylinder;
    public Matrix4x4 Transform { get; }

    /// <summary>Radius des Zylinders.</summary>
    public double Radius { get; }

    /// <summary>Hoehe des Zylinders (entlang Z-Achse).</summary>
    public double Height { get; }

    public CylinderPrimitive(double radius, double height, Matrix4x4? transform = null)
    {
        if (radius <= 0) throw new ArgumentOutOfRangeException(nameof(radius), "Radius muss positiv sein.");
        if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height), "Hoehe muss positiv sein.");

        Id = Guid.NewGuid();
        Radius = radius;
        Height = height;
        Transform = transform ?? Matrix4x4.Identity;
    }

    public BoundingBox3D GetBoundingBox()
    {
        var halfH = Height / 2.0;
        var center = Transform.TransformPoint(Vector3D.Zero);
        var extent = new Vector3D(Radius, Radius, halfH);
        return new BoundingBox3D(center - extent, center + extent);
    }
}
