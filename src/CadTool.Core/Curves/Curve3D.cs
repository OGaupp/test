using CadTool.Core.Math;

namespace CadTool.Core.Curves;

/// <summary>
/// Typ einer 3D-Kurve.
/// </summary>
public enum CurveType
{
    /// <summary>Liniensegment.</summary>
    Line,

    /// <summary>Kreisbogen.</summary>
    Arc,

    /// <summary>Polylinie (Folge von Liniensegmenten).</summary>
    Polyline,

    /// <summary>Spline (NURBS-Kurve).</summary>
    Spline
}

/// <summary>
/// Abstrakte 3D-Kurve.
/// Basis fuer Linien, Boegen und Splines im CAD-Kontext.
/// </summary>
public abstract class Curve3D
{
    /// <summary>Typ der Kurve.</summary>
    public abstract CurveType Type { get; }

    /// <summary>Startpunkt der Kurve.</summary>
    public abstract Vector3D StartPoint { get; }

    /// <summary>Endpunkt der Kurve.</summary>
    public abstract Vector3D EndPoint { get; }

    /// <summary>Laenge der Kurve.</summary>
    public abstract double Length { get; }

    /// <summary>Punkt auf der Kurve bei Parameter t (0..1).</summary>
    public abstract Vector3D PointAt(double t);

    /// <summary>Diskretisiert die Kurve in eine Folge von Punkten.</summary>
    public virtual IReadOnlyList<Vector3D> Tessellate(int segments = 20)
    {
        var points = new Vector3D[segments + 1];
        for (var i = 0; i <= segments; i++)
        {
            points[i] = PointAt((double)i / segments);
        }
        return points;
    }
}

/// <summary>
/// Liniensegment im 3D-Raum.
/// </summary>
public sealed class LineCurve3D : Curve3D
{
    public override CurveType Type => CurveType.Line;
    public override Vector3D StartPoint { get; }
    public override Vector3D EndPoint { get; }
    public override double Length => StartPoint.DistanceTo(EndPoint);

    public LineCurve3D(Vector3D start, Vector3D end)
    {
        StartPoint = start;
        EndPoint = end;
    }

    public override Vector3D PointAt(double t) =>
        StartPoint + (EndPoint - StartPoint) * System.Math.Clamp(t, 0.0, 1.0);
}

/// <summary>
/// Kreisbogen im 3D-Raum, definiert in einer Ebene.
/// </summary>
public sealed class ArcCurve3D : Curve3D
{
    public override CurveType Type => CurveType.Arc;

    /// <summary>Mittelpunkt des Kreises.</summary>
    public Vector3D Center { get; }

    /// <summary>Radius des Kreisbogens.</summary>
    public double Radius { get; }

    /// <summary>Normale der Ebene, in der der Bogen liegt.</summary>
    public Vector3D Normal { get; }

    /// <summary>Startwinkel in Radiant.</summary>
    public double StartAngle { get; }

    /// <summary>Endwinkel in Radiant.</summary>
    public double EndAngle { get; }

    private readonly Vector3D _xAxis;
    private readonly Vector3D _yAxis;

    public ArcCurve3D(Vector3D center, double radius, Vector3D normal, double startAngle, double endAngle)
    {
        if (radius <= 0) throw new ArgumentOutOfRangeException(nameof(radius), "Radius muss positiv sein.");

        Center = center;
        Radius = radius;
        Normal = normal.Normalized();
        StartAngle = startAngle;
        EndAngle = endAngle;

        // Lokales Koordinatensystem in der Bogen-Ebene berechnen
        _xAxis = ComputePerpendicularAxis(Normal);
        _yAxis = Normal.Cross(_xAxis).Normalized();
    }

    public override Vector3D StartPoint => PointAtAngle(StartAngle);
    public override Vector3D EndPoint => PointAtAngle(EndAngle);

