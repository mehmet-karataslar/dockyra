# Dockyra Technical Research Notes (`planning/research/README.md`)

## Key Findings Applied to Dockyra Architecture

1. **Windows 11 Taskbar Coexistence vs. Shell Hooking**:
   - Windows 11 `22H2`–`24H2+` hosts the taskbar inside `Explorer.exe` using XAML Islands. Killing `explorer.exe` or injecting native DLLs causes frequent crashes after monthly cumulative updates.
   - An independent dock bar using `EnumWindows` + `SetWinEventHook` + `DwmGetWindowAttribute` provides reliable window management with zero OS corruption risk.
2. **DWM Cloaked Window Filtering**:
   - Calling `IsWindowVisible` alone returns `true` for suspended UWP apps and hidden system frames (`TextInputHost.exe`). Checking `DWMWA_CLOAKED` (`14`) via `dwmapi.dll` accurately filters out invisible background frames.
3. **SQLite on .NET 10**:
   - `Microsoft.Data.Sqlite` provides fast, zero-configuration local storage with `WAL` (Write-Ahead Logging) mode and minimal memory overhead compared to heavy ORM layers.

