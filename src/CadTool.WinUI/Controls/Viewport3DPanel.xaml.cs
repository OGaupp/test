using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using CadTool.Core.Math;
using CadTool.Core.Viewport;
using CadTool.Geometry.Mesh;
using CadTool.Geometry.Viewport;
using CadTool.WinUI.ViewModels;
using Windows.Foundation;
using Windows.UI;

namespace CadTool.WinUI.Controls;

/// <summary>
/// 3D-Viewport mit Maussteuerung (AutoCAD-Style).
/// Rendert die Szene als Drahtgitter-Projektion auf einem WinUI Canvas.
/// Bei Verfuegbarkeit von HelixToolkit wird auf GPU-Rendering umgeschaltet.
/// </summary>
public sealed partial class Viewport3DPanel : UserControl
{
    private MainViewModel? _viewModel;
    private OrbitalCameraController? _cameraController;

    // Maus-Tracking
    private bool _isPanning;
    private bool _isOrbiting;
    private Point _lastPointerPosition;

    // Render-Farben
    private static readonly Color WireframeColor = Microsoft.UI.Colors.CornflowerBlue;
    private static readonly Color SelectedColor = Microsoft.UI.Colors.Orange;
    private static readonly Color GridColor = Microsoft.UI.Colors.Gray;

    public Viewport3DPanel()
    {
        InitializeComponent();
        SizeChanged += OnSizeChanged;
    }

    public void Initialize(MainViewModel viewModel)
    {
        _viewModel = viewModel;
        _cameraController = viewModel.CameraController;
        _viewModel.ViewportInvalidated += OnViewportInvalidated;
        Render();
    }

    // --- Maus-Input (AutoCAD-Style) ---

