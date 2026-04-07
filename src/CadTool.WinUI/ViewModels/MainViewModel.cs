using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CadTool.Core.Domain;
using CadTool.Core.Interfaces;
using CadTool.Core.Math;
using CadTool.Core.Primitives;
using CadTool.Core.Viewport;
using CadTool.Geometry.BooleanOps;
using CadTool.Geometry.Mesh;
using CadTool.Geometry.Primitives;
using CadTool.Geometry.Transforms;
using CadTool.Geometry.Viewport;
using CadTool.Infrastructure.Dxf;

namespace CadTool.WinUI.ViewModels;

/// <summary>
/// Haupt-ViewModel der Anwendung (MVVM).
/// Verbindet Domain-Logik mit der WinUI3-Oberflaeche.
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly CadScene _scene = new();
    private readonly ITransformService _transformService = new TransformService();
    private readonly IBooleanOperationService _booleanService = new BooleanOperationService();
    private readonly IDxfService _dxfService = new DxfService();
    private readonly OrbitalCameraController _cameraController;

    /// <summary>Kamera fuer den 3D-Viewport.</summary>
    public Camera3D Camera { get; } = new();

    /// <summary>Orbitaler Kamera-Controller (AutoCAD-Style).</summary>
    public OrbitalCameraController CameraController => _cameraController;

    /// <summary>Alle Koerper in der Szene.</summary>
    public ObservableCollection<CadBodyViewModel> Bodies { get; } = [];

    [ObservableProperty]
    public partial CadBodyViewModel? SelectedBody { get; set; }

    [ObservableProperty]
    public partial string StatusText { get; set; }

    [ObservableProperty]
    public partial int BodyCount { get; set; }

    [ObservableProperty]
    public partial int TriangleCount { get; set; }

    /// <summary>Event: Viewport muss neu gerendert werden.</summary>
    public event Action? ViewportInvalidated;

    public MainViewModel()
    {
        StatusText = "Bereit";
        _cameraController = new OrbitalCameraController(Camera);
        _cameraController.ResetToIsometric();
    }

    internal void InvalidateViewport()
    {
        ViewportInvalidated?.Invoke();
    }

    // --- Primitiv-Erstellung ---

    [RelayCommand]
    private void AddBox()
    {
        var primitive = PrimitiveFactory.CreateBox(20, 20, 20);
        AddBodyFromPrimitive("Quader", primitive);
    }

    [RelayCommand]
    private void AddSphere()
    {
        var primitive = PrimitiveFactory.CreateSphere(10);
        AddBodyFromPrimitive("Kugel", primitive);
    }

    [RelayCommand]
    private void AddCylinder()
    {
        var primitive = PrimitiveFactory.CreateCylinder(8, 30);
        AddBodyFromPrimitive("Zylinder", primitive);
    }

    [RelayCommand]
    private void AddTorus()
    {
        var primitive = PrimitiveFactory.CreateTorus(15, 3);
        AddBodyFromPrimitive("Torus", primitive);
    }

    private void AddBodyFromPrimitive(string name, IPrimitive3D primitive)
    {
        var body = new CadBody(name, primitive);
        _scene.Add(body);

        var vm = new CadBodyViewModel(body, this);
        Bodies.Add(vm);
        SelectedBody = vm;

        UpdateStats();
        StatusText = $"{name} hinzugefügt";
        InvalidateViewport();
    }

    // --- Boole'sche Operationen ---

    [RelayCommand]
    private void BooleanUnion() => ExecuteBooleanOp(BooleanOperationType.Union);

    [RelayCommand]
    private void BooleanSubtract() => ExecuteBooleanOp(BooleanOperationType.Subtract);

    [RelayCommand]
    private void BooleanIntersect() => ExecuteBooleanOp(BooleanOperationType.Intersect);

    private void ExecuteBooleanOp(BooleanOperationType operation)
    {
        if (Bodies.Count < 2)
        {
            StatusText = "Mindestens 2 Körper für Boole-Operation benötigt";
            return;
        }

        // Verwende die ersten zwei Koerper (oder selektierten + ersten anderen)
        var bodyA = SelectedBody ?? Bodies[0];
        var bodyB = Bodies.FirstOrDefault(b => b != bodyA);
        if (bodyB is null)
        {
            StatusText = "Zweiter Körper nicht gefunden";
            return;
        }

        try
        {
            var result = _booleanService.Execute(bodyA.Body, bodyB.Body, operation);
            _scene.Remove(bodyA.Body.Id);
            _scene.Remove(bodyB.Body.Id);
            _scene.Add(result);

            Bodies.Remove(bodyA);
            Bodies.Remove(bodyB);

            var resultVm = new CadBodyViewModel(result, this);
            Bodies.Add(resultVm);
            SelectedBody = resultVm;

            UpdateStats();
            StatusText = $"{operation} ausgeführt → {result.Name}";
            InvalidateViewport();
        }
        catch (Exception ex)
        {
            StatusText = $"Fehler: {ex.Message}";
        }
    }

    // --- Transformationen ---

    [RelayCommand]
    private void MoveSelected()
    {
        if (SelectedBody is null)
        {
            StatusText = "Kein Körper ausgewählt";
            return;
        }

        // Demo-Verschiebung um 10mm in X
        _transformService.MoveByPoints(SelectedBody.Body, Vector3D.Zero, new Vector3D(10, 0, 0));
        StatusText = $"{SelectedBody.Name} verschoben (+10mm X)";
        InvalidateViewport();
    }

    [RelayCommand]
    private void RotateSelected()
    {
        if (SelectedBody is null)
        {
            StatusText = "Kein Körper ausgewählt";
            return;
        }

        // Demo-Rotation um 45° um Z-Achse
        _transformService.RotateAroundAxis(SelectedBody.Body, Vector3D.Zero, Vector3D.UnitZ, System.Math.PI / 4);
        StatusText = $"{SelectedBody.Name} rotiert (45° um Z)";
        InvalidateViewport();
    }

    // --- DXF Im/Export ---

    [RelayCommand]
    private void ImportDxf()
    {
        // File-Picker wird vom Control gehandhabt, hier nur die Logik
        StatusText = "DXF-Import: Datei auswählen...";
    }

    /// <summary>Importiert die DXF-Datei und fuegt die Koerper zur Szene hinzu.</summary>
    public void ImportDxfFile(string filePath)
    {
        try
        {
            var importedBodies = _dxfService.Import(filePath);
            foreach (var body in importedBodies)
            {
                _scene.Add(body);
                Bodies.Add(new CadBodyViewModel(body, this));
            }

            UpdateStats();
            StatusText = $"{importedBodies.Count} Körper aus DXF importiert";
            InvalidateViewport();
        }
        catch (Exception ex)
        {
            StatusText = $"Import-Fehler: {ex.Message}";
        }
    }

    [RelayCommand]
    private void ExportDxf()
    {
        StatusText = "DXF-Export: Ziel auswählen...";
    }

    /// <summary>Exportiert alle sichtbaren Koerper in eine DXF-Datei.</summary>
    public void ExportDxfFile(string filePath)
    {
        try
        {
            var visibleBodies = _scene.Bodies.Where(b => b.IsVisible).ToList();
            _dxfService.Export(visibleBodies, filePath);
            StatusText = $"{visibleBodies.Count} Körper nach DXF exportiert";
        }
        catch (Exception ex)
        {
            StatusText = $"Export-Fehler: {ex.Message}";
        }
    }

    // --- Szenen-Verwaltung ---

    [RelayCommand]
    private void DeleteSelected()
    {
        if (SelectedBody is null)
        {
            StatusText = "Kein Körper ausgewählt";
            return;
        }

        var name = SelectedBody.Name;
        _scene.Remove(SelectedBody.Body.Id);
        Bodies.Remove(SelectedBody);
        SelectedBody = Bodies.LastOrDefault();

        UpdateStats();
        StatusText = $"{name} gelöscht";
        InvalidateViewport();
    }

    [RelayCommand]
    private void ClearScene()
    {
        _scene.Clear();
        Bodies.Clear();
        SelectedBody = null;

        UpdateStats();
        StatusText = "Szene geleert";
        ViewportInvalidated?.Invoke();
    }

    // --- Kamera ---

    [RelayCommand]
    private void ResetCamera()
    {
        _cameraController.ResetToIsometric();
        StatusText = "Kamera zurückgesetzt (Isometrisch)";
        ViewportInvalidated?.Invoke();
    }

    [RelayCommand]
    private void ViewTop()
    {
        _cameraController.SetStandardView(StandardView.Top);
        StatusText = "Draufsicht";
        ViewportInvalidated?.Invoke();
    }

    [RelayCommand]
    private void ViewFront()
    {
        _cameraController.SetStandardView(StandardView.Front);
        StatusText = "Vorderansicht";
        ViewportInvalidated?.Invoke();
    }

    [RelayCommand]
    private void ViewRight()
    {
        _cameraController.SetStandardView(StandardView.Right);
        StatusText = "Rechte Seitenansicht";
        ViewportInvalidated?.Invoke();
    }

    [RelayCommand]
    private void ZoomToFit()
    {
        if (!Bodies.Any())
        {
            StatusText = "Keine Körper in der Szene";
            return;
        }

        var bbox = _scene.Bodies[0].GetBoundingBox();
        for (var i = 1; i < _scene.Bodies.Count; i++)
        {
            bbox = bbox.Union(_scene.Bodies[i].GetBoundingBox());
        }

        // Kamera auf BBox ausrichten
        var center = bbox.Center;
        var size = bbox.Size;
        var maxDim = System.Math.Max(size.X, System.Math.Max(size.Y, size.Z));
        var distance = maxDim * 2.5;

        Camera.Target = center;
        _cameraController.ResetToIsometric(distance);
        Camera.Target = center;

        StatusText = "Zoom auf alle Körper";
        ViewportInvalidated?.Invoke();
    }

    // --- Interne Hilfsmethoden ---

    private void UpdateStats()
    {
        BodyCount = _scene.Bodies.Count;
        TriangleCount = _scene.Bodies
            .Where(b => b.Mesh is not null)
            .Sum(b => b.Mesh!.TriangleCount);
    }

    /// <summary>Zugriff auf die Szene fuer das Rendering.</summary>
    public CadScene Scene => _scene;
}
