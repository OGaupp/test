using CadTool.Core.Math;

namespace CadTool.Core.Primitives;

/// <summary>
/// Quader (Box) definiert durch Breite (X), Tiefe (Y) und Hoehe (Z).
/// Zentriert um den Ursprung in lokalen Koordinaten.
/// </summary>
public sealed class BoxPrimitive : IPrimitive3D
{
    public Guid Id { get; }
    public PrimitiveType Type => PrimitiveType.Box;
    public Matrix4x4 Transform { get; }

    /// <summary>Breite entlang der lokalen X-Achse.</summary>
    public double Width { get; }

    /// <summary>Tiefe entlang der lokalen Y-Achse.</summary>
    public double Depth { get; }

    /// <summary>Hoehe entlang der lokalen Z-Achse.</summary>
    public double Height { get; }

    public BoxPrimitive(double width, double depth, double height, Matrix4x4? transform = null)
    {
        if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width), "Breite muss positiv sein.");
        if (depth <= 0) throw new ArgumentOutOfRangeException(nameof(depth), "Tiefe muss positiv sein.");
        if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height), "Hoehe muss positiv sein.");

        Id = Guid.NewGuid();
        Width = width;
        Depth = depth;
        Height = height;
        Transform = transform ?? Matrix4x4.Identity;
    }

    public BoundingBox3D GetBoundingBox()
    {
        var halfW = Width / 2.0;
        var halfD = Depth / 2.0;
        var halfH = Height / 2.0;

        var corners = new Vector3D[8];
        corners[0] = Transform.TransformPoint(new Vector3D(-halfW, -halfD, -halfH));
        corners[1] = Transform.TransformPoint(new Vector3D(halfW, -halfD, -halfH));
        corners[2] = Transform.TransformPoint(new Vector3D(-halfW, halfD, -halfH));
        corners[3] = Transform.TransformPoint(new Vector3D(halfW, halfD, -halfH));
        corners[4] = Transform.TransformPoint(new Vector3D(-halfW, -halfD, halfH));
        corners[5] = Transform.TransformPoint(new Vector3D(halfW, -halfD, halfH));
        corners[6] = Transform.TransformPoint(new Vector3D(-halfW, halfD, halfH));
        corners[7] = Transform.TransformPoint(new Vector3D(halfW, halfD, halfH));

        var min = corners[0];
        var max = corners[0];
        for (var i = 1; i < 8; i++)
        {
            min = new Vector3D(
                System.Math.Min(min.X, corners[i].X),
                System.Math.Min(min.Y, corners[i].Y),
                System.Math.Min(min.Z, corners[i].Z));
            max = new Vector3D(
                System.Math.Max(max.X, corners[i].X),
                System.Math.Max(max.Y, corners[i].Y),
                System.Math.Max(max.Z, corners[i].Z));
        }

        return new BoundingBox3D(min, max);
    }
}
