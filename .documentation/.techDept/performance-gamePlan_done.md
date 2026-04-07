# Performance-GamePlan – Abgeschlossen

## Umsetzungszusammenfassung

Alle drei Bloecke (A, B, C) aus dem [Performance-GamePlan](performance-gamePlan.md) wurden umgesetzt.
Die Optimierungen betreffen sowohl den interaktiven Viewport als auch die CSG-Algorithmik.

---

## Block A – Viewport-Performance

### A1: Render-Throttling
**Datei:** `src/CadTool.WinUI/Controls/Viewport3DPanel.xaml.cs`

- `Render()` wird nicht mehr direkt aus `OnPointerMoved`, `OnPointerWheelChanged` und `OnSizeChanged` aufgerufen.
- Stattdessen: `RequestRender()` setzt ein `_renderScheduled`-Flag und plant den Render ueber `DispatcherQueue.TryEnqueue()`.
- Mehrere schnelle Maus-Events in einem UI-Takt werden zu einem einzigen Render-Durchlauf zusammengefasst.
- **Erwarteter Effekt:** Deutlich weniger redundante Komplett-Renderings waehrend Mausbewegung.

### A2: Mesh-Caching
**Dateien:** `CadBodyViewModel.cs`, `Viewport3DPanel.xaml.cs`, `MainViewModel.cs`

- `CadBodyViewModel` hat jetzt eine `GetOrCreateMesh()`-Methode mit internem Cache.
- Das Mesh wird beim ersten Zugriff aus dem Primitiv erzeugt und danach gecacht.
- `MeshGenerator.GenerateMesh()` wird nicht mehr im Renderpfad aufgerufen.
- `InvalidateMeshCache()` wird nach Transformationen (Move, Rotate) aufgerufen.
- **Erwarteter Effekt:** UI-Interaktion wird nicht mehr durch spontane Mesh-Erzeugung ausgebremst.

### A3: Grid/Achsen-Optimierung
**Datei:** `Viewport3DPanel.xaml.cs`

- Alle Brushes (Wireframe, Selected, Grid, Achsen) sind jetzt statische, wiederverwendbare Instanzen.
- Grid-Linien werden als einzelnes `Path`-Element mit `PathGeometry` gerendert statt vieler einzelner `Line`-Elemente.
- **Erwarteter Effekt:** Weniger Objekt-Allokationen und Layout-Kosten pro Frame.

---

## Block B – CSG-Algorithmik

### B1: MeshSpatialIndex (Uniform Grid)
**Neue Datei:** `src/CadTool.Geometry/Mesh/MeshSpatialIndex.cs`

- Neuer raeumlicher Index fuer Dreiecksnetze basierend auf einem gleichmaessigen 3D-Gitter.
- Gitteraufloesung: `ceil(cbrt(triangleCount))` pro Achse.
- Dreiecke werden per AABB-Overlap in Gitterzellen einsortiert.
- Degenerate Dimensionen (z. B. flaches Mesh) werden automatisch aufgeweitet.

### B2: Beschleunigte Punkt-in-Mesh-Pruefung
**Datei:** `MeshSpatialIndex.cs`

- `IsPointInside()` nutzt 3D-DDA-Gitter-Traversierung (Amanatides & Woo) statt linearer Dreiecks-Iteration.
- Nur Dreiecke in Zellen entlang des Strahlpfades werden als Kandidaten getestet.
- Majority-Vote mit 3 Strahlrichtungen bleibt erhalten (identische Logik wie vorher).
- Slab-basierter Ray-AABB-Schnitttest fuer das Gitter integriert.
- **Erwarteter Effekt:** Massiver Gewinn bei `Subtract`, `Intersect` und `Union` fuer groessere Meshes.

### B3: Wiederverwendbare Hilfsdaten
**Datei:** `src/CadTool.Geometry/Mesh/MeshBooleanOperations.cs`

