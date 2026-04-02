# Roadmap: 3D CAD Core & WinUI3 App Implementation

## 1. Context Analysis

Das Ziel ist die Entwicklung eines CAD-Werkzeugs für die Schärfung von schneidplattenbestückten Rotationswerkzeugen.

- **Domain:** Maschinenbau (Schärfmaschinen für Fräser, Bohrer, Reibahlen).
- **Core Challenge:** 3D-Geometrie-Engine mit Boole'schen Operationen, DXF-Im/Export und orbitale Navigation.
- **Architecture Goals:** WinUI3 (Desktop), entkoppelt für spätere MAUI/Blazor Portierung.
- **Tech Stack:** C#, .NET 8, WinUI3, NuGet-basierte CAD-Kernel (z. B. Eyeshot, HelixToolkit oder OpenCASCADE.NET).

## 2. Plan (Milestones)

### Phase 1: Solution Architecture & Foundation

- Setup einer Multi-Projekt-Solution (Clean Architecture).
- Definition des Domain-Modells (Werkzeug vs. Werkstück).
- Abstraktion der Grafik-Schnittstelle (für spätere Plattform-Wechsel).

### Phase 2: Geometry Engine Integration (The "Brain")

- Auswahl und Integration eines NuGet-Pakets für 3D-Körper (Primitive & CSG/Boole).
- Implementierung der DXF-Schnittstelle.

### Phase 3: 3D Viewport & Navigation

- WinUI3 Viewport Integration.
- Implementierung der orbitalen Kamera-Steuerung (AutoCAD-Style).

### Phase 4: CAD Features & Transformations

- Erstellung von Primitiven via Code/UI.
- Transformations-Logik (Move/Rotate via Points).

## 3. Key Decisions & Trade-offs

| Thema | Entscheidung | Grund |
|---|---|---|
| 3D Kernel | HelixToolkit.SharpDX oder Eyeshot | Helix ist Open Source (MIT), Eyeshot ist kommerziell, aber extrem nah an AutoCAD Mechanical (inkl. Boole). Für Boole-OPs ist Helix oft zu schwach; OpenCascade ist der Goldstandard für Engineering. |
| DXF Library | netDxf | Weit verbreitetes, stabiles NuGet-Paket für DXF R15 (AC1015) Formate. |
| Architektur | MVVM + Service Layer | Die Geometrie-Logik muss in einen reinen .NET Standard oder .NET 8 Library-Service, damit WinUI3 nur die Visualisierung übernimmt. |

## 4. Implementation Steps (Task List)

### Step 1: Project Structure

- [ ] Create `CadTool.Core` (Class Library): Interfaces, Geometrie-Modelle, Werkzeug/Werkstück Definitionen.
- [ ] Create `CadTool.Geometry` (Class Library): Implementierung der 3D-Logik & Boole-Operationen.
- [ ] Create `CadTool.Infrastructure` (Class Library): DXF Im/Export Logik.
- [ ] Create `CadTool.WinUI` (WinUI3 App): UI, Viewport, Input-Handling.

### Step 2: 3D Primitive Support

- [ ] Implementierung der Wrapper für: Torus, Kugel, Quader, Zylinder.
- [ ] Logik für Ebenen, Linien und Punkte im 3D-Raum.
- [ ] Research: Evaluierung von GTS oder OpenCASCADE für verlässliche Boole-Operationen (Union, Subtract, Intersect).

### Step 3: Viewport Implementation

- [ ] Integration des 3D-Controls in WinUI3.
- [ ] Mapping der Maus-Events für Orbit, Pan und Zoom (AutoCAD-Style: Middle Mouse Click = Pan, Shift+Middle = Orbit).

### Step 4: CAD Operations

- [ ] Point-to-Point Transformation Engine (Verschieben/Drehen basierend auf Snapping-Punkten).
- [ ] Konverter: DXF-Splines/Lines -> 3D-Kurven.

## 5. Technical Requirements & Dependencies

- **Framework:** .NET 8 / WinUI 3 (Windows App SDK).
- **NuGet Candidates:**
  - netDxf (DXF Support).
  - HelixToolkit.WinUI (Visualisierung).
  - CadLib (Kommerziell, exzellenter DXF/DWG Support) oder Eyeshot.
- **Deployment:** Unpackaged WinUI3 (für einfache .exe Distribution ohne Store).

## 6. Open Questions / Risks

- **Boolean Operations:** Wie komplex sind die Überschneidungen? (Einfache Primitiv-Subtraktion oder hochkomplexe 
Freiformflächen?).
  - vorerst sind nur einfache Primitiv-Subtraktionen geplant, komplexe Freiformflächen sind nicht vorgesehen.
- **Performance:** Bei sehr vielen Schärf-Operationen (Abtragssimulation) könnte ein GPU-basierter Ansatz nötig sein.
  - Den Punkt verstehe ich gar nicht. Es geht hier bestenfalls um Simulationen von einer Handvoll Operationen. Dieses 
 Tool soll beim Entwickeln und Denken helfen.
- **Lizenzierung:** Sollen rein freie Bibliotheken genutzt werden oder sind kommerzielle Engines (DevDept Eyeshot) 
eine Option? (Empfehlung für Maschinenbau: Kommerziell spart Monate an Arbeit).
  - es sind keine kommerziellen Libs erlaubt

## 7. Documentation Changes

Erstellung einer `architecture.md` zur Erklärung der Trennung von Werkzeug-Geometrie und Visualisierungs-Geometrie.

## Work Log (Preview for gamePlan_done.md)

- Initial project structure drafted.
- Dependency graph for Geometry Engine analyzed.
- Coordinate system conventions defined (Z-up vs Y-up).

---

**Hinweis zur Umsetzung:**
Da du eine saubere Trennung für MAUI/Blazor wünschst, werde ich im Code darauf achten, dass die `CadTool.Geometry` Library absolut keine Abhängigkeiten zu `Microsoft.UI.Xaml` (WinUI) hat. Sie wird rein mathematisch auf Vektoren und Matrizen operieren.
