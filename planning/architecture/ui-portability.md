# UI Portability: WPF to WinUI 3 Strategy (`planning/architecture/ui-portability.md`)

## 1. Architectural Goal
When a future `Dockyra.Presentation.WinUI` project is introduced, **100% of `Dockyra.Domain`, `Dockyra.Contracts`, `Dockyra.Application`, `Dockyra.Infrastructure` (SQLite), `Dockyra.WindowsIntegration`, and `Dockyra.Modules` must be reusable without modification**.

## 2. What Is 100% Portable Today
- **Domain Entities & Rules (`Dockyra.Domain`)**: `TaskbarLayout`, `ApplicationGroup`, `FolderItem`, `WorkProfile`, `SmartGroupingRuleEngine`, `FolderHierarchyRules`, `TaskbarLayoutValidator`.
- **Application Use Cases (`Dockyra.Application`)**: `TaskbarOrchestrationService`, `ApplicationGroupService`, `WindowSwitchUseCase`, `PowerModeCoordinator`, `SafeModeStartupCoordinator`.
- **SQLite Persistence & JSON Backup (`Dockyra.Infrastructure`)**: All database migrations, repositories, and JSON export/import services.
- **Windows 11 OS Adapters (`Dockyra.WindowsIntegration`)**: `Win32WindowManager`, `Win32MonitorService`, `Win32PowerMonitor`, `Win32ApplicationLauncher`.
- **ViewModel Patterns**: ViewModels use `CommunityToolkit.Mvvm` (`ObservableObject`, `RelayCommand`), which works identically in WPF and WinUI 3.

## 3. Realistic Adaptation Boundaries (What Will Change in WinUI 3)
We do **not** assume a zero-effort UI migration. Moving from `Dockyra.Presentation.Wpf` to `Dockyra.Presentation.WinUI` will require adapting:
1. **XAML Namespace & Control Differences**:
   - WPF `Window`, `Popup`, `ItemsControl`, and `ResourceDictionary` syntax differs from WinUI 3 (`Microsoft.UI.Xaml.*`, `AppWindow`, `Flyout`).
2. **Window Chrome & Backdrop APIs**:
   - WPF uses `WindowStyle="None"`, `AllowsTransparency`, or `WindowChrome`; WinUI 3 uses `AppWindowPresenter`, `DesktopAcrylicController`, and `MicaController`.
3. **Dispatcher Abstraction**:
   - WPF uses `System.Windows.Threading.Dispatcher`; WinUI 3 uses `Microsoft.UI.Dispatching.DispatcherQueue`. ViewModels avoid direct `Dispatcher.CurrentDispatcher` dependencies so UI marshaling stays thin.
4. **Design Tokens**:
   - Token values (hex colors, spacing scales, corner radii) are defined canonically and mapped into XAML `ResourceDictionary` files per UI framework.

