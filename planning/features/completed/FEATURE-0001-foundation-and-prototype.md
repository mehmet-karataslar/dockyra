# FEATURE-0001: Modular Taskbar Foundation, SQLite Persistence & Window Tracking Prototype

- **Status**: `COMPLETED`
- **Modules**: `MOD-CORE`, `MOD-TASKBAR`, `MOD-GROUPS`, `MOD-FOLDERS`, `MOD-WINDOWS`, `MOD-PROFILES`, `MOD-THEMES`, `MOD-SETTINGS`, `MOD-PERFORMANCE`, `MOD-POWER`
- **Related Tasks**: `TASK-0001` through `TASK-0007`

## 1. Purpose
Deliver the working foundation of Dockyra on `.NET 10` with Clean Architecture, SQLite persistence, Win32/DWM window tracking, smart application grouping, folder flyout search, work profile switching, and adaptive power modes.

## 2. Acceptance Criteria
- [x] `Dockyra.Presentation.Wpf` decoupled from `Dockyra.WindowsIntegration` and `Dockyra.Infrastructure`.
- [x] Open windows tracked via `Win32WindowManager` with `DWMWA_CLOAKED` filtering and `EventCoalescer`.
- [x] Groups, layouts, profiles, and settings persisted in SQLite (`DatabaseMigrator` + repositories).
- [x] Working WPF `TaskbarWindow` with group flyout search, profile switching, and power mode switching.