    public override double Length
    {
        get
        {
            var sweep = EndAngle - StartAngle;
            if (sweep < 0) sweep += 2.0 * System.Math.PI;
            return Radius * System.Math.Abs(sweep);
        }
    }

    public override Vector3D PointAt(double t)
    {
        t = System.Math.Clamp(t, 0.0, 1.0);
        var angle = StartAngle + (EndAngle - StartAngle) * t;
        return PointAtAngle(angle);
    }

    private Vector3D PointAtAngle(double angle) =>
        Center + _xAxis * (Radius * System.Math.Cos(angle)) + _yAxis * (Radius * System.Math.Sin(angle));

    private static Vector3D ComputePerpendicularAxis(Vector3D normal)
    {
        // Findet einen beliebigen Vektor senkrecht zur Normalen
        var candidate = System.Math.Abs(normal.Dot(Vector3D.UnitX)) < 0.9 ? Vector3D.UnitX : Vector3D.UnitY;
        return normal.Cross(candidate).Normalized();
    }
}

/// <summary>
/// Polylinie im 3D-Raum (Folge von Eckpunkten, verbunden durch Linien).
/// </summary>
public sealed class PolylineCurve3D : Curve3D
{
    public override CurveType Type => CurveType.Polyline;

    /// <summary>Eckpunkte der Polylinie.</summary>
    public IReadOnlyList<Vector3D> Points { get; }

    /// <summary>Ob die Polylinie geschlossen ist.</summary>
    public bool IsClosed { get; }

    public override Vector3D StartPoint => Points[0];
    public override Vector3D EndPoint => IsClosed ? Points[0] : Points[^1];

    public PolylineCurve3D(IReadOnlyList<Vector3D> points, bool isClosed = false)
    {
        if (points is null || points.Count < 2)
            throw new ArgumentException("Eine Polylinie benoetigt mindestens 2 Punkte.", nameof(points));

        Points = points;
        IsClosed = isClosed;
    }

    public override double Length
    {
        get
        {
            var totalLength = 0.0;
            for (var i = 0; i < Points.Count - 1; i++)
                totalLength += Points[i].DistanceTo(Points[i + 1]);
            if (IsClosed)
                totalLength += Points[^1].DistanceTo(Points[0]);
            return totalLength;
        }
    }

    public override Vector3D PointAt(double t)
    {
        t = System.Math.Clamp(t, 0.0, 1.0);
        var totalLen = Length;
        if (totalLen < 1e-15) return Points[0];

        var targetDist = t * totalLen;
        var accumulated = 0.0;

        var segmentCount = IsClosed ? Points.Count : Points.Count - 1;
        for (var i = 0; i < segmentCount; i++)
        {
            var next = (i + 1) % Points.Count;
            var segLen = Points[i].DistanceTo(Points[next]);
            if (accumulated + segLen >= targetDist)
            {
                var localT = (targetDist - accumulated) / segLen;
                return Points[i] + (Points[next] - Points[i]) * localT;
            }
            accumulated += segLen;
        }

        return EndPoint;
    }

    public override IReadOnlyList<Vector3D> Tessellate(int segments = 20)
    {
        // Fuer Polylinien genuegen die Eckpunkte selbst
        if (IsClosed)
        {
            var result = new Vector3D[Points.Count + 1];
            for (var i = 0; i < Points.Count; i++) result[i] = Points[i];
            result[Points.Count] = Points[0];
            return result;
        }
        return Points;
    }
}

/// <summary>
/// Kubischer B-Spline im 3D-Raum (De-Boor-Algorithmus).
/// </summary>
public sealed class SplineCurve3D : Curve3D
{
    public override CurveType Type => CurveType.Spline;

    /// <summary>Kontrollpunkte des Splines.</summary>
    public IReadOnlyList<Vector3D> ControlPoints { get; }

    /// <summary>Grad des Splines.</summary>
    public int Degree { get; }

