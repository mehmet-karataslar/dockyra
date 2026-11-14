# Changelog

All notable changes to the **Dockyra** project are documented in this file and synchronized with [`planning/CHANGELOG.md`](planning/CHANGELOG.md).

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

## [0.1.0] - 2026-11-14

### Added
- **Clean Architecture Solution (`Dockyra.sln`)**: Created 8 source projects (`Dockyra.Domain`, `Dockyra.Contracts`, `Dockyra.Application`, `Dockyra.Infrastructure`, `Dockyra.WindowsIntegration`, `Dockyra.Modules`, `Dockyra.Presentation.Wpf`, `Dockyra.Desktop`) and 5 automated test projects (`UnitTests`, `IntegrationTests`, `UITests`, `ArchitectureTests`, `PerformanceTests`) targeting `.NET 10`.
- **Strict UI / OS Decoupling**: Abstracted all Win32, DWM, Monitor, Power, and Process Launch operations behind platform-agnostic contracts (`IWindowManager`, `IMonitorService`, `IPowerMonitor`, `IApplicationLauncher`, `ITaskbarVisibilityService`).
- **Domain & Smart Grouping Engine**: Implemented `TaskbarLayout`, `TaskbarSection`, `ApplicationGroup`, `FolderItem`, `WorkProfile`, `UserPreferences`, `SmartGroupingRuleEngine`, `FolderHierarchyRules`, and `TaskbarLayoutValidator`.
- **SQLite Persistence & Migrations**: Implemented versioned SQLite schema migration (`DatabaseMigrator`), repositories for layouts, groups, profiles, and user settings, plus versioned JSON export/import and `FileSafeModeStateStore`.
- **Windows 11 Integration Layer**: Built `Win32WindowManager` (using `EnumWindows`, `DwmGetWindowAttribute` cloak detection, `SetForegroundWindow`, `ShowWindow`, and `SetWinEventHook` event tracking), `Win32MonitorService`, `Win32PowerMonitor`, and `Win32ApplicationLauncher`.
- **WPF Fluent Taskbar Shell**: Created the working taskbar dock window (`TaskbarWindow`), `TaskbarViewModel`, live open-window items, application group pills, folder flyout panel, power mode selector, Fluent DesignTokens (`Colors`, `Spacing`, `Radius`, `Typography`), Dark/Light themes, and Turkish/English localization dictionaries.
- **Planning Memory System (`planning/`)**: Established complete documentation covering overview, live state, roadmap, 9 module specs + `module-registry.yaml`, 7 phase specs, ADRs, workflows, quality budgets, and handoff records.
- **AI Agent System (`AGENTS.md`, `.agents/`)**: Created 9 specialized agents, 6 permanent rule files, and 7 procedural skills (`SKILL.md`).
- **Automated Governance Verification**: Added `Dockyra.ArchitectureTests` and `scripts/validate-planning.ps1` to verify layer dependencies, module registry integrity, task IDs, and markdown links.

