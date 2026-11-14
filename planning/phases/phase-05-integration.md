# PHASE-05: Multi-Monitor, DPI, Auto-Hide, Profiles & Startup Lifecycle

- **Phase ID**: `PHASE-05`
- **Status**: `BACKLOG`
- **Prerequisites**: `PHASE-04`

## 1. Objective
Extend Dockyra across multi-monitor setups with per-monitor DPI awareness, fullscreen game/media auto-hide detection, work profile automation, and safe Windows startup registration.

## 2. Affected Modules
- `MOD-TASKBAR`, `MOD-PROFILES`, `MOD-WINDOWS`

## 3. Acceptance Criteria
- [x] `Win32MonitorService` enumerates physical displays and work areas.
- [x] `WorkProfile` switching updates taskbar layout and groups dynamically.
- [ ] Independent `TaskbarWindow` instances per secondary monitor (`TASK-0010`).
- [ ] Fullscreen app detection and smooth auto-hide docking.

