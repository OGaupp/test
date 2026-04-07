using CommunityToolkit.Mvvm.ComponentModel;
using CadTool.Core.Domain;
using CadTool.Core.Mesh;
using CadTool.Geometry.Mesh;

namespace CadTool.WinUI.ViewModels;

/// <summary>
/// ViewModel fuer einen einzelnen CadBody im Scene-Tree.
/// </summary>
public partial class CadBodyViewModel : ObservableObject
{
    private readonly MainViewModel _parent;
    private TriangleMesh? _cachedMesh;

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
                _parent.InvalidateViewport();
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

    /// <summary>
    /// Gibt das Mesh des Koerpers zurueck. Wird beim ersten Zugriff aus dem Primitiv erzeugt
    /// und danach gecacht, um wiederholte Mesh-Erzeugung im Renderpfad zu vermeiden.
    /// </summary>
    public TriangleMesh? GetOrCreateMesh()
    {
        if (_cachedMesh is not null)
            return _cachedMesh;

        if (Body.Mesh is not null)
        {
            _cachedMesh = Body.Mesh;
            return _cachedMesh;
        }

        if (Body.Primitive is not null)
        {
            _cachedMesh = MeshGenerator.GenerateMesh(Body.Primitive);
            return _cachedMesh;
        }

        return null;
    }

    /// <summary>Invalidiert den Mesh-Cache (z. B. nach Transformation).</summary>
    public void InvalidateMeshCache()
    {
        _cachedMesh = null;
    }
}
