using CadTool.Core.Math;

namespace CadTool.Core.Mesh;

/// <summary>
/// Dreieck im 3D-Raum, definiert durch drei Eckpunkte (Vertices).
/// Windungsordnung: Gegenuhrzeigersinn (CCW) definiert die Aussenseite (Normalenrichtung).
/// </summary>
public readonly struct Triangle3D : IEquatable<Triangle3D>
{
    /// <summary>Erster Eckpunkt.</summary>
    public Vector3D V0 { get; }

    /// <summary>Zweiter Eckpunkt.</summary>
    public Vector3D V1 { get; }

    /// <summary>Dritter Eckpunkt.</summary>
    public Vector3D V2 { get; }

    public Triangle3D(Vector3D v0, Vector3D v1, Vector3D v2)
    {
        V0 = v0;
        V1 = v1;
        V2 = v2;
    }

    /// <summary>Flaechennormale (nicht normalisiert, Laenge = 2x Flaeche).</summary>
    public Vector3D Normal => (V1 - V0).Cross(V2 - V0);

    /// <summary>Normalisierte Flaechennormale.</summary>
    public Vector3D UnitNormal => Normal.Normalized();

    /// <summary>Flaecheninhalt des Dreiecks.</summary>
    public double Area => Normal.Length / 2.0;

    /// <summary>Schwerpunkt (Centroid) des Dreiecks.</summary>
    public Vector3D Centroid => (V0 + V1 + V2) / 3.0;

    public bool Equals(Triangle3D other) => V0 == other.V0 && V1 == other.V1 && V2 == other.V2;
    public override bool Equals(object? obj) => obj is Triangle3D t && Equals(t);
    public override int GetHashCode() => HashCode.Combine(V0, V1, V2);
    public static bool operator ==(Triangle3D left, Triangle3D right) => left.Equals(right);
    public static bool operator !=(Triangle3D left, Triangle3D right) => !left.Equals(right);
}