- Pro Boolean-Operation wird der `MeshSpatialIndex` fuer jedes Referenz-Mesh genau einmal aufgebaut.
- `CopyTrianglesOutside()`, `CopyTrianglesInside()` und `CopyTrianglesInsideInverted()` nutzen denselben vorberechneten Index.
- Die originale `IsPointInsideMesh()`-Methode bleibt fuer Einzeltests und Kompatibilitaet erhalten.

---

## Block C – Feinschliff

### C1: Zeichenstrategie ueberarbeitet
**Datei:** `Viewport3DPanel.xaml.cs`

- Koerper werden als einzelnes `Path`-Element mit `PathGeometry` gerendert.
- Statt `3 * TriangleCount` separate `Line`-UIElemente gibt es jetzt **1 UIElement pro Body**.
- Grid-Linien ebenso als einzelnes `Path`-Element statt vieler `Line`-Elemente.
- **Erwarteter Effekt:** Deutlich weniger UI-Allokationen, Layout- und Visual-Tree-Kosten.

### C2: Body-Level BBox-Clipping
**Datei:** `Viewport3DPanel.xaml.cs`

- Vor der Dreiecksschleife wird die Bounding Box des Body gegen den sichtbaren Bereich geprueft.
- Alle 8 Ecken der BBox werden projiziert. Wenn keine sichtbar ist, wird der Body uebersprungen.
- **Erwarteter Effekt:** Weniger Dreiecksprojektionen und Zeichenoperationen fuer unsichtbare Bodies.

---

## Tests

| Bereich | Vorher | Nachher |
|---------|--------|---------|
| CadTool.Core.Tests | 94 | 94 |
| CadTool.Geometry.Tests | 53 | 66 (+13) |
| CadTool.Infrastructure.Tests | 9 | 9 |
| **Gesamt** | **156** | **169** |

### Neue Tests (`MeshSpatialIndexTests`)
- `IsPointInside_InsideCube_ReturnsTrue`
- `IsPointInside_OutsideCube_ReturnsFalse`
- `IsPointInside_Origin_ReturnsTrue`
- `IsPointInside_ConsistentWithBruteForce` (10 Testpunkte)
- `IsPointInside_Sphere_ConsistentWithBruteForce` (9 Testpunkte)
- `IsPointInside_TranslatedMesh_ConsistentWithBruteForce` (4 Testpunkte)
- `Union_WithSpatialIndex_ProducesSameResult`
- `Subtract_WithSpatialIndex_ProducesSameResult`
- `Intersect_WithSpatialIndex_ProducesSameResult`
- `EmptyMesh_DoesNotThrow`
- `Null_Throws`
- `CountRayIntersections_ThroughCube_ReturnsEvenCount`
- `CountRayIntersections_MissesCube_ReturnsZero`

---

## Nicht umgesetzt (bewusst zurueckgestellt)

- **Phase 4 (Viewport-Invalidierung differenzieren):** Erfordert groesseres Refactoring des Event-Systems.
  Mit dem Render-Throttling ist der groesste Hebel bereits abgedeckt.
- **Item 10 (Mikro-Optimierungen):** Nur datenbasiert sinnvoll – erfordert erneutes Profiling nach diesen Aenderungen.
- **Items 4 und 7 (Profil-Aufnahmen):** Manuelle Aufgaben, die auf einem Windows-System mit Profiling-Tools durchgefuehrt werden muessen.

---

## Naechste Schritte

1. **Profiling auf Windows:** Die Optimierungen mit einem CPU-Profiler verifizieren.
2. **Viewport-Invalidierung differenzieren:** Falls das Rendering nach dem Throttling immer noch zu haeufig ist.
3. **BVH statt Uniform Grid:** Wenn CSG bei stark nicht-uniformen Meshes langsam bleibt.
4. **Win2D / CanvasGeometry:** Fuer noch schnelleres Rendering als XAML-Path-Elemente.
