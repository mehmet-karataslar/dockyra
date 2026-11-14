# Dockyra — Current Engineering State (`CURRENT-STATE.md`)

> **Last Updated**: 2026-11-14
> **Current Phase**: `PHASE-01` (Foundation Complete) & `PHASE-02` / `PHASE-03` (Core Taskbar, Window Tracking & Grouping Prototype Active)
> **Latest Handoff**: [`planning/handoffs/HANDOFF-2026-11-14-initial-foundation.md`](handoffs/HANDOFF-2026-11-14-initial-foundation.md)

---

## 1. Executive Summary
Dockyra has been initialized from scratch on `.NET 10` (`SDK 10.0.301`) on Windows 11 (`10.0.26300`) with all three foundational pillars operational:
1. **Application System (`src/`, `tests/`)**: 8 Clean Architecture source projects and 5 automated test projects (`UnitTests`, `IntegrationTests`, `UITests`, `ArchitectureTests`, `PerformanceTests`).
2. **Planning System (`planning/`)**: Complete architectural documentation, `module-registry.yaml`, ADRs, phase specs, task records, and automated validation script (`scripts/validate-planning.ps1`).
3. **Agent System (`AGENTS.md`, `.agents/`)**: 9 specialized agents, 6 always-on rules, and 7 procedural skills (`SKILL.md`).

---

## 2. Verified vs. Planned Functionality

### Verified & Working Modules
| Module ID | Name | Status | Verified Capabilities |
|---|---|---|---|
| `MOD-CORE` | Core Foundation & Module Host | `active` | Clean Architecture wiring, fault-isolated `ModuleHostService`, `SafeModeStartupCoordinator`, Serilog rolling logger. |
| `MOD-TASKBAR` | Taskbar Shell & Layout | `in-progress` | `TaskbarLayout` domain model, section validation, `TaskbarOrchestrationService`, dual-mode (`NativeOverlay` & `FullReplacement`) WPF `TaskbarWindow` & `TaskbarViewModel`, native `Shell_TrayWnd` geometry docking, and Windows Start Menu trigger. |
| `MOD-GROUPS` | Application Groups | `in-progress` | `ApplicationGroup` entity with 2x2 mini-icon preview (`PreviewApplications`), Android-style drag-one-icon-onto-another folder creation (`CreateFolderFromTwoAppsAsync`), clean removal of hardcoded default groups (`Yazılım`, `Tarayıcılar`, etc.), and SQLite persistence. |
| `MOD-FOLDERS` | Folder Flyouts & Hierarchy | `in-progress` | `FolderItem` tree with max-depth & cycle validation (`FolderHierarchyRules`), Android-style upward popup `FolderFlyoutControl` with inline editable folder title, 1-click app removal/unbox, and pure `.exe` icon extraction (`ExtractIconExW`). |
| `MOD-WINDOWS` | Open Window Tracking | `in-progress` | `Win32WindowManager` using `EnumWindows`, `DwmGetWindowAttribute(DWMWA_CLOAKED)`, `SetWinEventHook` + `EventCoalescer`, direct window/executable icon extraction without shortcut arrows, and Windows 11-style unified pinned + running taskbar item merging (`UnifiedTaskbarAppItem`). |
| `MOD-PROFILES` | Work Profiles | `in-progress` | `WorkProfile` domain model, built-in default profiles (*Software Development*, *Daily Use*, *Gaming*), SQLite persistence, profile switching in `TaskbarViewModel`. |
| `MOD-THEMES` | Fluent Design Tokens & Themes | `in-progress` | Centralized `DesignTokens/` (`Colors`, `Spacing`, `Radius`, `Typography` with WinUI 3 global control templates), 16 bilingual (`tr-TR` / `en-US`) native Mica/Acrylic color palettes, visual color swatches, Dark/Light theme dictionaries, and live localization switching. |
| `MOD-SETTINGS` | Settings & Persistence | `in-progress` | Versioned SQLite migrations (`DatabaseMigrator`), `SqliteUserSettingsRepository`, schema-versioned JSON settings export/import, and comprehensive startup `SettingsWindow` Personalization Studio (Live Bilingual Switcher, 16 Native Bilingual Color Palettes, Visual Swatches, Real-Time Geometry Sliders (`BarHeight`, `BarWidthPercent`, `IconSize`, `CornerRadius`, `Opacity`), 4 Icon/Box Shapes (`Circle`, `RoundedSquare`, `Square`, `Capsule`), 3-Column Drag-and-Drop Group/App Organizer, Profiles, Power, and 1-Click Microsoft ↔ Dockyra Taskbar Switcher). |
| `MOD-PERFORMANCE` | Event Coalescing & Diagnostics | `in-progress` | `EventCoalescer` throttling rapid OS events, memory/throughput benchmarks in `Dockyra.PerformanceTests`. |
| `MOD-POWER` | Adaptive Power Management | `in-progress` | `Win32PowerMonitor` (`GetSystemPowerStatus` + power event notifications) and `PowerModeCoordinator` (`PowerSaver`, `Balanced`, `Performance`). |

### Partially Completed / Pending Features
- **Interactive Drag-and-Drop (`TASK-0008`)**: Domain reordering (`TaskbarLayoutValidator`, `ApplicationGroup`) is implemented and unit-tested; interactive mouse/touch drag-and-drop visual adorners in WPF are scheduled next.
- **Full 12-Page Settings UI (`TASK-0009`)**: Persistence and JSON backup/restore work end-to-end; the WPF Settings window currently exposes core tabs (General, Taskbar, Appearance, Power, Backup) and will be expanded to all 12 detailed sections in `PHASE-04`.
- **Multi-Monitor Independent Taskbar Instances (`TASK-0010`)**: `Win32MonitorService` enumerates all monitors and work areas accurately; spawning separate `TaskbarWindow` instances per secondary display is planned for `PHASE-05`.

---

## 3. Latest Verified Build & Test Results
- **Solution Build (`dotnet build Dockyra.sln`)**: **SUCCEEDED** (0 errors, 0 warnings).
- **Automated Tests (`dotnet test Dockyra.sln`)**: **PASSED** across all 5 test projects (`UnitTests`, `IntegrationTests`, `UITests`, `ArchitectureTests`, `PerformanceTests`).
- **Planning Validation (`scripts/validate-planning.ps1`)**: **PASSED** (0 broken links, 0 invalid module IDs, 0 duplicate task IDs).

---

## 4. Known Technical Limitations & Risks
1. **Windows 11 Foreground Lock & UIPI**: `SetForegroundWindow` cannot force focus away from elevated (`Administrator`) windows when Dockyra runs as standard user (`asInvoker`). `Win32WindowManager` returns a structured `OperationResult` rather than crashing.
2. **Native System Tray & Notification Center**: Windows 11 does not expose supported public APIs to host third-party replicas of the Action Center or encrypted system tray icons. Dockyra preserves access to native Windows 11 shell shortcuts (`Win+N`, `Win+A`, `Win+X`).

---

## 5. Next Priority Tasks
1. **`TASK-0008` (`MOD-FOLDERS` / `MOD-GROUPS`)**: Implement interactive drag-and-drop reordering and visual drop-target adorners in `Dockyra.Presentation.Wpf`.
2. **`TASK-0009` (`MOD-SETTINGS`)**: Complete all 12 sections of the `SettingsWindow` with live preview binding.
3. **`TASK-0010` (`MOD-TASKBAR`)**: Add multi-monitor window instance manager and per-monitor DPI repositioning hooks.

