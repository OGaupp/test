using CadTool.Core.Math;

namespace CadTool.Core.Primitives;

/// <summary>
/// Typ eines geometrischen Grundkörpers.
/// </summary>
public enum PrimitiveType
{
    /// <summary>Quader (Box).</summary>
    Box,

    /// <summary>Kugel.</summary>
    Sphere,

    /// <summary>Zylinder.</summary>
    Cylinder,

    /// <summary>Torus.</summary>
    Torus
}

/// <summary>
/// Basisinterface für alle 3D-Grundkörper.
/// Jeder Körper definiert seine Geometrie unabhängig von der Visualisierung.
/// </summary>
public interface IPrimitive3D
{
    /// <summary>Eindeutige ID des Körpers.</summary>
    Guid Id { get; }

    /// <summary>Art des Grundkörpers.</summary>
    PrimitiveType Type { get; }

    /// <summary>Transformation des Körpers relativ zum Weltkoordinatensystem.</summary>
    Matrix4x4 Transform { get; }

    /// <summary>Achsgebundene Bounding Box im Weltkoordinatensystem.</summary>
    BoundingBox3D GetBoundingBox();
}
