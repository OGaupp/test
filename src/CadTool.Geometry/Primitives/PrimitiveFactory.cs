using CadTool.Core.Math;
using CadTool.Core.Primitives;

namespace CadTool.Geometry.Primitives;

/// <summary>
/// Factory fuer die Erstellung von 3D-Grundkoerpern.
/// Bietet Convenience-Methoden mit Standardparametern.
/// </summary>
public static class PrimitiveFactory
{
    /// <summary>Erstellt einen Quader an einer bestimmten Position.</summary>
    public static BoxPrimitive CreateBox(double width, double depth, double height, Vector3D? position = null)
    {
        var transform = position.HasValue
            ? Matrix4x4.CreateTranslation(position.Value)
            : Matrix4x4.Identity;
        return new BoxPrimitive(width, depth, height, transform);
    }

    /// <summary>Erstellt eine Kugel an einer bestimmten Position.</summary>
    public static SpherePrimitive CreateSphere(double radius, Vector3D? position = null)
    {
        var transform = position.HasValue
            ? Matrix4x4.CreateTranslation(position.Value)
            : Matrix4x4.Identity;
        return new SpherePrimitive(radius, transform);
    }

    /// <summary>Erstellt einen Zylinder an einer bestimmten Position.</summary>
    public static CylinderPrimitive CreateCylinder(double radius, double height, Vector3D? position = null)
    {
        var transform = position.HasValue
            ? Matrix4x4.CreateTranslation(position.Value)
            : Matrix4x4.Identity;
        return new CylinderPrimitive(radius, height, transform);
    }

    /// <summary>Erstellt einen Torus an einer bestimmten Position.</summary>
    public static TorusPrimitive CreateTorus(double majorRadius, double minorRadius, Vector3D? position = null)
    {
        var transform = position.HasValue
            ? Matrix4x4.CreateTranslation(position.Value)
            : Matrix4x4.Identity;
        return new TorusPrimitive(majorRadius, minorRadius, transform);
    }
}
