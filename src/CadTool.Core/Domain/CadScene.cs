namespace CadTool.Core.Domain;

/// <summary>
/// Die Szene enthaelt alle Koerper und repraesentiert den aktuellen Zustand des CAD-Modells.
/// </summary>
public sealed class CadScene
{
    private readonly List<CadBody> _bodies = [];

    /// <summary>Alle Koerper in der Szene (readonly).</summary>
    public IReadOnlyList<CadBody> Bodies => _bodies.AsReadOnly();

    /// <summary>Fuegt einen Koerper zur Szene hinzu.</summary>
    public void Add(CadBody body)
    {
        ArgumentNullException.ThrowIfNull(body);
        _bodies.Add(body);
    }

    /// <summary>Entfernt einen Koerper anhand seiner ID.</summary>
    public bool Remove(Guid bodyId)
    {
        var index = _bodies.FindIndex(b => b.Id == bodyId);
        if (index < 0) return false;
        _bodies.RemoveAt(index);
        return true;
    }

    /// <summary>Sucht einen Koerper anhand seiner ID.</summary>
    public CadBody? FindById(Guid id) => _bodies.Find(b => b.Id == id);

    /// <summary>Entfernt alle Koerper aus der Szene.</summary>
    public void Clear() => _bodies.Clear();
}
