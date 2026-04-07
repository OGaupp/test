using Microsoft.UI.Xaml;

namespace CadTool.WinUI;

/// <summary>
/// WinUI3 Application-Einstiegspunkt.
/// </summary>
public partial class App : Application
{
    private Window? _mainWindow;

    public App()
    {
        InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        _mainWindow = new MainWindow();
        _mainWindow.Activate();
    }
}
