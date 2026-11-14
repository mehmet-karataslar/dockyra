# Dockyra

**Dockyra** is a modern, modular, high-performance Windows 11 taskbar and desktop workspace management application built with **C#**, **.NET 10**, **WPF (Fluent Design)**, **Win32/Shell APIs**, and **SQLite** following strict **Clean Architecture** and **MVVM** principles.

---

## Key Capabilities

- **Customizable Taskbar Layout**: Configurable dock edge (bottom, top, left, right), centered or aligned sections, icon sizing, spacing, opacity, and rounded Fluent aesthetics.
- **Dynamic Application Groups & Folders**: Organize applications into named, color-coded, icon-tagged groups and flyout folders with search, grid/list views, and smart auto-classification rules.
- **Event-Driven Window Management**: Track open desktop windows via Win32 & DWM APIs (filtering cloaked/background shell frames), switch focus safely, minimize/restore/close windows, and highlight active windows.
- **Work Profiles**: Switch taskbar layouts and pinned application groups across contexts (e.g., *Software Development*, *Daily Use*, *Gaming*).
- **Adaptive Power & Performance Modes**: Built-in *Power Saver*, *Balanced*, and *Performance* modes responding to Windows battery/AC state changes with minimal idle CPU and memory footprint.
- **Multi-Monitor & Per-Monitor DPI Awareness**: Enumerate physical displays, track work areas, and preserve layout integrity across display changes.
- **Safe Coexistence & Recovery**: Runs safely alongside Windows 11 without terminating `explorer.exe`, injecting DLLs, or making destructive registry modifications. Includes automatic Safe Mode recovery.

---

## Repository Structure

Dockyra is engineered around three integrated pillars:

```text
Dockyra/
├── AGENTS.md                  # AI Agent entry point & mandatory rules
├── Dockyra.sln                # .NET 10 Solution
├── src/
│   ├── Dockyra.Domain/             # Pure business entities, value objects & rules (0 dependencies)
│   ├── Dockyra.Contracts/          # Stable cross-layer & cross-module contracts
│   ├── Dockyra.Application/        # Use cases, orchestration services & repository interfaces
│   ├── Dockyra.Infrastructure/     # SQLite persistence, migrations, JSON export/import, Serilog
│   ├── Dockyra.WindowsIntegration/ # Win32, DWM, Shell, Monitor & Power API adapters
│   ├── Dockyra.Modules/            # Modular feature registrations & fault-isolated module host
│   ├── Dockyra.Presentation.Wpf/   # WPF Views, ViewModels, Fluent DesignTokens & Localization
│   └── Dockyra.Desktop/            # Composition Root, DI bootstrapper & application lifecycle
├── tests/
│   ├── ArchitectureTests/          # Layer dependency & planning documentation verification
│   ├── UnitTests/                  # Domain rules, Application use cases & ViewModel tests
│   ├── IntegrationTests/           # SQLite repositories, migrations & Win32 adapter tests
│   ├── UITests/                    # WPF resource, theme & localization tests
│   └── PerformanceTests/           # Event coalescing, throughput & memory allocation tests
├── planning/                  # Permanent engineering memory (overview, state, roadmap, ADRs, tasks)
├── .agents/                   # Specialized AI agents, rules & procedural skills
└── scripts/                   # Build, test & planning validation scripts
```

---

## Quick Start

### Prerequisites
- Windows 11 (x64 / ARM64)
- .NET 10 SDK (`10.0.100` or newer)

### Build, Test & Run

```powershell
# 1. Restore and build the solution
dotnet build Dockyra.sln

# 2. Run all automated tests (Unit, Integration, UI, Performance, Architecture)
dotnet test Dockyra.sln

# 3. Validate planning & agent documentation integrity
pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/validate-planning.ps1

# 4. Run the Dockyra desktop application
dotnet run --project src/Dockyra.Desktop/Dockyra.Desktop.csproj
```

---

## Documentation & Governance Map

- **AI Agent Instructions**: [AGENTS.md](AGENTS.md)
- **Planning Knowledge Base**: [planning/README.md](planning/README.md)
- **Project Identity & Vision**: [planning/PROJECT-OVERVIEW.md](planning/PROJECT-OVERVIEW.md)
- **Live Engineering State**: [planning/CURRENT-STATE.md](planning/CURRENT-STATE.md)
- **Architecture Documentation**: [ARCHITECTURE.md](ARCHITECTURE.md) / [planning/architecture/README.md](planning/architecture/README.md)
- **Module Registry**: [planning/modules/module-registry.yaml](planning/modules/module-registry.yaml)
- **Development Roadmap**: [ROADMAP.md](ROADMAP.md) / [planning/ROADMAP.md](planning/ROADMAP.md)
- **Task Tracking**: [TASKS.md](TASKS.md) / [planning/tasks/README.md](planning/tasks/README.md)
- **Changelog**: [CHANGELOG.md](CHANGELOG.md)

