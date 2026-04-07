namespace CadTool.Core.Math;

/// <summary>
/// Dreidimensionaler Vektor (immutable) für die Geometrie-Engine.
/// Koordinatensystem: Z-up (Maschinenbau-Konvention).
/// </summary>
public readonly struct Vector3D : IEquatable<Vector3D>
{
    public double X { get; }
    public double Y { get; }
    public double Z { get; }

    public Vector3D(double x, double y, double z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    /// <summary>Betrag (Länge) des Vektors.</summary>
    public double Length => System.Math.Sqrt(X * X + Y * Y + Z * Z);

    /// <summary>Quadrat des Betrags – vermeidet Sqrt wo möglich.</summary>
    public double LengthSquared => X * X + Y * Y + Z * Z;

    /// <summary>Normalisierter Einheitsvektor. Wirft bei Nullvektor.</summary>
    public Vector3D Normalized()
    {
        var len = Length;
        if (len < 1e-15)
            throw new InvalidOperationException("Nullvektor kann nicht normalisiert werden.");
        return new Vector3D(X / len, Y / len, Z / len);
    }

    /// <summary>Skalarprodukt (Dot Product).</summary>
    public double Dot(Vector3D other) => X * other.X + Y * other.Y + Z * other.Z;

    /// <summary>Kreuzprodukt (Cross Product).</summary>
    public Vector3D Cross(Vector3D other) =>
        new(
            Y * other.Z - Z * other.Y,
            Z * other.X - X * other.Z,
            X * other.Y - Y * other.X
        );

    /// <summary>Euklidischer Abstand zu einem anderen Punkt.</summary>
    public double DistanceTo(Vector3D other) => (this - other).Length;

    public static Vector3D Zero => new(0, 0, 0);
    public static Vector3D UnitX => new(1, 0, 0);
    public static Vector3D UnitY => new(0, 1, 0);
    public static Vector3D UnitZ => new(0, 0, 1);

    public static Vector3D operator +(Vector3D a, Vector3D b) => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
    public static Vector3D operator -(Vector3D a, Vector3D b) => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
    public static Vector3D operator -(Vector3D a) => new(-a.X, -a.Y, -a.Z);
    public static Vector3D operator *(Vector3D v, double s) => new(v.X * s, v.Y * s, v.Z * s);
    public static Vector3D operator *(double s, Vector3D v) => v * s;
    public static Vector3D operator /(Vector3D v, double s) => new(v.X / s, v.Y / s, v.Z / s);

    public bool Equals(Vector3D other) =>
        System.Math.Abs(X - other.X) < 1e-10 &&
        System.Math.Abs(Y - other.Y) < 1e-10 &&
        System.Math.Abs(Z - other.Z) < 1e-10;

    public override bool Equals(object? obj) => obj is Vector3D v && Equals(v);
    public override int GetHashCode() => HashCode.Combine(
        System.Math.Round(X, 9),
        System.Math.Round(Y, 9),
        System.Math.Round(Z, 9));

    public static bool operator ==(Vector3D left, Vector3D right) => left.Equals(right);
    public static bool operator !=(Vector3D left, Vector3D right) => !left.Equals(right);

    public override string ToString() => $"({X:F4}, {Y:F4}, {Z:F4})";
}
