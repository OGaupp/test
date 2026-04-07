# Performance-GamePlan

## Zielbild
Die Anwendung soll in zwei zentralen Bereichen deutlich schneller werden:

1. **Interaktive Bedienung im Viewport**
   - Orbit, Pan, Zoom und Resize muessen sichtbar fluessig reagieren.
   - Ziel: kein kompletter UI-Einbruch bereits bei wenigen bis mittleren Szenen.
2. **Geometrie-/CSG-Operationen**
   - `Union`, `Subtract` und `Intersect` muessen fuer typische Arbeitsfaelle deutlich schneller werden.
   - Ziel: nachvollziehbare, messbare Laufzeiten statt "nicht verwendbar langsam".

Der Plan ist **messgetrieben** aufgebaut: erst reproduzierbar messen, dann gezielt optimieren, danach erneut messen.

---

## Aktueller Kenntnisstand

### Bereits identifizierte Schwachstellen

#### 1. `src/CadTool.WinUI/Controls/Viewport3DPanel.xaml.cs`
Wahrscheinliche Hotspots im interaktiven Rendering:

- `Render()` wird bei jedem `PointerMoved`, Zoom und Resize direkt aufgerufen.
- `RenderCanvas.Children.Clear()` leert pro Render den kompletten visuellen Baum.
- `DrawGrid()` zeichnet das Raster in jedem Render komplett neu.
- `DrawBody()` erzeugt pro Dreieck drei neue `Line`-Elemente.
- `MeshGenerator.GenerateMesh(...)` kann im Renderpfad erneut ausgefuehrt werden, wenn `body.Mesh` `null` ist.

**Risiko:** Sehr viele `UIElement`-Allokationen und Layout-/Render-Kosten pro Mausbewegung.

#### 2. `src/CadTool.Geometry/Mesh/MeshBooleanOperations.cs`
Wahrscheinliche Hotspots in der Geometrie:

- `CopyTrianglesOutside(...)`, `CopyTrianglesInside(...)` und `CopyTrianglesInsideInverted(...)` iterieren ueber alle Dreiecke.
- `IsPointInsideMesh(...)` fuehrt fuer jeden Punkt drei Ray-Casts gegen alle Dreiecke des Referenz-Meshes aus.
- Dadurch entsteht schnell ein Aufwand grob in der Groessenordnung `O(n * m)` bis `O(3 * n * m)`.

**Risiko:** Bei komplexeren Meshes skaliert CSG sehr schlecht.

#### 3. `src/CadTool.Geometry/Mesh/MeshGenerator.cs`
Wahrscheinliche Zusatzkosten:

- Meshes werden vollstaendig neu erzeugt.
- Wenn dies wiederholt im UI-Pfad passiert, werden Render-Lags zusaetzlich verstaerkt.

### Bereits vorhandene Messung
Es wurde bereits eine CPU-Messung fuer einen bestehenden Geometrie-Test aufgenommen:

- `CadTool.Geometry.Tests.Mesh.MeshBooleanOperationsTests.Subtract_RemovesInnerTriangles`

Beobachtung:

- `CadTool.Geometry.Mesh.MeshBooleanOperations.Subtract(...)` war dort der groesste Nutzer-Code-Hotspot.
- `CadTool.Geometry.Mesh.MeshGenerator.GenerateMesh(...)` war ebenfalls signifikant.

Das bestaetigt: **CSG und Mesh-Erzeugung sind echte Kostentreiber.**

---

## Priorisierte Arbeitsreihenfolge

### Phase 1 - Reproduzierbare Baseline herstellen
**Ziel:** Nicht raten, sondern genau wissen, wo Zeit verbrannt wird.

#### 1.1 Interaktiven Problemfall definieren
Es wird ein reproduzierbarer Referenzfall benoetigt, z. B.:

- App starten
- 1-5 Primitive erzeugen oder problematische DXF laden
- Kamera orbitieren / zoomen / pannen
- optional danach `Subtract` oder `Union` ausfuehren

#### 1.2 Zwei getrennte Baselines erfassen
Es werden bewusst zwei Problemklassen getrennt behandelt:

1. **UI-/Viewport-Baseline**
   - CPU-Profil waehrend Orbit/Pan/Zoom/Resize
   - Ziel: Kosten von `Render()`, `DrawGrid()`, `DrawBody()`, `ProjectToScreen()` und XAML-Elementaufbau isolieren
2. **CSG-/Mesh-Baseline**
   - vorhandene Unit-Tests gezielt profilieren
   - insbesondere:
     - `CadTool.Geometry.Tests.Mesh.MeshBooleanOperationsTests.Subtract_RemovesInnerTriangles`
     - `CadTool.Geometry.Tests.Mesh.MeshBooleanOperationsTests.Intersect_CompletelyInside_ReturnsSmallerMesh`
     - `CadTool.Geometry.Tests.Mesh.MeshBooleanOperationsTests.Union_TwoNonOverlapping_ContainsAllTriangles`

#### 1.3 Erfolgskriterien festlegen
Vor Beginn der Optimierung muessen konkrete Zielwerte dokumentiert werden, z. B.:

- Viewport bleibt bei `x` Bodies / `y` Dreiecken bedienbar
- `Subtract` ist mindestens `2x` schneller im Referenzfall
- Orbit/Pan fuehlt sich fluessig an und blockiert die UI nicht sichtbar

---

## Phase 2 - Groessten Hebel im Viewport umsetzen
**Ziel:** Interaktive Bedienung zuerst retten. Das ist wahrscheinlich der wichtigste Nutzerhebel.

### 2.1 Render-Throttling / Coalescing einfuehren
**Massnahme:** `Render()` nicht fuer jedes einzelne Maus-Event sofort komplett ausfuehren.

Konkrete Ideen:

- mehrere schnelle `PointerMoved`-Events zusammenfassen
- nur einen ausstehenden Render gleichzeitig zulassen
- optional auf einen festen Takt begrenzen

**Erwarteter Effekt:** Weniger redundante Komplett-Renderings waehrend Mausbewegung.

### 2.2 Mesh-Erzeugung aus dem Renderpfad entfernen
**Massnahme:** `MeshGenerator.GenerateMesh(...)` darf nicht waehrend eines normalen Repaints spontan erneut laufen.

Konkrete Ideen:

- Mesh beim Erzeugen/Importieren/Transformieren vorberechnen
- Mesh bei Aenderungen explizit invalidieren und neu aufbauen
- `CadBodyViewModel` oder Domain-Objekt als Cache-Anker nutzen

**Erwarteter Effekt:** UI-Interaktion wird nicht mehr durch Geometrie-Erzeugung ausgebremst.

### 2.3 Grid nicht in jedem Frame neu erzeugen
**Massnahme:** Raster und Achsen nur neu aufbauen, wenn es wirklich noetig ist.

Konkrete Ideen:

- Grid als separaten, wiederverwendbaren Layer behandeln
- nur bei Kamera-/Viewport-Aenderung aktualisieren, nicht bei jeder irrelevanten Aenderung
- falls moeglich, visuelle Wiederverwendung statt kompletter Neuerzeugung

**Erwarteter Effekt:** Weniger UI-Objekte und weniger Zeichenaufwand pro Frame.

### 2.4 `Line`-Element-Flut reduzieren
**Massnahme:** Pro Dreieck nicht dauerhaft drei neue XAML-`Line`-Objekte erzeugen.

Konkrete Ideen:

- statt vieler einzelner `Line`-Elemente eine kompaktere Zeichenstrategie verwenden
- wiederverwendbare visuelle Elemente / Objekt-Pooling pruefen
- Body- oder Frame-basiertes Batch-Zeichnen statt Element-pro-Kante

**Erwarteter Effekt:** Deutlich weniger UI-Allokationen, Layout- und Visual-Tree-Kosten.

### 2.5 Sichtbarkeits- und Grob-Clipping frueher anwenden
**Massnahme:** Unsichtbare oder klar ausserhalb liegende Inhalte frueher verwerfen.

Konkrete Ideen:

- Body-Bounding-Box vor Dreiecksschleife pruefen
- Bodies ausserhalb des relevanten Bereichs gar nicht zeichnen
- optional spaeter: Backface- oder Edge-Culling pruefen

**Erwarteter Effekt:** Weniger Dreiecksprojektionen und weniger Zeichenoperationen.

---

## Phase 3 - CSG-/Mesh-Algorithmik beschleunigen
**Ziel:** Boolean-Operationen fuer groessere Modelle substanziell schneller machen.

### 3.1 Bounding-Box-Vorfilterung ausbauen
**Massnahme:** Nicht nur Schwerpunkt gegen Gesamt-BBox pruefen, sondern feiner vorsortieren.

Konkrete Ideen:

- Triangle-Bounding-Boxes vorberechnen
- fruehzeitig Dreiecke ausschliessen, die sicher nicht relevant sind
- Referenz-Mesh-Vorfilterung vor Ray-/Triangle-Tests nutzen

**Erwarteter Effekt:** Weniger teure Ray-Triangle-Pruefungen.

### 3.2 `IsPointInsideMesh(...)` billiger machen
**Massnahme:** Die teuerste Schleife aggressiv entlasten.

Konkrete Ideen:

- BBox-Schnellabbruch immer vorgeschaltet lassen
- Kandidatenmenge fuer Ray-Triangle-Tests reduzieren
- spaeter moeglich: einfache Raumstruktur (Uniform Grid / BVH / Spatial Hash)

**Erwarteter Effekt:** Massiver Gewinn bei `Subtract`, `Intersect` und `Union`.

### 3.3 Wiederverwendbare Hilfsdaten vorberechnen
**Massnahme:** Pro Referenz-Mesh Hilfsstrukturen einmal berechnen statt mehrfach.

Konkrete Ideen:

- BBox, Triangle-Liste, Triangle-BBoxes, ggf. Beschleunigungsstruktur cachen
- in `CopyTrianglesOutside(...)`, `CopyTrianglesInside(...)` und `CopyTrianglesInsideInverted(...)` denselben vorbereiteten Kontext nutzen

