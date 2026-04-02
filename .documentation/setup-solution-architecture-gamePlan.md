# Game Plan: Solution Architecture & Foundation

## Ziel

Setup der Multi-Projekt-Solution mit Clean Architecture für das CAD-Tool zur Schärfung
von schneidplattenbestückten Rotationswerkzeugen.

## Aufgaben

- [x] Solution-Struktur erstellen (.sln, global.json, Directory.Build.props)
- [x] CadTool.Core: Domain-Modelle, Interfaces, Math-Typen
- [x] CadTool.Geometry: 3D-Logik, Transforms, Boole'sche Operationen
- [x] CadTool.Infrastructure: DXF-Service (Stub)
- [x] CadTool.WinUI: Projekt-Stub (Windows-only)
- [x] Unit Tests für Core (Vector3D, Matrix4x4, Plane3D, BoundingBox3D, Line3D, Primitives)
- [x] Unit Tests für Geometry (TransformService, BooleanOperationService, PrimitiveFactory)
- [x] Architektur-Dokumentation (architecture.md)

## Entscheidungen

| Thema | Entscheidung | Begründung |
|---|---|---|
| Koordinatensystem | Z-up | Maschinenbau-Konvention |
| .NET Version | 8.0 (via global.json) | Stabilität, LTS |
| Math-Typen | Eigene Structs (immutable) | Keine Abhängigkeit zu System.Numerics für Portierbarkeit |
| Primitive | Interface + sealed Klassen | Erweiterbar, aber kontrolliert |
| Boole-Operationen | Stub mit BBox-Validierung | Echte CSG-Engine wird in Phase 2 integriert |

## Offene Punkte

- [ ] netDxf-Integration (Phase 2)
- [ ] HelixToolkit.WinUI-Integration (Phase 3)
- [ ] Mesh-Generierung aus Primitiven (Phase 3)
- [ ] Point-to-Point Transformation UI (Phase 4)
