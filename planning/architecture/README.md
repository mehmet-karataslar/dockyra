# Dockyra Architecture Documentation Index

This folder contains the authoritative architectural blueprints and constraints for **Dockyra**.

## Documents

1. [system-overview.md](system-overview.md) — High-level system architecture, Clean Architecture layers, and Composition Root responsibilities.
2. [dependency-rules.md](dependency-rules.md) — Strict compile-time dependency rules and automated verification via `Dockyra.ArchitectureTests`.
3. [module-boundaries.md](module-boundaries.md) — Modular decomposition (`IModule`), fault isolation (`ModuleHostService`), and cross-module communication rules.
4. [data-flow.md](data-flow.md) — Request/response and state persistence flows across UI, Application, Domain, and SQLite.
5. [event-flow.md](event-flow.md) — Event-driven OS notifications (`SetWinEventHook`, `EventCoalescer`, power state changes) to ViewModels.
6. [ui-portability.md](ui-portability.md) — Architectural strategy and honest adaptation boundaries for future `WPF` -> `WinUI 3` migration.
7. [windows-integration.md](windows-integration.md) — Evaluation of Windows 11 taskbar integration strategies, supported Win32/DWM APIs, and safe coexistence.
8. [database-design.md](database-design.md) — SQLite schema design, migration versioning, and JSON export/import format.
9. [security.md](security.md) — Least-privilege execution, path/process validation, SQL parameterization, log privacy, and Safe Mode recovery.

