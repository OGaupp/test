namespace CadTool.Core.Math;

/// <summary>
/// Ebene im 3D-Raum, definiert durch Normalenvektor und Abstand zum Ursprung.
/// Normalform: N · P + D = 0
/// </summary>
public readonly struct Plane3D : IEquatable<Plane3D>
{
    /// <summary>Normalenvektor der Ebene (normalisiert).</summary>
    public Vector3D Normal { get; }

    /// <summary>Abstand zum Ursprung (vorzeichenbehaftet).</summary>
    public double Distance { get; }

    public Plane3D(Vector3D normal, double distance)
    {
        Normal = normal.Normalized();
        Distance = distance;
    }

    /// <summary>Erstellt eine Ebene aus einem Punkt und der Normalen.</summary>
    public static Plane3D FromPointAndNormal(Vector3D point, Vector3D normal)
    {
        var n = normal.Normalized();
        return new Plane3D(n, -n.Dot(point));
    }

    /// <summary>Erstellt eine Ebene aus drei nicht-kollinearen Punkten.</summary>
    public static Plane3D FromThreePoints(Vector3D a, Vector3D b, Vector3D c)
    {
        var normal = (b - a).Cross(c - a);
        if (normal.LengthSquared < 1e-20)
            throw new ArgumentException("Die drei Punkte sind kollinear – keine Ebene definierbar.");
        return FromPointAndNormal(a, normal);
    }

    /// <summary>Vorzeichenbehafteter Abstand eines Punktes zur Ebene.</summary>
    public double SignedDistanceTo(Vector3D point) => Normal.Dot(point) + Distance;

    /// <summary>Projektion eines Punktes auf die Ebene.</summary>
    public Vector3D ProjectPoint(Vector3D point) => point - Normal * SignedDistanceTo(point);

    public bool Equals(Plane3D other) => Normal == other.Normal && System.Math.Abs(Distance - other.Distance) < 1e-10;
    public override bool Equals(object? obj) => obj is Plane3D p && Equals(p);
    public override int GetHashCode() => HashCode.Combine(Normal, System.Math.Round(Distance, 9));

    public static bool operator ==(Plane3D left, Plane3D right) => left.Equals(right);
    public static bool operator !=(Plane3D left, Plane3D right) => !left.Equals(right);

    public override string ToString() => $"Plane(N={Normal}, D={Distance:F4})";
}
