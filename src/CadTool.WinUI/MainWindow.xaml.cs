using Microsoft.UI.Xaml;
using CadTool.WinUI.ViewModels;

namespace CadTool.WinUI;

/// <summary>
/// Hauptfenster der CadTool-Anwendung.
/// Verdrahtet ViewModel mit den UI-Controls.
/// </summary>
public sealed partial class MainWindow : Window
{
    public MainViewModel ViewModel { get; }

    public MainWindow()
    {
        ViewModel = new MainViewModel();
        InitializeComponent();

        Title = "CadTool – Schärfwerkzeug CAD";

        // Controls mit ViewModel verbinden
        ToolBar.Initialize(ViewModel);
        SceneTree.Initialize(ViewModel);
        Viewport.Initialize(ViewModel);
        StatusBar.Initialize(ViewModel);
    }
}
