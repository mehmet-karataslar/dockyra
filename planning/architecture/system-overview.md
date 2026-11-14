# System Overview (`planning/architecture/system-overview.md`)

## 1. Architectural Pattern
Dockyra combines **Clean Architecture** with a **Modular Monolith** and **MVVM** presentation layer on `.NET 10`.

```mermaid
flowchart TD
    Desktop["Dockyra.Desktop (Composition Root & Bootstrapper)"]
    Wpf["Dockyra.Presentation.Wpf (Views, ViewModels, Themes, Localization)"]
    Modules["Dockyra.Modules (Feature Module Registrations & Fault Boundary)"]
    WinInt["Dockyra.WindowsIntegration (Win32, DWM, Monitors, Power, Launcher)"]
    Infra["Dockyra.Infrastructure (SQLite, Migrations, JSON Backup, Serilog)"]
    App["Dockyra.Application (Use Cases, Orchestration Services, Interfaces)"]
    Contracts["Dockyra.Contracts (Stable DTOs, Results & OS Abstraction Contracts)"]
    Domain["Dockyra.Domain (Pure Entities, Value Objects, Business Rules)"]

    Desktop --> Wpf
    Desktop --> Modules
    Desktop --> WinInt
    Desktop --> Infra
    Desktop --> App
    Desktop --> Contracts
    Desktop --> Domain

    Wpf --> App
    Wpf --> Domain
    Wpf --> Contracts

    Modules --> App
    Modules --> Domain
    Modules --> Contracts

    WinInt --> App
    WinInt --> Contracts

    Infra --> App
    Infra --> Domain
    Infra --> Contracts

    App --> Domain
    App --> Contracts
```

## 2. Layer Responsibilities

| Project | Target Framework | Responsibility |
|---|---|---|
| `Dockyra.Domain` | `net10.0` | Pure domain entities (`TaskbarLayout`, `ApplicationGroup`, `FolderItem`, `WorkProfile`), value objects, and deterministic rules (`SmartGroupingRuleEngine`, `FolderHierarchyRules`, `TaskbarLayoutValidator`). Zero dependencies. |
| `Dockyra.Contracts` | `net10.0` | Cross-layer and cross-module interfaces (`IWindowManager`, `IMonitorService`, `IPowerMonitor`, `IApplicationLauncher`, `IModule`), `WindowId`, and `OperationResult`. |
| `Dockyra.Application` | `net10.0` | Application use cases (`TaskbarOrchestrationService`, `ApplicationGroupService`, `WindowSwitchUseCase`, `PowerModeCoordinator`, `SafeModeStartupCoordinator`) and repository interfaces. |
| `Dockyra.Infrastructure` | `net10.0` | SQLite persistence (`DatabaseMigrator`, repositories), schema-versioned JSON settings backup/restore, `FileSafeModeStateStore`, and Serilog logging. |
| `Dockyra.WindowsIntegration` | `net10.0-windows` | Win32, DWM, Shell, monitor, power, and process launch implementations (`Win32WindowManager`, `Win32MonitorService`, `Win32PowerMonitor`, `Win32ApplicationLauncher`). |
| `Dockyra.Modules` | `net10.0` | Self-contained module descriptors (`TaskbarModule`, `GroupsModule`, `FoldersModule`, etc.) and fault-isolated `ModuleHostService`. |
| `Dockyra.Presentation.Wpf` | `net10.0-windows` | WPF Views, MVVM ViewModels (`CommunityToolkit.Mvvm`), Fluent `DesignTokens`, themes, and `tr-TR`/`en-US` localization. |
| `Dockyra.Desktop` | `net10.0-windows` | Application entry point (`App.xaml.cs`), single-instance guard, DI container setup (`ServiceRegistration.cs`), and global exception recovery. |

