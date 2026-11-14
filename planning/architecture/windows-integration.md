# Windows 11 Integration Strategy (`planning/architecture/windows-integration.md`)

## 1. Comparison of Taskbar Integration Approaches

| Approach | Compatibility & Update Safety | Admin Required? | Reversibility | Verdict |
|---|---|---|---|---|
| **1. Independent Dock Panel Coexisting with Windows 11** | High (Uses documented Win32/DWM APIs) | No (`asInvoker`) | Instant (Close app or switch setting) | **Selected Primary Mode** |
| **2. Independent Taskbar + User Opt-In Auto-Hide of Native Taskbar** | Medium-High (`SHAppBarMessage` / user setting) | No | High (Automatic restore on exit/crash) | **Supported Optional Mode** |
| **3. Deep Shell Injection / Killing `explorer.exe`** | Fragile (Breaks on Windows 11 cumulative updates; triggers AV/EDR) | Often Yes | Poor (Can leave desktop without shell) | **Strictly Prohibited** |

## 2. Supported Win32 & DWM APIs Used in `Dockyra.WindowsIntegration`
- **Window Enumeration & Filtering**:
  - `EnumWindows`, `IsWindowVisible`, `GetWindowTextLengthW`, `GetWindowTextW`, `GetWindowLongPtr` (`GWL_EXSTYLE` for `WS_EX_TOOLWINDOW` / `WS_EX_APPWINDOW`), `GetWindowThreadProcessId`.
  - `DwmGetWindowAttribute` with `DWMWA_CLOAKED` (`14`) to exclude background UWP apps and hidden Windows 11 shell frames (`TextInputHost`, `ShellExperienceHost`).
- **Event-Driven Window Tracking**:
  - `SetWinEventHook` (`WINEVENT_OUTOFCONTEXT | WINEVENT_SKIPOWNPROCESS`) monitoring `EVENT_SYSTEM_FOREGROUND`, `EVENT_OBJECT_CREATE`, `EVENT_OBJECT_DESTROY`, `EVENT_OBJECT_SHOW`, `EVENT_OBJECT_HIDE`, and `EVENT_OBJECT_NAMECHANGE` with `EventCoalescer`.
- **Window State & Focus**:
  - `GetForegroundWindow`, `SetForegroundWindow`, `IsIconic`, `ShowWindow` (`SW_RESTORE`, `SW_MINIMIZE`), `PostMessage` (`WM_CLOSE`).
- **Multi-Monitor & Work Areas**:
  - `EnumDisplayMonitors`, `GetMonitorInfoW` (`MONITORINFOEXW`).
- **Power & Battery**:
  - `GetSystemPowerStatus` (`SYSTEM_POWER_STATUS` for AC line status, battery percentage, and system energy saver flag).

## 3. Preserving Native Windows 11 Shell Access
Because Windows 11 does not provide public APIs to replicate encrypted notification center internals or third-party system tray hosting without shell hooking, Dockyra preserves seamless access to native Windows 11 features (`Start`, `Action Center`, `Quick Settings`, `System Tray`).

