# Dockyra Modules Documentation (`planning/modules/README.md`)

Every functional module in **Dockyra** is cataloged in [`module-registry.yaml`](module-registry.yaml) and documented in its own specification file in this folder.

## Mandatory Rule for Developers & AI Agents
Before adding a new feature or creating a new module:
1. Inspect [`module-registry.yaml`](module-registry.yaml).
2. Determine if the feature belongs to an existing module (`MOD-TASKBAR`, `MOD-GROUPS`, `MOD-FOLDERS`, `MOD-WINDOWS`, `MOD-PROFILES`, `MOD-THEMES`, `MOD-SETTINGS`, `MOD-PERFORMANCE`, `MOD-POWER`, `MOD-CORE`).
3. Do **not** create a duplicate module if an existing module already covers the domain.

## Module Specifications
- [taskbar.md](taskbar.md) — `MOD-TASKBAR`: Taskbar Shell, Sections & Layout
- [groups.md](groups.md) — `MOD-GROUPS`: Application Groups & Smart Classification
- [folders.md](folders.md) — `MOD-FOLDERS`: Expandable Folder Flyouts & Hierarchy
- [windows.md](windows.md) — `MOD-WINDOWS`: Open Window Detection, Focus & Lifecycle
- [profiles.md](profiles.md) — `MOD-PROFILES`: Work Profiles & Context Switching
- [themes.md](themes.md) — `MOD-THEMES`: Fluent Design Tokens, Themes & Localization
- [settings.md](settings.md) — `MOD-SETTINGS`: User Preferences, SQLite Persistence & JSON Backup
- [performance.md](performance.md) — `MOD-PERFORMANCE`: Event Coalescing, Diagnostics & Resource Optimization
- [power-management.md](power-management.md) — `MOD-POWER`: Adaptive Power Modes & Battery Monitoring

