namespace CadTool.Core.Math;

/// <summary>
/// Achsgebundene Bounding Box (Axis-Aligned Bounding Box) für schnelle Kollisionsprüfungen.
/// </summary>
public readonly struct BoundingBox3D : IEquatable<BoundingBox3D>
{
    /// <summary>Minimaler Eckpunkt.</summary>
    public Vector3D Min { get; }

    /// <summary>Maximaler Eckpunkt.</summary>
    public Vector3D Max { get; }

    public BoundingBox3D(Vector3D min, Vector3D max)
    {
        Min = new Vector3D(
            System.Math.Min(min.X, max.X),
            System.Math.Min(min.Y, max.Y),
            System.Math.Min(min.Z, max.Z));
        Max = new Vector3D(
            System.Math.Max(min.X, max.X),
            System.Math.Max(min.Y, max.Y),
            System.Math.Max(min.Z, max.Z));
    }

    /// <summary>Mittelpunkt der Box.</summary>
    public Vector3D Center => (Min + Max) / 2.0;

    /// <summary>Dimensionen der Box (Breite, Tiefe, Höhe).</summary>
    public Vector3D Size => Max - Min;

    /// <summary>Prüft, ob ein Punkt innerhalb der Box liegt.</summary>
    public bool Contains(Vector3D point) =>
        point.X >= Min.X && point.X <= Max.X &&
        point.Y >= Min.Y && point.Y <= Max.Y &&
        point.Z >= Min.Z && point.Z <= Max.Z;

    /// <summary>Prüft, ob sich zwei Bounding Boxes überlappen.</summary>
    public bool Intersects(BoundingBox3D other) =>
        Min.X <= other.Max.X && Max.X >= other.Min.X &&
        Min.Y <= other.Max.Y && Max.Y >= other.Min.Y &&
        Min.Z <= other.Max.Z && Max.Z >= other.Min.Z;

    /// <summary>Vereinigung zweier Bounding Boxes.</summary>
    public BoundingBox3D Union(BoundingBox3D other) => new(
        new Vector3D(
            System.Math.Min(Min.X, other.Min.X),
            System.Math.Min(Min.Y, other.Min.Y),
            System.Math.Min(Min.Z, other.Min.Z)),
        new Vector3D(
            System.Math.Max(Max.X, other.Max.X),
            System.Math.Max(Max.Y, other.Max.Y),
            System.Math.Max(Max.Z, other.Max.Z)));

    public bool Equals(BoundingBox3D other) => Min == other.Min && Max == other.Max;
    public override bool Equals(object? obj) => obj is BoundingBox3D b && Equals(b);
    public override int GetHashCode() => HashCode.Combine(Min, Max);

    public static bool operator ==(BoundingBox3D left, BoundingBox3D right) => left.Equals(right);
    public static bool operator !=(BoundingBox3D left, BoundingBox3D right) => !left.Equals(right);

    public override string ToString() => $"BBox({Min} → {Max})";
}
