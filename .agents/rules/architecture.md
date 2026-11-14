---
trigger: always_on
---

# Dockyra Architectural Rules

1. **UI and OS Integration Separation**:
   - `Dockyra.Presentation.Wpf` must NEVER reference `Dockyra.WindowsIntegration` or `Dockyra.Infrastructure`.
   - `Dockyra.WindowsIntegration` must NEVER reference `Dockyra.Presentation.Wpf`.
   - WPF Views and ViewModels must interact with Windows features exclusively through interfaces in `Dockyra.Application` or `Dockyra.Contracts` (`IWindowManager`, `IMonitorService`, `IPowerMonitor`, `IApplicationLauncher`, `ITaskbarVisibilityService`).
2. **Domain Purity**:
   - `Dockyra.Domain` must have zero project references and zero platform-specific NuGet dependencies (no WPF, no Win32, no SQLite).
3. **Platform-Agnostic Contracts**:
   - Never expose `IntPtr`, `HWND`, P/Invoke structs, or `System.Windows.*` types in `Dockyra.Contracts`, `Dockyra.Domain`, or `Dockyra.Application`. Use `WindowId` and domain/contract DTOs.
4. **Module Isolation**:
   - Before creating any new module, inspect `planning/modules/module-registry.yaml`.
   - Modules must not reference internal classes of other modules; share only through `Dockyra.Contracts` or `Dockyra.Application`.
5. **Composition Root**:
   - Only `Dockyra.Desktop` may wire concrete `Infrastructure`, `WindowsIntegration`, `Modules`, and `Presentation.Wpf` implementations together via Dependency Injection.

