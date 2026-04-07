# Game Plan: Solution Architecture & Alle Phasen

## Ziel

Vollständige Implementierung aller 4 Phasen der CAD-Tool-Roadmap.

## Aufgaben

### Phase 1: Solution Architecture & Foundation ✅
- [x] Solution-Struktur erstellen (.sln, global.json, Directory.Build.props)
- [x] CadTool.Core: Domain-Modelle, Interfaces, Math-Typen
- [x] CadTool.Geometry: 3D-Logik, Transforms, Boole'sche Operationen
- [x] CadTool.Infrastructure: DXF-Service (Stub)
- [x] CadTool.WinUI: Projekt-Stub (Windows-only)

### .NET 10 Upgrade ✅
- [x] global.json auf .NET 10.0 aktualisiert
- [x] Alle csproj-Dateien auf net10.0 migriert
- [x] LangVersion auf 14 erhöht
- [x] xUnit und Test-SDK auf aktuelle Versionen aktualisiert

### Phase 2: Geometry Engine Integration ✅
- [x] TriangleMesh + Triangle3D (Dreiecksnetze im Core)
- [x] MeshGenerator: Primitive → Dreiecksnetz (Box, Sphere, Cylinder, Torus)
- [x] MeshBooleanOperations: CSG via Ray-Casting mit Majority-Vote
- [x] BooleanOperationService: Vollständige CSG-Integration statt Stub
- [x] CadBody um Mesh-Property erweitert
- [x] DxfService: Vollständige Implementierung mit netDxf (Import/Export)

### Phase 3: 3D Viewport & Navigation ✅
- [x] Camera3D: Plattformunabhängige Kamera-Abstraktion (ViewMatrix, Projektion)
- [x] IViewport3D: Interface für UI-Integration
- [x] OrbitalCameraController: Orbit, Pan, Zoom (AutoCAD-Style)
- [x] StandardView: Top, Front, Right, Isometric etc.

### Phase 4: CAD Features & Transformations ✅
- [x] 3D-Kurven: LineCurve3D, ArcCurve3D, PolylineCurve3D, SplineCurve3D (B-Spline/De-Boor)
- [x] DxfCurveConverter: DXF-Geometrie → 3D-Kurven
- [x] TransformService: Point-to-Point Move/Rotate (bereits in Phase 1)

### Tests ✅
- [x] 156 Unit Tests gesamt (94 Core + 53 Geometry + 9 Infrastructure)

## Entscheidungen

| Thema | Entscheidung | Begründung |
|---|---|---|
| .NET Version | 10.0 | User-Anforderung |
| CSG-Methode | Ray-Casting mit Majority-Vote | Robust gegen Kanten-Treffer, einfache Primitiv-Subtraktionen ausreichend |
| DXF-Library | netDxf 2023.11.10 | MIT-Lizenz, stabil, .NET-kompatibel |
| Spline-Algorithmus | De-Boor | Standard für B-Spline-Auswertung, exakt |
| Kamera-Steuerung | AutoCAD-Style | Branchenstandard im Maschinenbau |
