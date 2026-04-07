using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using CadTool.WinUI.ViewModels;

namespace CadTool.WinUI.Controls;

/// <summary>
/// Toolbar mit allen CAD-Aktionen.
/// </summary>
public sealed partial class CadToolBar : UserControl
{
    private MainViewModel? _viewModel;

    public CadToolBar()
    {
        InitializeComponent();
    }

    public void Initialize(MainViewModel viewModel)
    {
        _viewModel = viewModel;
    }

    // --- Primitiv-Erstellung ---
    private void OnAddBox(object sender, RoutedEventArgs e) => _viewModel?.AddBoxCommand.Execute(null);
    private void OnAddSphere(object sender, RoutedEventArgs e) => _viewModel?.AddSphereCommand.Execute(null);
    private void OnAddCylinder(object sender, RoutedEventArgs e) => _viewModel?.AddCylinderCommand.Execute(null);
    private void OnAddTorus(object sender, RoutedEventArgs e) => _viewModel?.AddTorusCommand.Execute(null);

    // --- Boole-Operationen ---
    private void OnBooleanUnion(object sender, RoutedEventArgs e) => _viewModel?.BooleanUnionCommand.Execute(null);
    private void OnBooleanSubtract(object sender, RoutedEventArgs e) => _viewModel?.BooleanSubtractCommand.Execute(null);
    private void OnBooleanIntersect(object sender, RoutedEventArgs e) => _viewModel?.BooleanIntersectCommand.Execute(null);

    // --- Transformationen ---
    private void OnMove(object sender, RoutedEventArgs e) => _viewModel?.MoveSelectedCommand.Execute(null);
    private void OnRotate(object sender, RoutedEventArgs e) => _viewModel?.RotateSelectedCommand.Execute(null);

    // --- Kamera ---
    private void OnResetCamera(object sender, RoutedEventArgs e) => _viewModel?.ResetCameraCommand.Execute(null);
    private void OnZoomToFit(object sender, RoutedEventArgs e) => _viewModel?.ZoomToFitCommand.Execute(null);
    private void OnViewTop(object sender, RoutedEventArgs e) => _viewModel?.ViewTopCommand.Execute(null);
    private void OnViewFront(object sender, RoutedEventArgs e) => _viewModel?.ViewFrontCommand.Execute(null);
    private void OnViewRight(object sender, RoutedEventArgs e) => _viewModel?.ViewRightCommand.Execute(null);

    // --- Datei ---
    private void OnImportDxf(object sender, RoutedEventArgs e) => _viewModel?.ImportDxfCommand.Execute(null);
    private void OnExportDxf(object sender, RoutedEventArgs e) => _viewModel?.ExportDxfCommand.Execute(null);
    private void OnDeleteSelected(object sender, RoutedEventArgs e) => _viewModel?.DeleteSelectedCommand.Execute(null);
    private void OnClearScene(object sender, RoutedEventArgs e) => _viewModel?.ClearSceneCommand.Execute(null);
}
