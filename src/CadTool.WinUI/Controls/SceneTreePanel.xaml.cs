using Microsoft.UI.Xaml.Controls;
using CadTool.WinUI.ViewModels;

namespace CadTool.WinUI.Controls;

/// <summary>
/// Scene-Tree: Baumansicht aller CadBodies mit Sichtbarkeit und Selektion.
/// </summary>
public sealed partial class SceneTreePanel : UserControl
{
    private MainViewModel? _viewModel;

    public SceneTreePanel()
    {
        InitializeComponent();
    }

    public void Initialize(MainViewModel viewModel)
    {
        _viewModel = viewModel;
        BodyListView.ItemsSource = viewModel.Bodies;
    }

    private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_viewModel is null) return;

        if (BodyListView.SelectedItem is CadBodyViewModel selectedBody)
        {
            _viewModel.SelectedBody = selectedBody;
        }
    }
}
