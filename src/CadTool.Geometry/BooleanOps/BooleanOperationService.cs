using CadTool.Core.Domain;
using CadTool.Core.Interfaces;
using CadTool.Core.Math;
using CadTool.Core.Mesh;
using CadTool.Geometry.Mesh;

namespace CadTool.Geometry.BooleanOps;

/// <summary>
/// Mesh-basierte Boole'sche Operationen auf 3D-Koerpern (CSG).
/// Konvertiert Primitive in Dreiecksnetze, fuehrt die Operation aus
/// und erzeugt einen zusammengesetzten Koerper mit dem Ergebnis-Mesh.
/// </summary>
public sealed class BooleanOperationService : IBooleanOperationService
{
    private readonly int _meshSegments;

    /// <summary>Erstellt den Service mit Standard-Mesh-Aufloesung (24 Segmente).</summary>
    public BooleanOperationService() : this(24) { }

    /// <summary>Erstellt den Service mit konfigurierbarer Mesh-Aufloesung.</summary>
    public BooleanOperationService(int meshSegments)
    {
        if (meshSegments < 4) throw new ArgumentOutOfRangeException(nameof(meshSegments), "Mindestens 4 Segmente erforderlich.");
        _meshSegments = meshSegments;
    }

    /// <inheritdoc />
    public CadBody Execute(CadBody bodyA, CadBody bodyB, BooleanOperationType operation)
    {
        ArgumentNullException.ThrowIfNull(bodyA);
        ArgumentNullException.ThrowIfNull(bodyB);

        ValidateBoundingBoxOverlap(bodyA, bodyB, operation);

        var resultName = operation switch
        {
            BooleanOperationType.Union => $"Union({bodyA.Name}, {bodyB.Name})",
            BooleanOperationType.Subtract => $"Subtract({bodyA.Name}, {bodyB.Name})",
            BooleanOperationType.Intersect => $"Intersect({bodyA.Name}, {bodyB.Name})",
            _ => throw new ArgumentOutOfRangeException(nameof(operation))
        };

        // Meshes aus Primitiven erzeugen
        var meshA = GetOrGenerateMesh(bodyA);
        var meshB = GetOrGenerateMesh(bodyB);

        // Boole'sche Operation auf Meshes ausfuehren
        var resultMesh = operation switch
        {
            BooleanOperationType.Union => MeshBooleanOperations.Union(meshA, meshB),
            BooleanOperationType.Subtract => MeshBooleanOperations.Subtract(meshA, meshB),
            BooleanOperationType.Intersect => MeshBooleanOperations.Intersect(meshA, meshB),
            _ => throw new ArgumentOutOfRangeException(nameof(operation))
        };

        return new CadBody(resultName, bodyA.WorldTransform, resultMesh);
    }

    private TriangleMesh GetOrGenerateMesh(CadBody body)
    {
        // Wenn der Koerper bereits ein Mesh hat (z. B. aus vorheriger Operation), verwenden
        if (body.Mesh is not null)
            return body.Mesh.Clone();

        // Sonst aus dem Primitiv erzeugen
        if (body.Primitive is null)
            throw new InvalidOperationException($"Koerper '{body.Name}' hat weder Primitiv noch Mesh.");

        return MeshGenerator.GenerateMesh(body.Primitive, _meshSegments);
    }

    private static void ValidateBoundingBoxOverlap(CadBody bodyA, CadBody bodyB, BooleanOperationType operation)
    {
        var bboxA = bodyA.GetBoundingBox();
        var bboxB = bodyB.GetBoundingBox();

        if (operation == BooleanOperationType.Intersect && !bboxA.Intersects(bboxB))
        {
            throw new InvalidOperationException(
                "Intersect-Operation nicht moeglich: Die Bounding Boxes der Koerper ueberlappen sich nicht.");
        }
    }
}
