# WinUI Unpackaged Game Plan

## Kontext
- Die aktuelle `CadTool.WinUI`-Anwendung startet als `WinUI 3`-Desktop-App.
- Zur Laufzeit wird jedoch eine kompatible `Windows App Runtime` verlangt.
- Ziel ist eine normale ausführbare Anwendung ohne separate Store-/Runtime-Installation.

## Aufgaben
- Projektdatei auf self-contained/unpackaged Deployment prüfen und anpassen.
- Sicherstellen, dass die Anwendung weiterhin als Desktop-EXE startet.
- Build validieren.
- Relevante Dokumentation ergänzen.

## Offene Punkte
- Ob zusätzlich ein dediziertes Publish-Profil benötigt wird.
- Ob weitere CI-/Build-Dateien angepasst werden müssen.

## Trade-offs
- `WinUI 3` bleibt erhalten, da dies der kleinste und sicherste Eingriff ist.
- Keine unnötige Migration auf eine andere UI-Technologie.
