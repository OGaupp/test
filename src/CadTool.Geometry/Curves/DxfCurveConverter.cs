using CadTool.Core.Curves;
using CadTool.Core.Math;

namespace CadTool.Geometry.Curves;

/// <summary>
/// Konvertiert DXF-Geometrie-Daten (Punkte, Winkel) in 3D-Kurven.
/// Dient als Bruecke zwischen dem netDxf-Import und dem internen Kurvenmodell.
/// </summary>
public static class DxfCurveConverter
{
    /// <summary>
    /// Konvertiert eine Folge von DXF-Linienpunkten in eine 3D-Polylinie.
    /// </summary>
    public static PolylineCurve3D ConvertToPolyline(IReadOnlyList<Vector3D> points, bool isClosed = false)
    {
        if (points is null || points.Count < 2)
            throw new ArgumentException("Mindestens 2 Punkte fuer eine Polylinie erforderlich.", nameof(points));

        return new PolylineCurve3D(points, isClosed);
    }

    /// <summary>
    /// Konvertiert DXF-Kreisbogen-Daten in einen 3D-Kreisbogen.
    /// </summary>
    /// <param name="center">Mittelpunkt des Bogens.</param>
    /// <param name="radius">Radius.</param>
    /// <param name="startAngleDegrees">Startwinkel in Grad (DXF-Format).</param>
    /// <param name="endAngleDegrees">Endwinkel in Grad (DXF-Format).</param>
    /// <param name="normal">Normale der Ebene (Standard: Z-Achse).</param>
    public static ArcCurve3D ConvertToArc(
        Vector3D center,
        double radius,
        double startAngleDegrees,
        double endAngleDegrees,
        Vector3D? normal = null)
    {
        var startRad = startAngleDegrees * System.Math.PI / 180.0;
        var endRad = endAngleDegrees * System.Math.PI / 180.0;

        return new ArcCurve3D(center, radius, normal ?? Vector3D.UnitZ, startRad, endRad);
    }

    /// <summary>
    /// Konvertiert DXF-Spline-Daten (Kontrollpunkte, Grad, Knoten) in einen 3D-B-Spline.
    /// </summary>
    public static SplineCurve3D ConvertToSpline(
        IReadOnlyList<Vector3D> controlPoints,
        int degree,
        IReadOnlyList<double>? knots = null)
    {
        return new SplineCurve3D(controlPoints, degree, knots);
    }

    /// <summary>
    /// Konvertiert eine DXF-Linie (zwei Punkte) in ein Liniensegment.
    /// </summary>
    public static LineCurve3D ConvertToLine(Vector3D start, Vector3D end)
    {
        return new LineCurve3D(start, end);
    }

    /// <summary>
    /// Konvertiert einen DXF-Vollkreis in einen Kreisbogen (0 bis 2π).
    /// </summary>
    public static ArcCurve3D ConvertCircleToArc(Vector3D center, double radius, Vector3D? normal = null)
    {
        return new ArcCurve3D(center, radius, normal ?? Vector3D.UnitZ, 0, 2.0 * System.Math.PI);
    }
}
