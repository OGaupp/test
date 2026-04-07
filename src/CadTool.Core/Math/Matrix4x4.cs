namespace CadTool.Core.Math;

/// <summary>
/// 4×4-Transformationsmatrix für affine Transformationen (Translation, Rotation, Skalierung).
/// Spalten-Hauptreihenfolge (Column-Major) wie in OpenGL/Engineering üblich.
/// </summary>
public readonly struct Matrix4x4 : IEquatable<Matrix4x4>
{
    // Elemente in Row-Major-Notation für Lesbarkeit: M[Zeile][Spalte]
    public double M11 { get; }
    public double M12 { get; }
    public double M13 { get; }
    public double M14 { get; }

    public double M21 { get; }
    public double M22 { get; }
    public double M23 { get; }
    public double M24 { get; }

    public double M31 { get; }
    public double M32 { get; }
    public double M33 { get; }
    public double M34 { get; }

    public double M41 { get; }
    public double M42 { get; }
    public double M43 { get; }
    public double M44 { get; }

    public Matrix4x4(
        double m11, double m12, double m13, double m14,
        double m21, double m22, double m23, double m24,
        double m31, double m32, double m33, double m34,
        double m41, double m42, double m43, double m44)
    {
        M11 = m11; M12 = m12; M13 = m13; M14 = m14;
        M21 = m21; M22 = m22; M23 = m23; M24 = m24;
        M31 = m31; M32 = m32; M33 = m33; M34 = m34;
        M41 = m41; M42 = m42; M43 = m43; M44 = m44;
    }

    /// <summary>Einheitsmatrix.</summary>
    public static Matrix4x4 Identity => new(
        1, 0, 0, 0,
        0, 1, 0, 0,
        0, 0, 1, 0,
        0, 0, 0, 1);

    /// <summary>Translationsmatrix.</summary>
    public static Matrix4x4 CreateTranslation(Vector3D offset) => new(
        1, 0, 0, offset.X,
        0, 1, 0, offset.Y,
        0, 0, 1, offset.Z,
        0, 0, 0, 1);

    /// <summary>Skalierungsmatrix (gleichmäßig).</summary>
    public static Matrix4x4 CreateScale(double factor) => new(
        factor, 0, 0, 0,
        0, factor, 0, 0,
        0, 0, factor, 0,
        0, 0, 0, 1);

    /// <summary>Rotation um die X-Achse (Winkel in Radiant).</summary>
    public static Matrix4x4 CreateRotationX(double radians)
    {
        var cos = System.Math.Cos(radians);
        var sin = System.Math.Sin(radians);
        return new(
            1, 0, 0, 0,
            0, cos, -sin, 0,
            0, sin, cos, 0,
            0, 0, 0, 1);
    }

    /// <summary>Rotation um die Y-Achse (Winkel in Radiant).</summary>
    public static Matrix4x4 CreateRotationY(double radians)
    {
        var cos = System.Math.Cos(radians);
        var sin = System.Math.Sin(radians);
        return new(
            cos, 0, sin, 0,
            0, 1, 0, 0,
            -sin, 0, cos, 0,
            0, 0, 0, 1);
    }

    /// <summary>Rotation um die Z-Achse (Winkel in Radiant).</summary>
    public static Matrix4x4 CreateRotationZ(double radians)
    {
        var cos = System.Math.Cos(radians);
        var sin = System.Math.Sin(radians);
        return new(
            cos, -sin, 0, 0,
            sin, cos, 0, 0,
            0, 0, 1, 0,
            0, 0, 0, 1);
    }

    /// <summary>Rotation um eine beliebige Achse (Rodrigues-Formel, Winkel in Radiant).</summary>
    public static Matrix4x4 CreateRotationAxis(Vector3D axis, double radians)
    {
        var n = axis.Normalized();
        var cos = System.Math.Cos(radians);
        var sin = System.Math.Sin(radians);
        var oneMinusCos = 1.0 - cos;

        return new(
            cos + n.X * n.X * oneMinusCos,
            n.X * n.Y * oneMinusCos - n.Z * sin,
            n.X * n.Z * oneMinusCos + n.Y * sin,
            0,

            n.Y * n.X * oneMinusCos + n.Z * sin,
            cos + n.Y * n.Y * oneMinusCos,
            n.Y * n.Z * oneMinusCos - n.X * sin,
            0,

            n.Z * n.X * oneMinusCos - n.Y * sin,
            n.Z * n.Y * oneMinusCos + n.X * sin,
            cos + n.Z * n.Z * oneMinusCos,
            0,

            0, 0, 0, 1);
    }

    /// <summary>Transformiert einen 3D-Punkt (w=1, mit Translation).</summary>
    public Vector3D TransformPoint(Vector3D p) =>
        new(
            M11 * p.X + M12 * p.Y + M13 * p.Z + M14,
            M21 * p.X + M22 * p.Y + M23 * p.Z + M24,
            M31 * p.X + M32 * p.Y + M33 * p.Z + M34);

    /// <summary>Transformiert eine Richtung (w=0, ohne Translation).</summary>
    public Vector3D TransformDirection(Vector3D d) =>
        new(
            M11 * d.X + M12 * d.Y + M13 * d.Z,
            M21 * d.X + M22 * d.Y + M23 * d.Z,
            M31 * d.X + M32 * d.Y + M33 * d.Z);

    /// <summary>Matrix-Multiplikation.</summary>
    public static Matrix4x4 operator *(Matrix4x4 a, Matrix4x4 b) => new(
        a.M11 * b.M11 + a.M12 * b.M21 + a.M13 * b.M31 + a.M14 * b.M41,
        a.M11 * b.M12 + a.M12 * b.M22 + a.M13 * b.M32 + a.M14 * b.M42,
        a.M11 * b.M13 + a.M12 * b.M23 + a.M13 * b.M33 + a.M14 * b.M43,
        a.M11 * b.M14 + a.M12 * b.M24 + a.M13 * b.M34 + a.M14 * b.M44,

        a.M21 * b.M11 + a.M22 * b.M21 + a.M23 * b.M31 + a.M24 * b.M41,
        a.M21 * b.M12 + a.M22 * b.M22 + a.M23 * b.M32 + a.M24 * b.M42,
        a.M21 * b.M13 + a.M22 * b.M23 + a.M23 * b.M33 + a.M24 * b.M43,
        a.M21 * b.M14 + a.M22 * b.M24 + a.M23 * b.M34 + a.M24 * b.M44,

        a.M31 * b.M11 + a.M32 * b.M21 + a.M33 * b.M31 + a.M34 * b.M41,
        a.M31 * b.M12 + a.M32 * b.M22 + a.M33 * b.M32 + a.M34 * b.M42,
        a.M31 * b.M13 + a.M32 * b.M23 + a.M33 * b.M33 + a.M34 * b.M43,
        a.M31 * b.M14 + a.M32 * b.M24 + a.M33 * b.M34 + a.M34 * b.M44,

        a.M41 * b.M11 + a.M42 * b.M21 + a.M43 * b.M31 + a.M44 * b.M41,
        a.M41 * b.M12 + a.M42 * b.M22 + a.M43 * b.M32 + a.M44 * b.M42,
        a.M41 * b.M13 + a.M42 * b.M23 + a.M43 * b.M33 + a.M44 * b.M43,
        a.M41 * b.M14 + a.M42 * b.M24 + a.M43 * b.M34 + a.M44 * b.M44);

    public bool Equals(Matrix4x4 other)
    {
        const double eps = 1e-10;
        return System.Math.Abs(M11 - other.M11) < eps && System.Math.Abs(M12 - other.M12) < eps &&
               System.Math.Abs(M13 - other.M13) < eps && System.Math.Abs(M14 - other.M14) < eps &&
               System.Math.Abs(M21 - other.M21) < eps && System.Math.Abs(M22 - other.M22) < eps &&
               System.Math.Abs(M23 - other.M23) < eps && System.Math.Abs(M24 - other.M24) < eps &&
               System.Math.Abs(M31 - other.M31) < eps && System.Math.Abs(M32 - other.M32) < eps &&
               System.Math.Abs(M33 - other.M33) < eps && System.Math.Abs(M34 - other.M34) < eps &&
               System.Math.Abs(M41 - other.M41) < eps && System.Math.Abs(M42 - other.M42) < eps &&
               System.Math.Abs(M43 - other.M43) < eps && System.Math.Abs(M44 - other.M44) < eps;
    }

    public override bool Equals(object? obj) => obj is Matrix4x4 m && Equals(m);
    public override int GetHashCode() => HashCode.Combine(M11, M12, M13, M14);

    public static bool operator ==(Matrix4x4 left, Matrix4x4 right) => left.Equals(right);
    public static bool operator !=(Matrix4x4 left, Matrix4x4 right) => !left.Equals(right);
}
