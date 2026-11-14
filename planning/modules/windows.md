# Module Specification: Open Window Tracking & Management (`MOD-WINDOWS`)

```yaml
id: MOD-WINDOWS
name: Open Window Tracking & Management
status: in-progress
purpose: Tracks top-level desktop windows via Win32/DWM APIs and manages focus, minimize, restore, and close operations.
source_paths:
  - src/Dockyra.Application/UseCases/WindowSwitchUseCase.cs
  - src/Dockyra.WindowsIntegration/WindowTracking/Win32WindowManager.cs
  - src/Dockyra.Modules/Windows/
dependencies:
  - MOD-CORE
  - MOD-PERFORMANCE
contracts:
  - IWindowManager
  - IWindowSwitchUseCase
tests:
  - tests/UnitTests/
  - tests/IntegrationTests/
owners:
  - windows-specialist
  - backend-developer
related_tasks:
  - TASK-0006
  - TASK-0007
```

## Responsibilities
- Enumerate visible user windows using `EnumWindows`, `IsWindowVisible`, `GetWindowTextW`, and `DwmGetWindowAttribute(DWMWA_CLOAKED)`.
- Subscribe to OS window lifecycle and focus events via `SetWinEventHook` paired with `EventCoalescer`.
- Abstract native window handles behind `WindowId(ulong Value)`.
- Execute `ActivateWindowAsync`, `MinimizeWindowAsync`, `RestoreWindowAsync`, and `CloseWindowAsync` while respecting Windows foreground lock rules.

