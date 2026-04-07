using CommunityToolkit.Mvvm.ComponentModel;
using CadTool.Core.Domain;

namespace CadTool.WinUI.ViewModels;

/// <summary>
/// ViewModel fuer einen einzelnen CadBody im Scene-Tree.
/// </summary>
public partial class CadBodyViewModel : ObservableObject
{
    private readonly MainViewModel _parent;

    /// <summary>Zugrunde liegender Domain-Koerper.</summary>
    public CadBody Body { get; }

    /// <summary>Anzeigename.</summary>
    public string Name
    {
        get => Body.Name;
        set
        {
            if (Body.Name != value)
            {
                Body.Name = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>Sichtbarkeit.</summary>
    public bool IsVisible
    {
        get => Body.IsVisible;
        set
        {
            if (Body.IsVisible != value)
            {
                Body.IsVisible = value;
                OnPropertyChanged();
                _parent.ViewportInvalidated?.Invoke();
            }
        }
    }

    /// <summary>Typ-Bezeichnung fuer die Anzeige.</summary>
    public string TypeLabel => Body.Primitive?.Type.ToString() ?? (Body.Mesh is not null ? "Mesh" : "Compound");

    /// <summary>Info-Text (z. B. Dreieckszahl).</summary>
    public string InfoText => Body.Mesh is not null
        ? $"{Body.Mesh.TriangleCount} Dreiecke"
        : Body.Primitive is not null
            ? $"Primitiv: {Body.Primitive.Type}"
            : "Zusammengesetzt";

    public CadBodyViewModel(CadBody body, MainViewModel parent)
    {
        Body = body ?? throw new ArgumentNullException(nameof(body));
        _parent = parent ?? throw new ArgumentNullException(nameof(parent));
    }
}
