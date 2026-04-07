using Microsoft.UI.Xaml.Controls;
using CadTool.WinUI.ViewModels;

namespace CadTool.WinUI.Controls;

/// <summary>
/// Statusleiste mit Status-Text, Koerper- und Dreieckszahl.
/// </summary>
public sealed partial class StatusBarPanel : UserControl
{
    private MainViewModel? _viewModel;

    public StatusBarPanel()
    {
        InitializeComponent();
    }

    public void Initialize(MainViewModel viewModel)
    {
        _viewModel = viewModel;
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
        UpdateDisplay();
    }

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        DispatcherQueue?.TryEnqueue(UpdateDisplay);
    }

    private void UpdateDisplay()
    {
        if (_viewModel is null) return;

        TxtStatus.Text = _viewModel.StatusText;
        TxtBodyCount.Text = $"Körper: {_viewModel.BodyCount}";
        TxtTriangleCount.Text = $"Dreiecke: {_viewModel.TriangleCount}";
    }
}
