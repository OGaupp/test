namespace CadTool.Core.Math;

/// <summary>
/// Linie im 3D-Raum, definiert durch einen Startpunkt und eine Richtung.
/// </summary>
public readonly struct Line3D
{
    /// <summary>Startpunkt der Linie.</summary>
    public Vector3D Origin { get; }

    /// <summary>Richtungsvektor (nicht zwingend normalisiert).</summary>
    public Vector3D Direction { get; }

    public Line3D(Vector3D origin, Vector3D direction)
    {
        Origin = origin;
        Direction = direction;
    }

    /// <summary>Erstellt eine Linie durch zwei Punkte.</summary>
    public static Line3D FromTwoPoints(Vector3D a, Vector3D b) => new(a, b - a);

    /// <summary>Punkt auf der Linie bei Parameter t: P = Origin + t * Direction.</summary>
    public Vector3D PointAt(double t) => Origin + Direction * t;

    /// <summary>Nächster Punkt auf der Linie zu einem gegebenen Punkt.</summary>
    public Vector3D ClosestPointTo(Vector3D point)
    {
        var t = (point - Origin).Dot(Direction) / Direction.LengthSquared;
        return PointAt(t);
    }

    /// <summary>Abstand eines Punktes zur Linie.</summary>
    public double DistanceTo(Vector3D point) => (point - ClosestPointTo(point)).Length;

    public override string ToString() => $"Line(Origin={Origin}, Dir={Direction})";
}
