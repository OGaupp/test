using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using CadTool.Core.Math;
using CadTool.Core.Viewport;
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

    // Render-Throttling: verhindert mehrfache Komplett-Renderings pro Maus-Event-Burst
    private bool _renderScheduled;

    // Wiederverwendbare Brushes (vermeidet Neuerzeugung pro Frame)
    private static readonly SolidColorBrush WireframeBrush = new(Microsoft.UI.Colors.CornflowerBlue);
    private static readonly SolidColorBrush SelectedBrush = new(Microsoft.UI.Colors.Orange);
    private static readonly SolidColorBrush GridBrush = new(Microsoft.UI.Colors.Gray);
    private static readonly SolidColorBrush AxisXBrush = new(Microsoft.UI.Colors.Red);
    private static readonly SolidColorBrush AxisYBrush = new(Microsoft.UI.Colors.Green);
    private static readonly SolidColorBrush AxisZBrush = new(Microsoft.UI.Colors.Blue);

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
            RequestRender();
            e.Handled = true;
        }
        else if (_isPanning)
        {
            _cameraController.Pan(deltaX, deltaY);
            RequestRender();
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
        RequestRender();
        e.Handled = true;
    }

    // --- Render-Throttling ---

    /// <summary>
    /// Plant einen Render-Durchlauf. Mehrere schnelle Aufrufe werden zu einem einzigen
    /// Render zusammengefasst (Coalescing), um redundante Komplett-Renderings zu vermeiden.
    /// </summary>
    private void RequestRender()
    {
        if (_renderScheduled) return;
        _renderScheduled = true;

        DispatcherQueue?.TryEnqueue(() =>
        {
            _renderScheduled = false;
            Render();
        });
    }

    // --- Rendering (Drahtgitter-Projektion) ---

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        RequestRender();
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

        // Bodenraster zeichnen (als einzelnes Path-Element statt vieler Lines)
        DrawGrid(viewMatrix, width, height);

        // Koerper zeichnen
        foreach (var bodyVm in _viewModel.Bodies)
        {
            if (!bodyVm.IsVisible) continue;

            var isSelected = bodyVm == _viewModel.SelectedBody;
            var brush = isSelected ? SelectedBrush : WireframeBrush;

            DrawBody(bodyVm, viewMatrix, width, height, brush);
        }

        // Kamera-Info
        TxtCameraInfo.Text = $"Pos: {camera.Position} | Ziel: {camera.Target}";
    }

    private void DrawBody(CadBodyViewModel bodyVm, Matrix4x4 viewMatrix, double width, double height, SolidColorBrush brush)
    {
        // Gecachtes Mesh verwenden (kein MeshGenerator.GenerateMesh im Renderpfad)
        var mesh = bodyVm.GetOrCreateMesh();
        if (mesh is null) return;

        // Body-Level BBox-Clipping: fruehes Aussortieren unsichtbarer Koerper
        var bodyBBox = mesh.GetBoundingBox();
        if (!IsBodyPotentiallyVisible(bodyBBox, viewMatrix, width, height))
            return;

        // Drahtgitter als einzelnes Path-Element (statt 3*n separate Line-Elemente)
        var pathGeometry = new PathGeometry();
        for (var i = 0; i < mesh.TriangleCount; i++)
        {
            var tri = mesh.GetTriangle(i);

            var p0 = ProjectToScreen(tri.V0, viewMatrix, width, height);
            var p1 = ProjectToScreen(tri.V1, viewMatrix, width, height);
            var p2 = ProjectToScreen(tri.V2, viewMatrix, width, height);

            if (p0 is null || p1 is null || p2 is null) continue;

            var figure = new PathFigure { StartPoint = p0.Value, IsClosed = true };
            figure.Segments.Add(new LineSegment { Point = p1.Value });
            figure.Segments.Add(new LineSegment { Point = p2.Value });
            pathGeometry.Figures.Add(figure);
        }

        if (pathGeometry.Figures.Count > 0)
        {
            RenderCanvas.Children.Add(new Path
            {
                Data = pathGeometry,
                Stroke = brush,
                StrokeThickness = 1
            });
        }
    }

    /// <summary>
    /// Prueft ob ein Body potenziell sichtbar ist.
    /// Konservative Pruefung: gibt im Zweifelsfall true zurueck, um falsche Ausblendung zu vermeiden.
    /// </summary>
    private bool IsBodyPotentiallyVisible(BoundingBox3D bbox, Matrix4x4 viewMatrix, double width, double height)
    {
        // Sonderfall: Kameraposition liegt innerhalb der BBox → Body ist sicher sichtbar
        if (bbox.Contains(_viewModel!.Camera.Position))
            return true;

        // Alle 8 Ecken der BBox pruefen
        ReadOnlySpan<Vector3D> corners =
        [
            new(bbox.Min.X, bbox.Min.Y, bbox.Min.Z),
            new(bbox.Max.X, bbox.Min.Y, bbox.Min.Z),
            new(bbox.Min.X, bbox.Max.Y, bbox.Min.Z),
            new(bbox.Max.X, bbox.Max.Y, bbox.Min.Z),
            new(bbox.Min.X, bbox.Min.Y, bbox.Max.Z),
            new(bbox.Max.X, bbox.Min.Y, bbox.Max.Z),
            new(bbox.Min.X, bbox.Max.Y, bbox.Max.Z),
            new(bbox.Max.X, bbox.Max.Y, bbox.Max.Z),
        ];

        var allBehindCamera = true;
        foreach (var corner in corners)
        {
            var viewPoint = viewMatrix.TransformPoint(corner);

            // Mindestens eine Ecke vor der Kamera → noch sichtbar pruefbar
            if (viewPoint.Z < 0)
                allBehindCamera = false;

            if (ProjectToScreen(corner, viewMatrix, width, height) is not null)
                return true;
        }

        // Wenn Ecken sowohl vor als auch hinter der Kamera liegen, kann der Body
        // den sichtbaren Bereich kreuzen, auch wenn keine Ecke sichtbar projiziert.
        // Konservativ: sichtbar annehmen.
        return !allBehindCamera && corners.Length > 0;
    }

    private void DrawGrid(Matrix4x4 viewMatrix, double width, double height)
    {
        var gridSize = 100.0;
        var gridStep = 10.0;

        // Grid als einzelnes Path-Element (statt vieler separater Lines)
        var gridGeometry = new PathGeometry();

        for (var x = -gridSize; x <= gridSize; x += gridStep)
        {
            var from = ProjectToScreen(new Vector3D(x, -gridSize, 0), viewMatrix, width, height);
            var to = ProjectToScreen(new Vector3D(x, gridSize, 0), viewMatrix, width, height);
            if (from is not null && to is not null)
                AddLineToGeometry(gridGeometry, from.Value, to.Value);
        }

        for (var y = -gridSize; y <= gridSize; y += gridStep)
        {
            var from = ProjectToScreen(new Vector3D(-gridSize, y, 0), viewMatrix, width, height);
            var to = ProjectToScreen(new Vector3D(gridSize, y, 0), viewMatrix, width, height);
            if (from is not null && to is not null)
                AddLineToGeometry(gridGeometry, from.Value, to.Value);
        }

        if (gridGeometry.Figures.Count > 0)
        {
            RenderCanvas.Children.Add(new Path
            {
                Data = gridGeometry,
                Stroke = GridBrush,
                StrokeThickness = 0.3
            });
        }

        // Koordinatenachsen (farbig, jeweils ein eigenes Path-Element)
        DrawAxis(Vector3D.Zero, new Vector3D(gridSize, 0, 0), viewMatrix, width, height, AxisXBrush);
        DrawAxis(Vector3D.Zero, new Vector3D(0, gridSize, 0), viewMatrix, width, height, AxisYBrush);
        DrawAxis(Vector3D.Zero, new Vector3D(0, 0, gridSize), viewMatrix, width, height, AxisZBrush);
    }

    private void DrawAxis(Vector3D from, Vector3D to, Matrix4x4 viewMatrix, double width, double height, SolidColorBrush brush)
    {
        var p0 = ProjectToScreen(from, viewMatrix, width, height);
        var p1 = ProjectToScreen(to, viewMatrix, width, height);
        if (p0 is not null && p1 is not null)
            DrawLine(p0.Value, p1.Value, brush, 2);
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

    /// <summary>Fuegt eine Linie als PathFigure zu einer PathGeometry hinzu.</summary>
    private static void AddLineToGeometry(PathGeometry geometry, Point from, Point to)
    {
        var figure = new PathFigure { StartPoint = from };
        figure.Segments.Add(new LineSegment { Point = to });
        geometry.Figures.Add(figure);
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
