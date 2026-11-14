# ADR-0003: Safe Independent Taskbar Coexistence Over Shell Injection

- **Status**: Accepted
- **Date**: 2026-11-14
- **Affected Modules**: `MOD-TASKBAR`, `MOD-WINDOWS`, `MOD-CORE`

## 1. Context
Windows 11 redesigned `explorer.exe` and the taskbar XAML islands. Replacing the native taskbar via DLL injection, memory patching, or terminating `explorer.exe` causes system instability, breaks on Windows Update, triggers security software, and can strand users without a working desktop shell.

## 2. Decision
Dockyra operates as an **independent, standard-user (`asInvoker`), reversible taskbar dock**:
- Never terminates `explorer.exe`, never injects DLLs, and never modifies protected system files.
- Uses supported Win32 and DWM APIs (`EnumWindows`, `DwmGetWindowAttribute`, `SetWinEventHook`, `SetForegroundWindow`, `ShowWindow`).
- Preserves access to native Windows 11 shell components (Start menu, System Tray, Notification Center).
- Includes `SafeModeStartupCoordinator` so repeated startup crashes automatically disable optional modules and allow one-click recovery.

