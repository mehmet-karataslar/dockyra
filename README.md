# Dockyra

**Your desktop. Your rules.**

**Dockyra** is a Windows 11 desktop application project designed to rethink how people use their taskbar. Instead of treating the taskbar as a fixed row of icons, Dockyra envisions a **personal, modular workspace** where applications, folders, active windows, shortcuts, and system tools are organized around the way you work.

Built with **C#**, **.NET 10**, **WPF**, and Windows system APIs, Dockyra aims to combine a native-feeling Windows experience with flexible layouts, focused workflows, and resource-conscious operation.

> **Project status:** Dockyra is under development. The capabilities described below represent the product vision and planned scope; they should not be interpreted as a claim that every feature is already implemented. See [`planning/CURRENT-STATE.md`](planning/CURRENT-STATE.md) for the current implementation status.

---

## Why Dockyra?

The Windows taskbar is essential to everyday computing, but it offers limited freedom to organize different kinds of work. People who use development tools, communication apps, browsers, design software, and games often need more than a single list of pinned and running applications.

Dockyra is being designed to answer a simple question:

**What if your taskbar could adapt to your workflow instead of forcing your workflow to adapt to it?**

Dockyra's goal is to make desktop navigation more intentional: fewer scattered icons, faster access to relevant tools, and a layout you control.

### Who is it for?

- **Developers:** Keep IDEs, terminals, repositories, browsers, and utilities together.
- **Designers and creators:** Group editing, design, asset, and communication tools.
- **Students and professionals:** Switch between study, work, meeting, and personal setups.
- **Power users:** Customize placement, grouping, shortcuts, and multi-display behavior.
- **Anyone who wants a cleaner desktop:** Reduce visual clutter without sacrificing access.

---

## What Dockyra aims to offer

### 1. A taskbar you can shape

Dockyra aims to provide a customizable desktop bar with configurable position, sections, icon sizes, spacing, alignment, transparency, and auto-hide behavior. The goal is a cohesive Windows 11 experience—not simply a visual imitation of another dock.

Planned options include positioning the bar on the **bottom, top, left, or right** edge, arranging independent sections, and preserving layouts between sessions.

### 2. Application groups and folders

Organize shortcuts into meaningful groups, such as **Development**, **Communication**, **Design**, **Office**, or **Gaming**. A group can act as a compact entry point to a flyout containing its apps, with planned support for search, drag-and-drop ordering, and grid or list views.

Example workspace:

```text
[ Start ]  [ Development ▾ ]  [ Communication ▾ ]  [ Running Apps ]  [ System ]

Development
  ├── Visual Studio Code
  ├── Windows Terminal
  ├── Browser
  └── Git Tools

Communication
  ├── Teams
  ├── Discord
  └── Messaging
```

Future intelligent grouping rules may suggest categories based on application identity while keeping the user's own choices authoritative.

### 3. Live window management

Beyond launching pinned shortcuts, Dockyra is designed to show and manage active application windows. Planned interactions include identifying the focused window, switching between windows, grouping multiple windows of the same app, and exposing common actions such as minimize, restore, and close.

The Windows integration layer will use **documented Win32, Shell, and DWM capabilities where appropriate**, with attention to window lifecycle events and the restrictions Windows places on focus changes.

### 4. Workspaces and profiles

A single layout rarely fits every task. Dockyra plans to support saved profiles that can be switched without rebuilding the taskbar manually.

For example:

| Profile | Typical tools | Goal |
| --- | --- | --- |
| Development | IDE, terminal, browser, Git tools | Keep engineering tools within reach |
| Daily | Browser, files, messaging | Simplify everyday use |
| Creative | Design, editing, media tools | Focus on content creation |
| Gaming | Game launchers, voice chat, utilities | Reduce unnecessary clutter |

Each profile may define its own groups, pinned items, appearance, and layout rules.

### 5. Modern Windows 11 design

Dockyra targets a polished interface inspired by **Fluent Design**, including consistent typography, rounded corners, clear states, light/dark themes, and appropriate use of native-feeling surfaces.

Visual effects must remain optional and responsive. Accessibility, keyboard navigation, scaling, and reduced-motion behavior are part of the design goals—not afterthoughts.

### 6. Power-aware performance

A taskbar stays open all day. Dockyra is therefore designed with **low idle activity** as a core requirement.

The intended approach prioritizes event-driven updates instead of constant high-frequency polling. It will also consider lazy-loaded previews, icon caching, and adjustable visual effects.

Planned modes:

