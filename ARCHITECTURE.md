# Dockyra Architecture Summary

> **Single Source of Truth**: Full architectural documentation lives in [`planning/architecture/`](planning/architecture/README.md). This root document provides a quick structural overview and links directly to the canonical specifications.

## Layering & Dependency Rule

Dockyra strictly enforces **Clean Architecture** and **UI / OS Separation**:

```text
                  Dockyra.Domain (Zero Dependencies)
                                ↑
                       Dockyra.Application
                     (Depends on Domain + Contracts)
                                ↑
         ┌──────────────────────┼──────────────────────┐
         │                      │                      │
Dockyra.Infrastructure  Dockyra.WindowsIntegration  Dockyra.Presentation.Wpf
  (SQLite, Logging,       (Win32, DWM, Monitors,      (Views, ViewModels,
   JSON Backup)            Power, Process Launch)      DesignTokens, Themes)
         │                      │                      │
         └──────────────────────┼──────────────────────┘
                                │
                         Dockyra.Desktop
                   (Composition Root & DI Host)
```

### Hard Constraints (Enforced by `Dockyra.ArchitectureTests`)
1. `Dockyra.Domain` has **no** project or platform package references.
2. `Dockyra.Application` references **only** `Dockyra.Domain` and `Dockyra.Contracts`.
3. `Dockyra.Presentation.Wpf` **never** references `Dockyra.WindowsIntegration` or `Dockyra.Infrastructure`.
4. `Dockyra.WindowsIntegration` **never** references `Dockyra.Presentation.Wpf`.
5. Native handles (`HWND`, `IntPtr`) and P/Invoke structs never leak into `Domain`, `Application`, `Contracts`, or `Presentation.Wpf`.

## Detailed Architecture Documents
- [Architecture Index](planning/architecture/README.md)
- [System Overview](planning/architecture/system-overview.md)
- [Dependency Rules](planning/architecture/dependency-rules.md)
- [Module Boundaries](planning/architecture/module-boundaries.md)
- [Data Flow](planning/architecture/data-flow.md)
- [Event Flow](planning/architecture/event-flow.md)
- [UI Portability (WPF to WinUI 3)](planning/architecture/ui-portability.md)
- [Windows 11 Integration Strategy](planning/architecture/windows-integration.md)
- [SQLite Database Design](planning/architecture/database-design.md)
- [Security & Recovery Model](planning/architecture/security.md)
- [ADR-0001: Clean Architecture & Modular Monolith](planning/decisions/ADR-0001-architecture.md)
- [ADR-0002: UI and WindowsIntegration Separation](planning/decisions/ADR-0002-ui-separation.md)

