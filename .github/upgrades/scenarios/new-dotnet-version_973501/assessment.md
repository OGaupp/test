# Projects and dependencies analysis

This document provides a comprehensive overview of the projects and their dependencies in the context of upgrading to .NETCoreApp,Version=v10.0.

## Table of Contents

- [Executive Summary](#executive-Summary)
  - [Highlevel Metrics](#highlevel-metrics)
  - [Projects Compatibility](#projects-compatibility)
  - [Package Compatibility](#package-compatibility)
  - [API Compatibility](#api-compatibility)
- [Aggregate NuGet packages details](#aggregate-nuget-packages-details)
- [Top API Migration Challenges](#top-api-migration-challenges)
  - [Technologies and Features](#technologies-and-features)
  - [Most Frequent API Issues](#most-frequent-api-issues)
- [Projects Relationship Graph](#projects-relationship-graph)
- [Project Details](#project-details)

  - [src\CadTool.Core\CadTool.Core.csproj](#srccadtoolcorecadtoolcorecsproj)
  - [src\CadTool.Geometry\CadTool.Geometry.csproj](#srccadtoolgeometrycadtoolgeometrycsproj)
  - [src\CadTool.Infrastructure\CadTool.Infrastructure.csproj](#srccadtoolinfrastructurecadtoolinfrastructurecsproj)
  - [tests\CadTool.Core.Tests\CadTool.Core.Tests.csproj](#testscadtoolcoretestscadtoolcoretestscsproj)
  - [tests\CadTool.Geometry.Tests\CadTool.Geometry.Tests.csproj](#testscadtoolgeometrytestscadtoolgeometrytestscsproj)


## Executive Summary

### Highlevel Metrics

| Metric | Count | Status |
| :--- | :---: | :--- |
| Total Projects | 5 | All require upgrade |
| Total NuGet Packages | 4 | All compatible |
| Total Code Files | 29 |  |
| Total Code Files with Incidents | 5 |  |
| Total Lines of Code | 1714 |  |
| Total Number of Issues | 5 |  |
| Estimated LOC to modify | 0+ | at least 0,0% of codebase |

### Projects Compatibility

| Project | Target Framework | Difficulty | Package Issues | API Issues | Est. LOC Impact | Description |
| :--- | :---: | :---: | :---: | :---: | :---: | :--- |
| [src\CadTool.Core\CadTool.Core.csproj](#srccadtoolcorecadtoolcorecsproj) | net8.0 | 🟢 Low | 0 | 0 |  | ClassLibrary, Sdk Style = True |
| [src\CadTool.Geometry\CadTool.Geometry.csproj](#srccadtoolgeometrycadtoolgeometrycsproj) | net8.0 | 🟢 Low | 0 | 0 |  | ClassLibrary, Sdk Style = True |
| [src\CadTool.Infrastructure\CadTool.Infrastructure.csproj](#srccadtoolinfrastructurecadtoolinfrastructurecsproj) | net8.0 | 🟢 Low | 0 | 0 |  | ClassLibrary, Sdk Style = True |
| [tests\CadTool.Core.Tests\CadTool.Core.Tests.csproj](#testscadtoolcoretestscadtoolcoretestscsproj) | net8.0 | 🟢 Low | 0 | 0 |  | DotNetCoreApp, Sdk Style = True |
| [tests\CadTool.Geometry.Tests\CadTool.Geometry.Tests.csproj](#testscadtoolgeometrytestscadtoolgeometrytestscsproj) | net8.0 | 🟢 Low | 0 | 0 |  | DotNetCoreApp, Sdk Style = True |

### Package Compatibility

| Status | Count | Percentage |
| :--- | :---: | :---: |
| ✅ Compatible | 4 | 100,0% |
| ⚠️ Incompatible | 0 | 0,0% |
| 🔄 Upgrade Recommended | 0 | 0,0% |
| ***Total NuGet Packages*** | ***4*** | ***100%*** |

### API Compatibility

| Category | Count | Impact |
| :--- | :---: | :--- |
| 🔴 Binary Incompatible | 0 | High - Require code changes |
| 🟡 Source Incompatible | 0 | Medium - Needs re-compilation and potential conflicting API error fixing |
| 🔵 Behavioral change | 0 | Low - Behavioral changes that may require testing at runtime |
| ✅ Compatible | 1792 |  |
| ***Total APIs Analyzed*** | ***1792*** |  |

## Aggregate NuGet packages details

| Package | Current Version | Suggested Version | Projects | Description |
| :--- | :---: | :---: | :--- | :--- |
| coverlet.collector | 6.0.0 |  | [CadTool.Core.Tests.csproj](#testscadtoolcoretestscadtoolcoretestscsproj)<br/>[CadTool.Geometry.Tests.csproj](#testscadtoolgeometrytestscadtoolgeometrytestscsproj) | ✅Compatible |
| Microsoft.NET.Test.Sdk | 17.8.0 |  | [CadTool.Core.Tests.csproj](#testscadtoolcoretestscadtoolcoretestscsproj)<br/>[CadTool.Geometry.Tests.csproj](#testscadtoolgeometrytestscadtoolgeometrytestscsproj) | ✅Compatible |
| xunit | 2.5.3 |  | [CadTool.Core.Tests.csproj](#testscadtoolcoretestscadtoolcoretestscsproj)<br/>[CadTool.Geometry.Tests.csproj](#testscadtoolgeometrytestscadtoolgeometrytestscsproj) | ✅Compatible |
| xunit.runner.visualstudio | 2.5.3 |  | [CadTool.Core.Tests.csproj](#testscadtoolcoretestscadtoolcoretestscsproj)<br/>[CadTool.Geometry.Tests.csproj](#testscadtoolgeometrytestscadtoolgeometrytestscsproj) | ✅Compatible |

## Top API Migration Challenges

### Technologies and Features

| Technology | Issues | Percentage | Migration Path |
| :--- | :---: | :---: | :--- |

### Most Frequent API Issues

| API | Count | Percentage | Category |
| :--- | :---: | :---: | :--- |

## Projects Relationship Graph

Legend:
📦 SDK-style project
⚙️ Classic project

```mermaid
flowchart LR
    P1["<b>📦&nbsp;CadTool.Core.csproj</b><br/><small>net8.0</small>"]
    P2["<b>📦&nbsp;CadTool.Geometry.csproj</b><br/><small>net8.0</small>"]
    P3["<b>📦&nbsp;CadTool.Infrastructure.csproj</b><br/><small>net8.0</small>"]
    P4["<b>📦&nbsp;CadTool.Core.Tests.csproj</b><br/><small>net8.0</small>"]
    P5["<b>📦&nbsp;CadTool.Geometry.Tests.csproj</b><br/><small>net8.0</small>"]
    P2 --> P1
    P3 --> P1
    P4 --> P1
    P5 --> P2
    P5 --> P1
    click P1 "#srccadtoolcorecadtoolcorecsproj"
    click P2 "#srccadtoolgeometrycadtoolgeometrycsproj"
    click P3 "#srccadtoolinfrastructurecadtoolinfrastructurecsproj"
    click P4 "#testscadtoolcoretestscadtoolcoretestscsproj"
    click P5 "#testscadtoolgeometrytestscadtoolgeometrytestscsproj"

```

## Project Details

<a id="srccadtoolcorecadtoolcorecsproj"></a>
### src\CadTool.Core\CadTool.Core.csproj

#### Project Info

- **Current Target Framework:** net8.0
- **Proposed Target Framework:** net10.0
- **SDK-style**: True
- **Project Kind:** ClassLibrary
- **Dependencies**: 0
- **Dependants**: 4
- **Number of Files**: 16
- **Number of Files with Incidents**: 1
- **Lines of Code**: 807
- **Estimated LOC to modify**: 0+ (at least 0,0% of the project)

#### Dependency Graph

Legend:
📦 SDK-style project
⚙️ Classic project

```mermaid
flowchart TB
    subgraph upstream["Dependants (4)"]
        P2["<b>📦&nbsp;CadTool.Geometry.csproj</b><br/><small>net8.0</small>"]
        P3["<b>📦&nbsp;CadTool.Infrastructure.csproj</b><br/><small>net8.0</small>"]
        P4["<b>📦&nbsp;CadTool.Core.Tests.csproj</b><br/><small>net8.0</small>"]
        P5["<b>📦&nbsp;CadTool.Geometry.Tests.csproj</b><br/><small>net8.0</small>"]
        click P2 "#srccadtoolgeometrycadtoolgeometrycsproj"
        click P3 "#srccadtoolinfrastructurecadtoolinfrastructurecsproj"
        click P4 "#testscadtoolcoretestscadtoolcoretestscsproj"
        click P5 "#testscadtoolgeometrytestscadtoolgeometrytestscsproj"
    end
    subgraph current["CadTool.Core.csproj"]
        MAIN["<b>📦&nbsp;CadTool.Core.csproj</b><br/><small>net8.0</small>"]
        click MAIN "#srccadtoolcorecadtoolcorecsproj"
    end
    P2 --> MAIN
    P3 --> MAIN
    P4 --> MAIN
    P5 --> MAIN

```

### API Compatibility

| Category | Count | Impact |
| :--- | :---: | :--- |
| 🔴 Binary Incompatible | 0 | High - Require code changes |
| 🟡 Source Incompatible | 0 | Medium - Needs re-compilation and potential conflicting API error fixing |
| 🔵 Behavioral change | 0 | Low - Behavioral changes that may require testing at runtime |
| ✅ Compatible | 1025 |  |
| ***Total APIs Analyzed*** | ***1025*** |  |

<a id="srccadtoolgeometrycadtoolgeometrycsproj"></a>
### src\CadTool.Geometry\CadTool.Geometry.csproj

#### Project Info

- **Current Target Framework:** net8.0
- **Proposed Target Framework:** net10.0
- **SDK-style**: True
- **Project Kind:** ClassLibrary
- **Dependencies**: 1
- **Dependants**: 1
- **Number of Files**: 3
- **Number of Files with Incidents**: 1
- **Lines of Code**: 131
- **Estimated LOC to modify**: 0+ (at least 0,0% of the project)

#### Dependency Graph

Legend:
📦 SDK-style project
⚙️ Classic project

```mermaid
flowchart TB
    subgraph upstream["Dependants (1)"]
        P5["<b>📦&nbsp;CadTool.Geometry.Tests.csproj</b><br/><small>net8.0</small>"]
        click P5 "#testscadtoolgeometrytestscadtoolgeometrytestscsproj"
    end
    subgraph current["CadTool.Geometry.csproj"]
        MAIN["<b>📦&nbsp;CadTool.Geometry.csproj</b><br/><small>net8.0</small>"]
        click MAIN "#srccadtoolgeometrycadtoolgeometrycsproj"
    end
    subgraph downstream["Dependencies (1"]
        P1["<b>📦&nbsp;CadTool.Core.csproj</b><br/><small>net8.0</small>"]
        click P1 "#srccadtoolcorecadtoolcorecsproj"
    end
    P5 --> MAIN
    MAIN --> P1

```

### API Compatibility

| Category | Count | Impact |
| :--- | :---: | :--- |
| 🔴 Binary Incompatible | 0 | High - Require code changes |
| 🟡 Source Incompatible | 0 | Medium - Needs re-compilation and potential conflicting API error fixing |
| 🔵 Behavioral change | 0 | Low - Behavioral changes that may require testing at runtime |
| ✅ Compatible | 55 |  |
| ***Total APIs Analyzed*** | ***55*** |  |

<a id="srccadtoolinfrastructurecadtoolinfrastructurecsproj"></a>
### src\CadTool.Infrastructure\CadTool.Infrastructure.csproj

#### Project Info

- **Current Target Framework:** net8.0
- **Proposed Target Framework:** net10.0
- **SDK-style**: True
- **Project Kind:** ClassLibrary
- **Dependencies**: 1
- **Dependants**: 0
- **Number of Files**: 1
- **Number of Files with Incidents**: 1
- **Lines of Code**: 39
- **Estimated LOC to modify**: 0+ (at least 0,0% of the project)

#### Dependency Graph

Legend:
📦 SDK-style project
⚙️ Classic project

```mermaid
flowchart TB
    subgraph current["CadTool.Infrastructure.csproj"]
        MAIN["<b>📦&nbsp;CadTool.Infrastructure.csproj</b><br/><small>net8.0</small>"]
        click MAIN "#srccadtoolinfrastructurecadtoolinfrastructurecsproj"
    end
    subgraph downstream["Dependencies (1"]
        P1["<b>📦&nbsp;CadTool.Core.csproj</b><br/><small>net8.0</small>"]
        click P1 "#srccadtoolcorecadtoolcorecsproj"
    end
    MAIN --> P1

```

### API Compatibility

| Category | Count | Impact |
| :--- | :---: | :--- |
| 🔴 Binary Incompatible | 0 | High - Require code changes |
| 🟡 Source Incompatible | 0 | Medium - Needs re-compilation and potential conflicting API error fixing |
| 🔵 Behavioral change | 0 | Low - Behavioral changes that may require testing at runtime |
| ✅ Compatible | 27 |  |
| ***Total APIs Analyzed*** | ***27*** |  |

<a id="testscadtoolcoretestscadtoolcoretestscsproj"></a>
### tests\CadTool.Core.Tests\CadTool.Core.Tests.csproj

#### Project Info

- **Current Target Framework:** net8.0
- **Proposed Target Framework:** net10.0
- **SDK-style**: True
- **Project Kind:** DotNetCoreApp
- **Dependencies**: 1
- **Dependants**: 0
- **Number of Files**: 8
- **Number of Files with Incidents**: 1
- **Lines of Code**: 535
- **Estimated LOC to modify**: 0+ (at least 0,0% of the project)

#### Dependency Graph

Legend:
📦 SDK-style project
⚙️ Classic project

```mermaid
flowchart TB
    subgraph current["CadTool.Core.Tests.csproj"]
        MAIN["<b>📦&nbsp;CadTool.Core.Tests.csproj</b><br/><small>net8.0</small>"]
        click MAIN "#testscadtoolcoretestscadtoolcoretestscsproj"
    end
    subgraph downstream["Dependencies (1"]
        P1["<b>📦&nbsp;CadTool.Core.csproj</b><br/><small>net8.0</small>"]
        click P1 "#srccadtoolcorecadtoolcorecsproj"
    end
    MAIN --> P1

```

### API Compatibility

| Category | Count | Impact |
| :--- | :---: | :--- |
| 🔴 Binary Incompatible | 0 | High - Require code changes |
| 🟡 Source Incompatible | 0 | Medium - Needs re-compilation and potential conflicting API error fixing |
| 🔵 Behavioral change | 0 | Low - Behavioral changes that may require testing at runtime |
| ✅ Compatible | 517 |  |
| ***Total APIs Analyzed*** | ***517*** |  |

<a id="testscadtoolgeometrytestscadtoolgeometrytestscsproj"></a>
### tests\CadTool.Geometry.Tests\CadTool.Geometry.Tests.csproj

#### Project Info

- **Current Target Framework:** net8.0
- **Proposed Target Framework:** net10.0
- **SDK-style**: True
- **Project Kind:** DotNetCoreApp
- **Dependencies**: 2
- **Dependants**: 0
- **Number of Files**: 5
- **Number of Files with Incidents**: 1
- **Lines of Code**: 202
- **Estimated LOC to modify**: 0+ (at least 0,0% of the project)

#### Dependency Graph

Legend:
📦 SDK-style project
⚙️ Classic project

```mermaid
flowchart TB
    subgraph current["CadTool.Geometry.Tests.csproj"]
        MAIN["<b>📦&nbsp;CadTool.Geometry.Tests.csproj</b><br/><small>net8.0</small>"]
        click MAIN "#testscadtoolgeometrytestscadtoolgeometrytestscsproj"
    end
    subgraph downstream["Dependencies (2"]
        P2["<b>📦&nbsp;CadTool.Geometry.csproj</b><br/><small>net8.0</small>"]
        P1["<b>📦&nbsp;CadTool.Core.csproj</b><br/><small>net8.0</small>"]
        click P2 "#srccadtoolgeometrycadtoolgeometrycsproj"
        click P1 "#srccadtoolcorecadtoolcorecsproj"
    end
    MAIN --> P2
    MAIN --> P1

```

### API Compatibility

| Category | Count | Impact |
| :--- | :---: | :--- |
| 🔴 Binary Incompatible | 0 | High - Require code changes |
| 🟡 Source Incompatible | 0 | Medium - Needs re-compilation and potential conflicting API error fixing |
| 🔵 Behavioral change | 0 | Low - Behavioral changes that may require testing at runtime |
| ✅ Compatible | 168 |  |
| ***Total APIs Analyzed*** | ***168*** |  |