| Mode | Intended behavior |
| --- | --- |
| **Power Saver** | Minimize optional animations and background work |
| **Balanced** | Combine responsive interaction with restrained effects |
| **Performance** | Allow richer visual effects when desired |

Windows power-state notifications can help Dockyra adapt when moving between AC power and battery. Actual CPU, memory, GPU, and energy usage must be verified through benchmarks; no fixed savings are promised.

### 7. Multiple displays, one coherent experience

Dockyra plans to account for multi-monitor configurations, different DPI scaling factors, work areas, and changing monitor arrangements. The intended experience is predictable positioning and preserved user preferences, including after external monitors are disconnected or reconnected.

### 8. Safe operation and recovery

Dockyra is intended to be **reversible**. Rather than modifying or replacing protected Windows system files, the project favors a standalone taskbar surface that can coexist with Windows components and provide a reliable way back to the original desktop experience.

The design explicitly avoids depending on terminating `explorer.exe`, injecting code into system processes, or making destructive registry changes.

> **Windows integration note:** The Windows 11 system tray, Start menu, notification center, and Quick Settings are not fully replaceable through a simple set of public APIs. Dockyra will document such limitations and preserve access to native Windows functionality instead of pretending those integrations are universally supported.

---

## What makes Dockyra different?

Dockyra is intended to be more than an icon launcher. Its design combines:

- **Organization:** Application folders, configurable sections, and focused work profiles.
- **Desktop awareness:** Interaction with running windows rather than only pinned shortcuts.
- **Customization:** A layout system built around user preferences.
- **Efficiency:** Event-driven updates and power-aware behavior.
- **Windows-first engineering:** Native APIs, DPI awareness, and recovery-oriented design.
- **Long-term maintainability:** Clear module boundaries and documented architectural decisions.

The goal is not to replicate every internal Windows taskbar feature. It is to provide an extensible, practical alternative where customization and predictable behavior matter most.

---

## Technology and architecture

Dockyra uses a **modular Clean Architecture** approach with **MVVM** for its WPF presentation layer.

| Area | Technology / approach |
| --- | --- |
| Language | C# |
| Runtime | .NET 10 |
| UI | WPF and Windows 11-inspired Fluent styling |
| UI pattern | MVVM |
| System integration | Win32, Shell, DWM, and Windows notifications |
| Local storage | SQLite |
| Dependency management | Dependency Injection |
| Automated testing | Unit, integration, architecture, UI, and performance tests |
| Engineering documentation | `planning/` and architecture decision records |
| AI-assisted workflows | `AGENTS.md` and `.agents/` |

### Why separate UI from Windows integration?

A central architectural decision is that **WPF screens must not contain Windows API integration logic**.

The responsibilities are divided as follows:

- **Domain:** Application concepts and business rules, independent of WPF and Win32.
- **Application:** Use cases and interfaces for the services those use cases need.
- **Infrastructure:** Persistence, logging, settings, caching, and other technical implementations.
- **WindowsIntegration:** Adapters for window management, monitors, power events, and Shell behavior.
- **Presentation.Wpf:** Views, ViewModels, styles, localization, and interactions.
- **Desktop:** Startup, service registration, and application lifecycle.

This separation is intentional: a future **WinUI 3 interface** should be possible without rewriting the core business rules, database layer, and most Windows integration services. Some UI-specific adaptation would still be required.

### Repository layout

```text
Dockyra/
├── AGENTS.md                     # Common instructions for AI coding agents
├── Dockyra.sln                   # .NET solution
├── src/
│   ├── Dockyra.Domain/          # Entities, value objects, business rules
│   ├── Dockyra.Contracts/       # Shared contracts between modules/layers
│   ├── Dockyra.Application/     # Use cases and application interfaces
│   ├── Dockyra.Infrastructure/  # Database, logging, settings, backups
│   ├── Dockyra.WindowsIntegration/ # Windows API and Shell adapters
│   ├── Dockyra.Modules/         # Feature modules and registration
│   ├── Dockyra.Presentation.Wpf/ # Views, ViewModels, styles, localization
│   └── Dockyra.Desktop/         # Application entry point and DI composition
├── tests/
│   ├── ArchitectureTests/
│   ├── UnitTests/
│   ├── IntegrationTests/
│   ├── UITests/
│   └── PerformanceTests/
├── planning/                     # Product plans and persistent project knowledge
├── .agents/                      # Agent roles, rules, and reusable skills
├── scripts/                      # Development and validation scripts
├── docs/                         # Additional technical documentation
└── installer/                    # Packaging and installation work
```

The tree represents the **intended repository organization**. Consult the actual repository and current-state documents to confirm which directories and tools are present.

---

## Planning and AI-assisted development