**Erwarteter Effekt:** Weniger doppelte Arbeit innerhalb einer Boolean-Operation.

### 3.4 Mesh-Zugriff effizienter machen
**Massnahme:** Eng getaktete Schleifen auf unnötige Objekterzeugung pruefen.

Konkrete Ideen:

- `GetTriangle(i)`-Nutzung in Hotpaths hinterfragen
- wiederholte temporaere Strukturen reduzieren
- Zugriffsmuster auf Vertices und Triangle-Indizes straffen

**Erwarteter Effekt:** Zusatzeinsparungen in inneren Schleifen.

---

## Phase 4 - Datenfluss und Invalidierung sauberziehen
**Ziel:** Nur das neu berechnen, was sich wirklich geaendert hat.

### 4.1 Mesh-Lifecycle definieren
Fuer jeden Body muss klar sein:

- wann das Mesh erzeugt wird
- wann es gueltig bleibt
- wann es invalidiert wird
- wann es neu erzeugt werden darf

### 4.2 Viewport-Invalidierung differenzieren
Nicht jede Aenderung soll denselben teuren Pfad ausloesen.

Konkrete Trennung:

- Kamera geaendert
- Szeneninhalt geaendert
- nur Auswahl/Farbe geaendert
- nur Text/Status geaendert

**Erwarteter Effekt:** Weniger unnoetige Komplett-Renderings.

---

## Phase 5 - Messung nach jeder Optimierung
**Ziel:** Jede Optimierung muss belegen, dass sie etwas bringt.

Nach jeder groesseren Massnahme:

1. dieselbe UI-Szene erneut profilieren
2. dieselben Geometrie-Tests erneut profilieren
3. Ergebnisse dokumentieren
   - vorher/nachher
   - CPU-Anteil
   - gefuehlte Interaktivitaet
   - Nebenwirkungen / Genauigkeitsrisiken

---

## Konkreter Umsetzungs-Backlog

### Block A - Sofort hoher Hebel
1. **Render-Throttling fuer Mausbewegung einfuehren**
2. **Mesh-Erzeugung aus `Render()` entfernen und cachen**
3. **Grid/Achsen nicht pro Frame komplett neu aufbauen**
4. **Profil erneut aufnehmen**

### Block B - Zweiter grosser Hebel
5. **CSG-Hilfsdaten pro Operation vorberechnen**
6. **`IsPointInsideMesh(...)` ueber Vorfilterung beschleunigen**
7. **Profil der vorhandenen Boolean-Tests erneut aufnehmen**

### Block C - Danach Feinschliff
8. **Zeichenstrategie fuer Linien/Edges ueberarbeiten**
9. **Body-Level-Clipping und fruehes Aussortieren einfuehren**
10. **weitere Mikro-Optimierungen nur datenbasiert umsetzen**

---

## Input, den ich vom Nutzer noch brauche

Damit die Arbeit maximal wirksam wird, werden folgende Informationen benoetigt:

### A. Reproduzierbarer Problemfall
Bitte moeglichst konkret:

- Welche Schritte fuehren sicher zum Problem?
- Passiert es schon mit primitiven Demo-Koerpern?
- Oder erst mit importierten DXF-/Mesh-Daten?

### B. Problematische Beispieldaten
Ideal waere mindestens eins davon:

- eine kleine problematische DXF-Datei
- Anzahl Bodies und ungefaehre Dreieckszahl einer typischen Szene
- Screenshot / kurze Beschreibung des schlimmsten Falls

### C. Prioritaet des Schmerzes
Was ist aktuell am schlimmsten?

1. Orbit/Pan/Zoom
2. Fenster-Resize
3. Import
4. Boolean Union/Subtract/Intersect
5. Transformationen auf bestehenden Bodies

### D. Zielerwartung
Was heisst fuer dich "gut genug"?

Beispiele:

- Viewport bleibt bei 20k Dreiecken fluessig
- `Subtract` soll unter 1 Sekunde bleiben
- einfache Szenen muessen ohne sichtbares Ruckeln bedienbar sein

### E. Laufumgebung
Falls abweichend oder relevant:

- Debug oder Release?
- CPU / GPU / RAM
- integrierte oder dedizierte Grafik?

---

## Risiken / Hinweise

- Reine Mikro-Optimierungen werden das UI-Problem wahrscheinlich nicht loesen, solange der Viewport pro Event tausende XAML-Elemente neu erzeugt.
- Reine UI-Optimierung wird CSG-Wartezeiten nicht loesen, wenn die Mesh-Algorithmik bei echten Daten dominiert.
- Deshalb ist die Trennung in **Viewport** und **CSG** entscheidend.

---

## Empfohlene erste Umsetzungsetappe
Wenn nur mit dem wahrscheinlich groessten Hebel gestartet werden soll, dann in dieser Reihenfolge:

1. `Viewport3DPanel` entlasten
   - Render-Throttling
   - Mesh-Caching
   - weniger UI-Element-Neuerzeugung
2. erneut messen
3. danach `MeshBooleanOperations` beschleunigen

Das ist voraussichtlich der schnellste Weg, um die Anwendung spuerbar benutzbarer zu machen.
