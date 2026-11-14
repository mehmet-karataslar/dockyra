# Dockyra — Project Overview

## 1. Identity & Purpose
**Dockyra** is a modular, customizable Windows 11 taskbar and desktop workspace management application. It provides power users, developers, creators, and everyday desktop users with a flexible, organized, and resource-efficient alternative/companion to the inflexible native Windows 11 taskbar.

## 2. Core Problem Solved
The standard Windows 11 taskbar restricts user control:
- It lacks native application categorization, custom named groups, and expandable taskbar folders.
- It cannot switch pinned application layouts automatically or manually across distinct workflows (e.g., *Software Development*, *Communication/Office*, *Gaming*).
- It offers limited control over section partitioning, dock edge placement, and power-aware visual behavior.

Dockyra solves these limitations without resorting to brittle shell hacks, DLL injection, or terminating `explorer.exe`.

## 3. Target Users
- **Software Developers & Engineers**: Need rapid switching between IDEs, terminals, browsers, and documentation with profile-based taskbar layouts.
- **Designers & Content Creators**: Organize creative suites, asset tools, and reference windows into structured visual groups and folders.
- **Multi-Monitor & Laptop Power Users**: Require per-monitor taskbar organization alongside strict battery and CPU efficiency.
- **Everyday Windows 11 Users**: Want intuitive drag-and-drop folders and clean visual organization without complex technical configuration.

## 4. Core Capabilities
1. **Modular Taskbar Layout**: Configurable dock edge (`Bottom`, `Top`, `Left`, `Right`), section alignment (`Left`, `Center`, `Right`), icon sizes, spacing, corner radius, and opacity.
2. **Dynamic Application Groups & Folders**: Named, colored, icon-labeled groups with expandable flyout folders (grid/list view, instant search, max-depth-bounded nested folders).
3. **Smart Auto-Classification**: Rule engine (`SmartGroupingRuleEngine`) that suggests or assigns applications to groups by executable name, process path, or window title while strictly prioritizing user-defined rules.
4. **Event-Driven Window Tracking**: Tracks visible top-level desktop windows via Win32 + DWM (`DWMWA_CLOAKED` filtering), highlights the active foreground window, and switches/minimizes/restores/closes windows safely.
5. **Work Profiles**: Save, switch, import, and export complete taskbar layouts (*Development*, *Daily Use*, *Gaming*).
6. **Adaptive Power & Performance Management**: Three operating modes (`PowerSaver`, `Balanced`, `Performance`) linked to Windows AC/Battery notifications.
7. **Safe Coexistence & Recovery**: Runs as a standard user process (`asInvoker`) with automatic Safe Mode fallback if consecutive startup crashes occur.

## 5. Technology Stack
| Layer / Concern | Technology |
|---|---|
| Language & Runtime | C# 13 / .NET 10 LTS (`net10.0` & `net10.0-windows`) |
| Desktop UI | WPF with Windows 11 Fluent Design Tokens (`Dockyra.Presentation.Wpf`) |
| Architecture | Clean Architecture + Modular Monolith + MVVM (`CommunityToolkit.Mvvm`) |
| OS Integration | Win32 API, DWM API, Shell API via P/Invoke (`Dockyra.WindowsIntegration`) |
| Persistence | Local SQLite (`Microsoft.Data.Sqlite`) + schema-versioned JSON export/import |
| Dependency Injection | `Microsoft.Extensions.DependencyInjection` |
| Logging | `Microsoft.Extensions.Logging` + `Serilog` (rolling local files) |
| Automated Testing | `xUnit` across Unit, Integration, UI, Architecture, and Performance suites |

## 6. Architectural Principles
- **Strict UI & OS Separation**: `Dockyra.Presentation.Wpf` has zero direct dependency on `Dockyra.WindowsIntegration` or `Dockyra.Infrastructure`.
- **WinUI 3 Portability**: Domain, Application, Contracts, Infrastructure, and WindowsIntegration are decoupled from WPF so a future `Dockyra.Presentation.WinUI` layer can reuse 100% of core logic and OS adapters.
- **Event-Driven Efficiency**: Coalesced OS events instead of high-frequency polling loops.

## 7. Performance Budgets (Optimization Targets)
- **Idle CPU**: `< 0.5%` average on standard Windows 11 hardware.
- **Memory (Working Set)**: `< 150 MB` in typical daily usage.
- **GPU**: Zero continuous background animation overhead when idle or in `PowerSaver` mode.

## 8. Supported Platforms & Long-Term Vision
- **Primary Platform**: Windows 11 (22H2 / 23H2 / 24H2+, x64 and ARM64).
- **Long-Term Vision**: Evolve Dockyra into a complete, extensible desktop workspace shell supporting optional WinUI 3 presentation, workspace automation triggers, and sandboxed widget modules.