    private void OnPointerPressed(object sender, PointerRoutedEventArgs e)
    {
        var props = e.GetCurrentPoint(this).Properties;
        _lastPointerPosition = e.GetCurrentPoint(this).Position;

        if (props.IsMiddleButtonPressed)
        {
            var isShiftDown = InputKeyboardSource.GetKeyStateForCurrentThread(
                Windows.System.VirtualKey.Shift).HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down);

            if (isShiftDown)
            {
                _isOrbiting = true;
            }
            else
            {
                _isPanning = true;
            }

            CapturePointer(e.Pointer);
            e.Handled = true;
        }
    }

    private void OnPointerMoved(object sender, PointerRoutedEventArgs e)
    {
        if (_cameraController is null) return;

        var currentPos = e.GetCurrentPoint(this).Position;
        var deltaX = currentPos.X - _lastPointerPosition.X;
        var deltaY = currentPos.Y - _lastPointerPosition.Y;

        if (_isOrbiting)
        {
            _cameraController.Orbit(deltaX, deltaY);
            Render();
            e.Handled = true;
        }
        else if (_isPanning)
        {
            _cameraController.Pan(deltaX, deltaY);
            Render();
            e.Handled = true;
        }

        _lastPointerPosition = currentPos;
    }

    private void OnPointerReleased(object sender, PointerRoutedEventArgs e)
    {
        _isPanning = false;
        _isOrbiting = false;
        ReleasePointerCapture(e.Pointer);
        e.Handled = true;
    }

    private void OnPointerWheelChanged(object sender, PointerRoutedEventArgs e)
    {
        if (_cameraController is null) return;

        var delta = e.GetCurrentPoint(this).Properties.MouseWheelDelta;
        _cameraController.Zoom(delta > 0 ? 1 : -1);
        Render();
        e.Handled = true;
    }

    // --- Rendering (Drahtgitter-Projektion) ---

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        Render();
    }

    private void OnViewportInvalidated()
    {
        // DispatcherQueue stellt Thread-Sicherheit sicher
        DispatcherQueue?.TryEnqueue(Render);
    }

    /// <summary>
    /// Rendert die Szene als Drahtgitter-Projektion auf den Canvas.
    /// Einfache Perspektiv-Projektion: Welt → View → Bildschirm.
    /// </summary>
    private void Render()
    {
        if (_viewModel is null) return;

        RenderCanvas.Children.Clear();
        var width = RenderCanvas.ActualWidth;
        var height = RenderCanvas.ActualHeight;
        if (width < 1 || height < 1) return;

        var camera = _viewModel.Camera;
        var viewMatrix = camera.GetViewMatrix();

        // Hilfstext ausblenden wenn Koerper vorhanden
        TxtPlaceholder.Visibility = _viewModel.Bodies.Count == 0
            ? Visibility.Visible
            : Visibility.Collapsed;

        // Bodenraster zeichnen
        DrawGrid(viewMatrix, width, height);

        // Koerper zeichnen
        foreach (var bodyVm in _viewModel.Bodies)
        {
            if (!bodyVm.IsVisible) continue;

            var isSelected = bodyVm == _viewModel.SelectedBody;
            var color = isSelected ? SelectedColor : WireframeColor;

            DrawBody(bodyVm, viewMatrix, width, height, color);
        }

        // Kamera-Info
        TxtCameraInfo.Text = $"Pos: {camera.Position} | Ziel: {camera.Target}";
    }

    private void DrawBody(CadBodyViewModel bodyVm, Matrix4x4 viewMatrix, double width, double height, Color color)
    {
        var body = bodyVm.Body;

        // Mesh erzeugen (aus Primitiv oder vorhandenes Mesh)
        var mesh = body.Mesh ?? (body.Primitive is not null ? MeshGenerator.GenerateMesh(body.Primitive) : null);
        if (mesh is null) return;

        // Drahtgitter: Kanten der Dreiecke zeichnen
        var brush = new SolidColorBrush(color);
        for (var i = 0; i < mesh.TriangleCount; i++)
        {
            var tri = mesh.GetTriangle(i);

            var p0 = ProjectToScreen(tri.V0, viewMatrix, width, height);
            var p1 = ProjectToScreen(tri.V1, viewMatrix, width, height);
            var p2 = ProjectToScreen(tri.V2, viewMatrix, width, height);

            if (p0 is null || p1 is null || p2 is null) continue;

            DrawLine(p0.Value, p1.Value, brush, 1);
            DrawLine(p1.Value, p2.Value, brush, 1);
            DrawLine(p2.Value, p0.Value, brush, 1);
        }
    }

    private void DrawGrid(Matrix4x4 viewMatrix, double width, double height)
    {
        var brush = new SolidColorBrush(GridColor);
        var gridSize = 100.0;
        var gridStep = 10.0;

        for (var x = -gridSize; x <= gridSize; x += gridStep)
        {
            var from = ProjectToScreen(new Vector3D(x, -gridSize, 0), viewMatrix, width, height);
            var to = ProjectToScreen(new Vector3D(x, gridSize, 0), viewMatrix, width, height);
            if (from is not null && to is not null)
                DrawLine(from.Value, to.Value, brush, 0.3);
        }

        for (var y = -gridSize; y <= gridSize; y += gridStep)
        {
            var from = ProjectToScreen(new Vector3D(-gridSize, y, 0), viewMatrix, width, height);
            var to = ProjectToScreen(new Vector3D(gridSize, y, 0), viewMatrix, width, height);
            if (from is not null && to is not null)
                DrawLine(from.Value, to.Value, brush, 0.3);
        }

        // Koordinatenachsen (farbig)
        DrawAxis(Vector3D.Zero, new Vector3D(gridSize, 0, 0), viewMatrix, width, height, Microsoft.UI.Colors.Red);     // X = Rot
        DrawAxis(Vector3D.Zero, new Vector3D(0, gridSize, 0), viewMatrix, width, height, Microsoft.UI.Colors.Green);   // Y = Gruen
        DrawAxis(Vector3D.Zero, new Vector3D(0, 0, gridSize), viewMatrix, width, height, Microsoft.UI.Colors.Blue);    // Z = Blau
    }

    private void DrawAxis(Vector3D from, Vector3D to, Matrix4x4 viewMatrix, double width, double height, Color color)
    {
        var p0 = ProjectToScreen(from, viewMatrix, width, height);
        var p1 = ProjectToScreen(to, viewMatrix, width, height);
        if (p0 is not null && p1 is not null)
            DrawLine(p0.Value, p1.Value, new SolidColorBrush(color), 2);
    }

    /// <summary>
    /// Projiziert einen 3D-Punkt auf Bildschirmkoordinaten.
    /// Einfache Perspektiv-Projektion mit View-Matrix.
    /// </summary>
    private Point? ProjectToScreen(Vector3D worldPoint, Matrix4x4 viewMatrix, double width, double height)
    {
        var viewPoint = viewMatrix.TransformPoint(worldPoint);

        // Z-Clipping (hinter der Kamera)
        if (viewPoint.Z >= 0) return null;

        // Perspektivische Division
        var fov = _viewModel!.Camera.FieldOfView * System.Math.PI / 180.0;
        var perspectiveScale = 1.0 / System.Math.Tan(fov / 2.0);

        var aspect = width / height;
        var screenX = width / 2.0 + (viewPoint.X / -viewPoint.Z * perspectiveScale / aspect * height / 2.0);
        var screenY = height / 2.0 - (viewPoint.Y / -viewPoint.Z * perspectiveScale * height / 2.0);

        // Clipping
        if (screenX < -width || screenX > 2 * width || screenY < -height || screenY > 2 * height)
            return null;

        return new Point(screenX, screenY);
    }

    private void DrawLine(Point from, Point to, SolidColorBrush brush, double thickness)
    {
        var line = new Line
        {
            X1 = from.X,
            Y1 = from.Y,
            X2 = to.X,
            Y2 = to.Y,
            Stroke = brush,
            StrokeThickness = thickness
        };
        RenderCanvas.Children.Add(line);
    }
}
