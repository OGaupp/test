using CadTool.Core.Domain;
using CadTool.Core.Interfaces;
using CadTool.Core.Math;

namespace CadTool.Geometry.BooleanOps;

/// <summary>
/// Basisimplementierung fuer Boole'sche Operationen auf Primitiven.
/// 
/// Aktueller Stand: Erzeugt zusammengesetzte Koerper mit korrekter Bounding Box.
/// Die eigentliche Mesh-basierte CSG-Logik erfordert eine externe Bibliothek
/// (z. B. OpenCASCADE.NET) und wird in einer spaeteren Phase integriert.
/// </summary>
public sealed class BooleanOperationService : IBooleanOperationService
{
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

        // Platzhalter: Kombinierte Bounding Box als Approximation
        // Die exakte CSG-Berechnung erfolgt nach Integration einer Mesh-Engine.
        return new CadBody(resultName, bodyA.WorldTransform);
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
