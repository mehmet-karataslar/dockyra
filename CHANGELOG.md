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
- **WPF Fluent Taskbar Shell & Dual Native Taskbar Integration**: Created the working taskbar dock window (`TaskbarWindow`) supporting both **Native Overlay Mode** (docks directly inside the existing Windows 11 `Shell_TrayWnd` taskbar) and **Full Replacement Mode** (reversibly hides `Shell_TrayWnd`, docks edge-to-edge across the bottom of the screen, and restores the native taskbar on mode switch or exit), plus automatic synchronization of the user's real Windows 11 pinned taskbar shortcuts (`%AppData%\...\User Pinned\TaskBar\*.lnk`), Windows Start Menu trigger (`⊞ Başlat`), live open-window items, application group pills, folder flyout panel, power mode selector, Fluent DesignTokens, Dark/Light themes, and Turkish/English localization dictionaries.
- **Planning Memory System (`planning/`)**: Established complete documentation covering overview, live state, roadmap, 9 module specs + `module-registry.yaml`, 7 phase specs, ADRs, workflows, quality budgets, and handoff records.
- **Startup Personalization Studio & Drag-and-Drop Organizer (`SettingsWindow`)**: Configured `App.xaml.cs` to launch the full-featured Dockyra Personalization Studio first, featuring live `🇹🇷 Türkçe` / `🇬🇧 English` language switching, 16 soft matte color palette presets + visual color swatches (zero raw hex codes), instant real-time taskbar geometry sliders (`BarHeight`, `BarWidthPercent`, `IconSize`, `CornerRadius`, `Opacity`), 4 selectable icon/container shapes (`Circle`, `RoundedSquare`, `Square`, `Capsule`), 3-column Drag-and-Drop group/folder and application organizer, file browser (`OpenFileDialog`), fullscreen video auto-hide detection, and 1-click **"Görev Çubuğunu Değiştir (Dockyra ↔ Microsoft)"** taskbar switching.
- **Automated Governance Verification**: Added `Dockyra.ArchitectureTests` and `scripts/validate-planning.ps1` to verify layer dependencies, module registry integrity, task IDs, and markdown links.

