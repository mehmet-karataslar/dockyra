# ADR-0002: Strict Separation of UI (`Presentation.Wpf`) and OS Integration (`WindowsIntegration`)

- **Status**: Accepted
- **Date**: 2026-11-14
- **Affected Modules**: `MOD-CORE`, `MOD-TASKBAR`, `MOD-WINDOWS`, `MOD-POWER`, `MOD-THEMES`

## 1. Context
In many desktop utilities, Win32 P/Invoke calls (`EnumWindows`, `SetForegroundWindow`, `HWND` pointers) are placed directly inside WPF code-behind or ViewModels. This makes unit testing impossible without live windows and locks the application permanently to WPF.

## 2. Decision
1. `Dockyra.Presentation.Wpf` is **strictly forbidden** from referencing `Dockyra.WindowsIntegration` or `Dockyra.Infrastructure`.
2. `Dockyra.WindowsIntegration` is **strictly forbidden** from referencing `Dockyra.Presentation.Wpf`.
3. All OS interactions cross through platform-independent interfaces in `Dockyra.Contracts` (`IWindowManager`, `IMonitorService`, `IPowerMonitor`, `IApplicationLauncher`, `ITaskbarVisibilityService`) using value types (`WindowId`, `WindowInfo`, `MonitorInfo`, `PowerState`).
4. `Dockyra.ArchitectureTests` enforces these rules on every test run.

## 3. Consequences
- **Advantages**: ViewModels and Use Cases can be unit-tested with lightweight fakes in milliseconds; future `Dockyra.Presentation.WinUI` can reuse 100% of `Domain`, `Application`, `Infrastructure`, and `WindowsIntegration`.
- **Trade-offs**: Requires explicit DTO/contract mapping at the boundary (`WindowId` instead of raw `IntPtr`).

