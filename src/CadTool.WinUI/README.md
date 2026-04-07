# CadTool.WinUI

## Bereitstellung

Die Anwendung ist als normale Desktop-EXE ohne MSIX-/Store-Paket konfiguriert.

Wichtige Projektmerkmale:
- `WindowsPackageType=None`
- `SelfContained=true`
- `WindowsAppSDKSelfContained=true`

Damit soll die Anwendung ohne separat installierte `Windows App Runtime` auf unterstützten Windows-Systemen startbar sein.

## Build

Beispiel für einen Release-Publish:

```powershell
dotnet publish .\src\CadTool.WinUI\CadTool.WinUI.csproj -c Release -r win-x64
```
