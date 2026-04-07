# WinUI Unpackaged Game Plan – Abschluss

## Umgesetzte Arbeit
- `src/CadTool.WinUI/CadTool.WinUI.csproj` auf self-contained, unpackaged Desktop-Bereitstellung angepasst.
- Dokumentation zur gewünschten EXE-Bereitstellung in `src/CadTool.WinUI/README.md` ergänzt.
- Workspace-Build erfolgreich validiert.

## Relevante Änderungen
- `RuntimeIdentifier` auf `win-x64` gesetzt.
- `SelfContained=true` ergänzt.
- `WindowsAppSDKSelfContained=true` ergänzt.
- `WindowsAppSdkUndockedRegFreeWinRTInitialize=true` ergänzt.

## Validierung
- `run_build`: erfolgreich.
- Zusätzlicher `dotnet publish`-Aufruf per CLI schlägt lokal an einer fehlenden `PRI`-MSBuild-Komponente der installierten SDK-/Toolchain-Umgebung fehl.

## Hinweis
- Die Umstellung adressiert gezielt das Ziel „normale ausführbare Anwendung statt Store-/MSIX-Abhängigkeit“.
