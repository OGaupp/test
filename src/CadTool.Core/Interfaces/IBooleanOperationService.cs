using CadTool.Core.Domain;

namespace CadTool.Core.Interfaces;

/// <summary>
/// Art der Boole'schen Operation.
/// </summary>
public enum BooleanOperationType
{
    /// <summary>Vereinigung (Union): A + B.</summary>
    Union,

    /// <summary>Differenz (Subtract): A - B.</summary>
    Subtract,

    /// <summary>Schnittmenge (Intersect): A ∩ B.</summary>
    Intersect
}

/// <summary>
/// Service fuer Boole'sche Operationen auf 3D-Koerpern.
/// Die Implementierung liegt in CadTool.Geometry, nicht im Core.
/// </summary>
public interface IBooleanOperationService
{
    /// <summary>
    /// Fuehrt eine Boole'sche Operation auf zwei Koerpern aus.
    /// </summary>
    /// <param name="bodyA">Erster Operand.</param>
    /// <param name="bodyB">Zweiter Operand.</param>
    /// <param name="operation">Art der Operation.</param>
    /// <returns>Resultierender zusammengesetzter Koerper.</returns>
    CadBody Execute(CadBody bodyA, CadBody bodyB, BooleanOperationType operation);
}
