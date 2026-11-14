# Dependency Rules (`planning/architecture/dependency-rules.md`)

## 1. Allowed vs. Forbidden Project References

| Source Project | Allowed Project References | Strictly Forbidden References |
|---|---|---|
| `Dockyra.Domain` | *None* | All projects, WPF, Win32, SQLite |
| `Dockyra.Contracts` | *None* | All projects, WPF, Win32, SQLite |
| `Dockyra.Application` | `Domain`, `Contracts` | `Infrastructure`, `WindowsIntegration`, `Presentation.Wpf`, `Modules`, `Desktop` |
| `Dockyra.Infrastructure` | `Application`, `Domain`, `Contracts` | `WindowsIntegration`, `Presentation.Wpf`, `Desktop` |
| `Dockyra.WindowsIntegration` | `Application`, `Contracts` | `Presentation.Wpf`, `Infrastructure`, `Desktop` |
| `Dockyra.Modules` | `Application`, `Domain`, `Contracts` | `Presentation.Wpf`, `WindowsIntegration`, `Infrastructure`, `Desktop` |
| `Dockyra.Presentation.Wpf` | `Application`, `Domain`, `Contracts` | **`WindowsIntegration`**, **`Infrastructure`**, `Desktop` |
| `Dockyra.Desktop` | All projects (Composition Root) | *None* |

## 2. Type Isolation Constraints

1. **No Native Handles Outside `WindowsIntegration`**:
   - `IntPtr`, `nint`, `HWND`, `HMONITOR`, `HWINEVENTHOOK`, and `[DllImport]` / `[LibraryImport]` attributes are restricted to `Dockyra.WindowsIntegration`.
   - Window identity across `Contracts`, `Application`, and `Presentation.Wpf` is represented by the value object `WindowId(ulong Value)`.
2. **No WPF Types in Core Layers**:
   - `System.Windows.*` namespaces (`Window`, `UIElement`, `Dispatcher`, `Brush`, `ImageSource`) must never appear in `Domain`, `Contracts`, `Application`, `Infrastructure`, or `WindowsIntegration`.
   - Colors and icons in `Domain` are represented via platform-agnostic `HexColor` and `IconReference` value objects.

## 3. Automated Enforcement
Every build and test run validates these constraints via `tests/ArchitectureTests/Dockyra.ArchitectureTests.csproj`:
- `LayerDependencyTests.Domain_ShouldNotReferenceAnyProjectOrPlatformPackage`
- `LayerDependencyTests.PresentationWpf_ShouldNeverReferenceWindowsIntegrationOrInfrastructure`
- `LayerDependencyTests.WindowsIntegration_ShouldNeverReferencePresentationWpf`
- `LayerDependencyTests.CoreLayers_ShouldNotExposeIntPtrOrPInvoke`

