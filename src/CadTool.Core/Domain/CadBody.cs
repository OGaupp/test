using CadTool.Core.Math;
using CadTool.Core.Mesh;
using CadTool.Core.Primitives;

namespace CadTool.Core.Domain;

/// <summary>
/// Repraesentiert einen einzelnen 3D-Koerper in der Szene.
/// Ein CadBody kann ein Primitiv, ein Mesh oder das Ergebnis einer Boole'schen Operation sein.
/// </summary>
public sealed class CadBody
{
    /// <summary>Eindeutige ID.</summary>
    public Guid Id { get; }

    /// <summary>Anzeigename des Koerpers.</summary>
    public string Name { get; set; }

    /// <summary>Zugrunde liegendes Primitiv (null bei zusammengesetzten Koerpern).</summary>
    public IPrimitive3D? Primitive { get; }

    /// <summary>Dreiecksnetz (null bei reinen Primitiv-Koerpern ohne CSG-Ergebnis).</summary>
    public TriangleMesh? Mesh { get; }

    /// <summary>Transformation im Weltkoordinatensystem.</summary>
    public Matrix4x4 WorldTransform { get; set; }

    /// <summary>Sichtbarkeit in der Szene.</summary>
    public bool IsVisible { get; set; } = true;

    public CadBody(string name, IPrimitive3D primitive)
    {
        Id = Guid.NewGuid();
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Primitive = primitive ?? throw new ArgumentNullException(nameof(primitive));
        WorldTransform = primitive.Transform;
    }

    /// <summary>Erstellt einen zusammengesetzten Koerper (z. B. nach Boole-Operation).</summary>
    public CadBody(string name, Matrix4x4 worldTransform)
    {
        Id = Guid.NewGuid();
        Name = name ?? throw new ArgumentNullException(nameof(name));
        WorldTransform = worldTransform;
    }

    /// <summary>Erstellt einen Koerper mit Mesh (z. B. Ergebnis einer CSG-Operation).</summary>
    public CadBody(string name, Matrix4x4 worldTransform, TriangleMesh mesh)
    {
        Id = Guid.NewGuid();
        Name = name ?? throw new ArgumentNullException(nameof(name));
        WorldTransform = worldTransform;
        Mesh = mesh ?? throw new ArgumentNullException(nameof(mesh));
    }

    /// <summary>Bounding Box im Weltkoordinatensystem.</summary>
    public BoundingBox3D GetBoundingBox()
    {
        if (Mesh is not null)
            return Mesh.GetBoundingBox();

        return Primitive?.GetBoundingBox()
            ?? new BoundingBox3D(Vector3D.Zero, Vector3D.Zero);
    }
}