    /// <summary>Knotenvektor.</summary>
    public IReadOnlyList<double> Knots { get; }

    public override Vector3D StartPoint => PointAt(0);
    public override Vector3D EndPoint => PointAt(1);

    public SplineCurve3D(IReadOnlyList<Vector3D> controlPoints, int degree, IReadOnlyList<double>? knots = null)
    {
        if (controlPoints is null || controlPoints.Count < 2)
            throw new ArgumentException("Ein Spline benoetigt mindestens 2 Kontrollpunkte.", nameof(controlPoints));
        if (degree < 1)
            throw new ArgumentOutOfRangeException(nameof(degree), "Grad muss mindestens 1 sein.");
        if (degree >= controlPoints.Count)
            throw new ArgumentException("Grad muss kleiner als die Anzahl der Kontrollpunkte sein.", nameof(degree));

        ControlPoints = controlPoints;
        Degree = degree;
        Knots = knots ?? GenerateUniformKnots(controlPoints.Count, degree);

        if (Knots.Count != controlPoints.Count + degree + 1)
            throw new ArgumentException(
                $"Knotenvektor-Laenge ({Knots.Count}) muss n+p+1 ({controlPoints.Count + degree + 1}) sein.",
                nameof(knots));
    }

    public override double Length
    {
        get
        {
            // Numerische Approximation der Laenge durch Tessellierung
            var points = Tessellate(100);
            var length = 0.0;
            for (var i = 0; i < points.Count - 1; i++)
                length += points[i].DistanceTo(points[i + 1]);
            return length;
        }
    }

    public override Vector3D PointAt(double t)
    {
        t = System.Math.Clamp(t, 0.0, 1.0);

        // Parameter auf Knotenbereich abbilden
        var tMin = Knots[Degree];
        var tMax = Knots[Knots.Count - Degree - 1];
        var u = tMin + t * (tMax - tMin);

        return DeBoor(u);
    }

    /// <summary>De-Boor-Algorithmus fuer B-Spline-Auswertung.</summary>
    private Vector3D DeBoor(double u)
    {
        // Finde das Knotenintervall
        var k = FindKnotSpan(u);

        // Kopie der relevanten Kontrollpunkte
        var d = new Vector3D[Degree + 1];
        for (var j = 0; j <= Degree; j++)
        {
            var idx = k - Degree + j;
            if (idx >= 0 && idx < ControlPoints.Count)
                d[j] = ControlPoints[idx];
        }

        // De-Boor-Iteration
        for (var r = 1; r <= Degree; r++)
        {
            for (var j = Degree; j >= r; j--)
            {
                var knotIdx = k - Degree + j;
                var left = Knots[knotIdx];
                var right = Knots[knotIdx + Degree - r + 1];
                var denom = right - left;

                if (System.Math.Abs(denom) < 1e-15)
                    continue;

                var alpha = (u - left) / denom;
                d[j] = d[j - 1] * (1.0 - alpha) + d[j] * alpha;
            }
        }

        return d[Degree];
    }

    private int FindKnotSpan(double u)
    {
        var n = ControlPoints.Count - 1;
        if (u >= Knots[n + 1]) return n;

        for (var i = Degree; i < n + 1; i++)
        {
            if (u >= Knots[i] && u < Knots[i + 1])
                return i;
        }

        return Degree;
    }

    private static IReadOnlyList<double> GenerateUniformKnots(int controlPointCount, int degree)
    {
        var knotCount = controlPointCount + degree + 1;
        var knots = new double[knotCount];

        for (var i = 0; i <= degree; i++) knots[i] = 0.0;
        for (var i = knotCount - degree - 1; i < knotCount; i++) knots[i] = 1.0;

        var internalKnots = knotCount - 2 * (degree + 1);
        for (var i = 0; i < internalKnots; i++)
        {
            knots[degree + 1 + i] = (double)(i + 1) / (internalKnots + 1);
        }

        return knots;
    }
}