Dockyra is designed to remain understandable across development sessions and AI coding tools. The project maintains structured engineering knowledge alongside its source code.

### `planning/`: the project's engineering memory

This directory is intended to answer *what Dockyra is, what exists, why decisions were made, what comes next, and where a feature belongs*.

Key references:

| File | Purpose |
| --- | --- |
| [`planning/PROJECT-OVERVIEW.md`](planning/PROJECT-OVERVIEW.md) | Product mission, scope, and goals |
| [`planning/CURRENT-STATE.md`](planning/CURRENT-STATE.md) | Latest verified implementation status |
| [`planning/ROADMAP.md`](planning/ROADMAP.md) | Development phases and milestones |
| [`planning/modules/module-registry.yaml`](planning/modules/module-registry.yaml) | Module ownership, source paths, and dependencies |
| [`planning/architecture/`](planning/architecture/) | Architecture and system behavior |
| [`planning/decisions/`](planning/decisions/) | Architecture Decision Records (ADRs) |
| [`planning/tasks/`](planning/tasks/) | Work items, status, and acceptance criteria |
| [`planning/handoffs/`](planning/handoffs/) | Context for future contributors or agents |

### `.agents/`: consistent development workflows

The `.agents/` directory is intended to host specialized agent descriptions, development rules, and repeatable skills. Example responsibilities include architecture reviews, WPF implementation, Windows integration, testing, performance analysis, and documentation updates.

The shared entry point is [`AGENTS.md`](AGENTS.md). AI tools may differ in how they discover agent-specific files, so the Markdown documentation in `planning/` remains tool-independent.

**Typical contribution flow:** understand the current state → locate the relevant module → plan the change → implement it within the correct layer → validate behavior → update documentation.

---

## Roadmap

The intended development sequence is:

| Phase | Focus |
| --- | --- |
| **1. Foundation** | Solution setup, architecture, dependency injection, logging, storage, tests |
| **2. Core taskbar** | Desktop bar, window discovery, focus handling, positioning |
| **3. Organization** | Groups, folders, pinned apps, drag-and-drop, saved layouts |
| **4. User experience** | Fluent UI, settings, themes, localization, accessibility |
| **5. Windows integration** | Multi-monitor behavior, DPI, auto-hide, startup, recovery |
| **6. Optimization** | Power modes, profiling, caching, long-running stability |
| **7. Release** | Installer, compatibility testing, documentation, versioning |

The exact scope and delivery status are maintained in [`planning/ROADMAP.md`](planning/ROADMAP.md), not inferred from this overview.

---

## Getting started for developers

### Prerequisites

- Windows 11
- .NET 10 SDK
- A Windows development environment with the required desktop build tools
- Git

### Build and run

From the repository root, use the following commands **if the corresponding solution, projects, and scripts are present**:

```powershell
# Restore dependencies and build
dotnet build Dockyra.sln

# Run automated tests
dotnet test Dockyra.sln

# Validate planning documents
pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/validate-planning.ps1

# Start the desktop app
dotnet run --project src/Dockyra.Desktop/Dockyra.Desktop.csproj
```

Windows-specific functionality must be validated on a supported Windows 11 system. A passing build alone does not confirm correct taskbar integration.

### How to contribute

1. Read [`AGENTS.md`](AGENTS.md) and [`planning/CURRENT-STATE.md`](planning/CURRENT-STATE.md).
2. Find the relevant feature in [`planning/modules/module-registry.yaml`](planning/modules/module-registry.yaml).
3. Review the architecture boundaries and existing tests.
4. Implement the smallest coherent change, without mixing UI code and Windows API logic.
5. Build, test, record any limitations, and update the related planning documents.

Please avoid introducing unsupported Shell hacks or changes that prevent users from returning to the standard Windows desktop.

---

## Principles

**User control first.** The desktop should be customizable without becoming fragile.

**Fast when idle.** A taskbar should consume as few resources as practical when nothing changes.

**Modularity over shortcuts.** Features should be easy to extend without creating tightly coupled code.

**Safe by design.** Recovery and coexistence with native Windows components are essential.

**Truthful engineering.** Benchmarks, compatibility claims, and feature statuses should reflect actual tests.

---

## Project information

- **Project:** Dockyra
- **Platform:** Windows 11
- **Category:** Taskbar customization and desktop workspace management
- **Primary language:** C#
- **UI framework:** WPF
- **Status and availability:** See the repository's current-state and release documents
- **License:** See `LICENSE` if one has been added; otherwise licensing terms have not yet been specified

---

**Dockyra — Your desktop. Your rules.**
