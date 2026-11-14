# Dockyra Releases (`planning/releases/README.md`)

## Release History

### `v0.1.0` — Initial Modular Architecture & Taskbar Prototype (2026-11-14)
- Clean Architecture `.NET 10` solution (`Dockyra.sln`).
- Strict UI (`Presentation.Wpf`) and OS (`WindowsIntegration`) decoupling.
- Local SQLite persistence (`DatabaseMigrator` + repositories) and JSON export/import.
- Win32/DWM window detection (`Win32WindowManager`) + `EventCoalescer` + `Win32PowerMonitor`.
- Working WPF `TaskbarWindow` prototype with application groups, folder flyout search, work profiles, power mode selector, Fluent Dark/Light themes, and `tr-TR`/`en-US` localization.
- Complete `planning/` memory system and `.agents/` AI agent infrastructure.

