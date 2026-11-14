# Module Boundaries (`planning/architecture/module-boundaries.md`)

## 1. Module Contract (`IModule`)
Each functional unit in Dockyra is registered as an `IModule` defined in `Dockyra.Contracts`:

```csharp
public interface IModule
{
    string Id { get; }
    string Name { get; }
    bool IsEssential { get; }
    Task<OperationResult> InitializeAsync(CancellationToken cancellationToken = default);
    Task ShutdownAsync(CancellationToken cancellationToken = default);
}
```

## 2. Fault Isolation & Safe Mode (`ModuleHostService`)
- `ModuleHostService` initializes registered modules in dependency order.
- If a non-essential module fails during `InitializeAsync`, `ModuleHostService` catches the exception, logs a structured `ModuleError`, marks that module as degraded/disabled, and continues starting the core taskbar (`MOD-TASKBAR`, `MOD-WINDOWS`).
- When `SafeModeStartupCoordinator` detects consecutive startup crashes (`>= 3` ungraceful startups), non-essential modules and heavy visual effects are skipped automatically so the user can always access settings and restore defaults.

## 3. Cross-Module Communication Rules
1. Modules **never** reference internal implementation classes of another module.
2. Shared operations are exposed through service interfaces in `Dockyra.Application` or `Dockyra.Contracts`.
3. Before creating a new module, agents must inspect `planning/modules/module-registry.yaml`.

