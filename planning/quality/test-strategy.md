# Dockyra Test Strategy (`planning/quality/test-strategy.md`)

Dockyra employs a five-tier automated test pyramid executed via `dotnet test Dockyra.sln`:

| Tier | Project | Scope |
|---|---|---|
| **1. Architecture & Governance** | `Dockyra.ArchitectureTests` | Verifies `.csproj` dependency rules, platform type isolation (`IntPtr`, `System.Windows.*`), `module-registry.yaml` paths, and `planning/` markdown links. |
| **2. Unit Tests** | `Dockyra.UnitTests` | Tests `Dockyra.Domain` entities/rules (`SmartGroupingRuleEngine`, `FolderHierarchyRules`, `TaskbarLayoutValidator`), `Dockyra.Application` services, and `Dockyra.Presentation.Wpf` ViewModels using lightweight fakes. |
| **3. Integration Tests** | `Dockyra.IntegrationTests` | Tests `Dockyra.Infrastructure` SQLite schema migrations, repositories, JSON backup/restore, `SafeModeStartupCoordinator`, `ModuleHostService`, and `Dockyra.WindowsIntegration` adapters. |
| **4. UI & Resource Tests** | `Dockyra.UITests` | Tests XAML `DesignTokens`, theme dictionaries, and `tr-TR`/`en-US` localization dictionary parity. |
| **5. Performance Tests** | `Dockyra.PerformanceTests` | Tests `EventCoalescer` batching under rapid event bursts and `SmartGroupingRuleEngine` evaluation throughput. |

