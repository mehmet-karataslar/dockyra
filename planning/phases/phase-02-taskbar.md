# PHASE-02: Core Taskbar Shell & Real-Time Window Tracking

- **Phase ID**: `PHASE-02`
- **Status**: `IN_PROGRESS` (Prototype Active)
- **Prerequisites**: `PHASE-01`

## 1. Objective
Build the independent taskbar dock window (`TaskbarWindow`) and connect it through `Dockyra.Application` contracts to `Win32WindowManager` for real-time open window detection, active window indication, and focus/minimize/restore/close actions.

## 2. Affected Modules
- `MOD-TASKBAR`, `MOD-WINDOWS`, `MOD-PERFORMANCE`

## 3. Technical Risks & Mitigations
- **Risk**: Hidden UWP / Windows Shell frames appearing as open user windows.
  - **Mitigation**: Query `DwmGetWindowAttribute` with `DWMWA_CLOAKED` and check `WS_EX_TOOLWINDOW`.
- **Risk**: High CPU overhead from window event bursts.
  - **Mitigation**: Coalesce `SetWinEventHook` callbacks through `EventCoalescer`.

## 4. Acceptance Criteria
- [x] `Win32WindowManager` lists visible top-level windows without leaking `IntPtr` outside `WindowsIntegration`.
- [x] Clicking a window item in `TaskbarViewModel` activates, restores, or minimizes the target window safely.
- [x] Dock edge positioning (`Bottom`, `Top`, `Left`, `Right`) updates layout geometry cleanly.

