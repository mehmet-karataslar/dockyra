# Workflow: Testing & Verification (`planning/workflows/testing.md`)

1. **Unit Tests (`tests/UnitTests/`)**: Test all `Dockyra.Domain` rules (`SmartGroupingRuleEngine`, `FolderHierarchyRules`, `TaskbarLayoutValidator`), `Dockyra.Application` services, and `Dockyra.Presentation.Wpf` ViewModels in isolation.
2. **Integration Tests (`tests/IntegrationTests/`)**: Test `Dockyra.Infrastructure` SQLite migrations, repositories, JSON backup/restore, `ModuleHostService` fault boundaries, and `Dockyra.WindowsIntegration` adapters.
3. **UI Tests (`tests/UITests/`)**: Verify XAML theme dictionaries, design tokens, and `tr-TR`/`en-US` localization completeness.
4. **Architecture Tests (`tests/ArchitectureTests/`)**: Enforce project references, type isolation, and `planning/` integrity.
5. **Performance Tests (`tests/PerformanceTests/`)**: Verify `EventCoalescer` batching and rule evaluation latency.

