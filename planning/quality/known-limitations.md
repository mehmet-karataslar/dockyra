# Known Technical Limitations (`planning/quality/known-limitations.md`)

1. **Windows 11 Foreground Lock & UIPI**:
   - Windows restricts standard-user (`asInvoker`) applications from stealing foreground focus under certain conditions or interacting with elevated (`Administrator`) windows due to User Interface Privilege Isolation (UIPI).
   - Dockyra respects these OS boundaries and returns a structured `OperationResult` without unsafe thread-attachment workarounds.
2. **Closed-Source Windows 11 Notification Center & System Tray Internals**:
   - Windows 11 does not publish public APIs for hosting third-party Action Center / Quick Settings clones.
   - Dockyra preserves access to the native Windows 11 notification area and shell shortcuts (`Win+N`, `Win+A`).
3. **Packaged UWP / WinUI Application Frame Cloaking**:
   - Suspended UWP windows remain registered as top-level HWNDs in Windows; Dockyra filters them via `DwmGetWindowAttribute(DWMWA_CLOAKED)`.

