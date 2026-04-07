# WinUI Build Fix – Arbeitslog

## Ergebnis
- Der Workspace baut wieder erfolgreich.

## Umgesetzte Änderungen
- `Microsoft.UI.Color` in `src/CadTool.WinUI/Controls/Viewport3DPanel.xaml.cs` auf den korrekten WinUI-3-Farbtyp `Windows.UI.Color` umgestellt.
- Direkten Fremdaufruf des Events `ViewportInvalidated` durch eine kapselnde Methode `InvalidateViewport()` im `MainViewModel` ersetzt.
- `[ObservableProperty]` in `src/CadTool.WinUI/ViewModels/MainViewModel.cs` auf explizite öffentliche `partial`-Properties umgestellt.
- `src/CadTool.WinUI/CadTool.WinUI.csproj` um `<LangVersion>preview</LangVersion>` ergänzt, damit der MVVM Toolkit-Generator für partielle Properties in `WinUI 3` korrekt arbeitet.
- Initialwert von `StatusText` in den Konstruktor verschoben.

## Validierung
- Vollständiger Rebuild erfolgreich.

## Hinweise
- Die ursprünglichen XAML-Fehler waren Folgefehler der C#-/Generatorprobleme im `WinUI`-Projekt.
