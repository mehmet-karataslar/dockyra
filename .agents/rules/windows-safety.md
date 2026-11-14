---
trigger: always_on
---

# Dockyra Windows 11 Safety & Integration Rules

1. **Non-Destructive Coexistence**:
   - Never terminate `explorer.exe`, inject DLLs into Windows Shell processes, modify system files, or apply irreversible registry edits.
   - Always preserve a safe fallback to the native Windows 11 taskbar, Start menu, and notification area.
2. **Centralized P/Invoke**:
   - Place all native Win32/DWM/Shell declarations strictly inside `src/Dockyra.WindowsIntegration/Native/`.
   - Always check return values and bounds on native calls (`GetWindowText`, `DwmGetWindowAttribute`, `GetMonitorInfo`).
3. **Window Filtering & Focus Respect**:
   - Filter out invisible, tool-window, and DWM-cloaked (`DWMWA_CLOAKED`) background windows when enumerating open user windows.
   - Respect Windows foreground lock restrictions (`SetForegroundWindow`) without unsafe thread-input attachment hacks.
4. **Safe Process Launching**:
   - Validate executable file paths and arguments before calling `Process.Start`. Never run with elevated privileges unless explicitly requested by the user.
5. **Safe Mode Recovery**:
   - Maintain startup crash detection (`SafeModeStartupCoordinator`) so repeated startup failures automatically disable non-essential modules and visual effects.

