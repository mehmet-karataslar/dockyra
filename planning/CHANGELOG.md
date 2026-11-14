# Dockyra Planning & Engineering Changelog (`planning/CHANGELOG.md`)

## [0.1.0] - 2026-11-14

### Architecture & Governance
- Established Clean Architecture solution `Dockyra.sln` on `.NET 10` (`SDK 10.0.301`).
- Enforced strict separation between `Dockyra.Presentation.Wpf` and `Dockyra.WindowsIntegration` via `Dockyra.ArchitectureTests`.
- Created `planning/` permanent engineering memory system, `planning/modules/module-registry.yaml`, and `scripts/validate-planning.ps1`.
- Created `AGENTS.md`, 9 specialized agents in `.agents/agents/`, 6 always-on rules in `.agents/rules/`, and 7 procedural skills in `.agents/skills/`.

### Application & Modules
- Implemented `Dockyra.Domain` entities (`TaskbarLayout`, `TaskbarSection`, `ApplicationGroup`, `FolderItem`, `WorkProfile`, `UserPreferences`, `ClassificationRule`) and rules (`SmartGroupingRuleEngine`, `FolderHierarchyRules`, `TaskbarLayoutValidator`).
- Implemented `Dockyra.Infrastructure` SQLite persistence (`DatabaseMigrator`, repositories), schema-versioned JSON settings backup/restore, and `FileSafeModeStateStore`.
- Implemented `Dockyra.WindowsIntegration` (`Win32WindowManager`, `Win32MonitorService`, `Win32PowerMonitor`, `Win32ApplicationLauncher`, `TaskbarCoexistenceService`).
- Implemented `Dockyra.Presentation.Wpf` and `Dockyra.Desktop` working taskbar prototype with live open-window tracking, group pills, folder flyout search, profile switching, power mode switching, Fluent Dark/Light themes, and `tr-TR`/`en-US` localization.

