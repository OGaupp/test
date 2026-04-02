using CadTool.Core.Math;

namespace CadTool.Core.Primitives;

/// <summary>
/// Torus definiert durch Hauptradius (Abstand Mitte Ring zu Mitte Rohr) und Rohrradius.
/// Liegt in der XY-Ebene, zentriert am Ursprung.
/// </summary>
public sealed class TorusPrimitive : IPrimitive3D
{
    public Guid Id { get; }
    public PrimitiveType Type => PrimitiveType.Torus;
    public Matrix4x4 Transform { get; }

    /// <summary>Hauptradius (Major Radius): Abstand vom Zentrum zum Mittelpunkt des Rohr-Querschnitts.</summary>
    public double MajorRadius { get; }

    /// <summary>Rohrradius (Minor Radius): Radius des Rohr-Querschnitts.</summary>
    public double MinorRadius { get; }

    public TorusPrimitive(double majorRadius, double minorRadius, Matrix4x4? transform = null)
    {
        if (majorRadius <= 0) throw new ArgumentOutOfRangeException(nameof(majorRadius), "Hauptradius muss positiv sein.");
        if (minorRadius <= 0) throw new ArgumentOutOfRangeException(nameof(minorRadius), "Rohrradius muss positiv sein.");
        if (minorRadius >= majorRadius) throw new ArgumentException("Rohrradius muss kleiner als Hauptradius sein.");

        Id = Guid.NewGuid();
        MajorRadius = majorRadius;
        MinorRadius = minorRadius;
        Transform = transform ?? Matrix4x4.Identity;
    }

    public BoundingBox3D GetBoundingBox()
    {
        var center = Transform.TransformPoint(Vector3D.Zero);
        var outerRadius = MajorRadius + MinorRadius;
        var extent = new Vector3D(outerRadius, outerRadius, MinorRadius);
        return new BoundingBox3D(center - extent, center + extent);
    }
}
