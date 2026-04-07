# WinUI Build Fix – Arbeitsplan

## Kontext
- Das `WinUI`-Projekt schlägt beim Build fehl.
- Die XAML-Fehler wirken wie Folgefehler einer primären C#-Kompilierungsursache.
- Erste Build-Ausgabe zeigt fehlenden Typ `Microsoft.UI.Color` in `Viewport3DPanel.xaml.cs`.

## Aufgaben
- [ ] Primäre Build-Ursache im `WinUI`-Projekt verifizieren.
- [ ] Minimalen Fix in `Viewport3DPanel.xaml.cs` umsetzen.
- [ ] Gesamten Workspace neu bauen.
- [ ] Prüfen, ob weitere echte Folgefehler verbleiben.
- [ ] Arbeitslog in `winui-build-fix-gamePlan_done.md` schreiben.

## Offene Fragen
- Sind die XAML-Fehler reine Designer-/Folgefehler oder gibt es zusätzlich ein Tooling-Problem?
- Reicht ein Namespace-/Typ-Fix aus, um alle gemeldeten Fehler zu beseitigen?

## Risiken / Trade-offs
- Bevorzugt wird ein minimaler Typ-Fix statt breiterer Projektänderungen an `TargetFramework` oder Paketversionen.
- Änderungen an WinUI-/SDK-Konfigurationen nur, wenn der Rebuild danach weiterhin echte Ursachen zeigt.
