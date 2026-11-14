# PHASE-06: CPU/RAM/GPU Optimization, Power Modes & Endurance Testing

- **Phase ID**: `PHASE-06`
- **Status**: `BACKLOG`
- **Prerequisites**: `PHASE-05`

## 1. Objective
Verify and tune resource consumption across `PowerSaver`, `Balanced`, and `Performance` modes so Dockyra stays within its `< 0.5%` idle CPU and `< 150 MB` RAM budgets during extended sessions.

## 2. Affected Modules
- `MOD-PERFORMANCE`, `MOD-POWER`

## 3. Acceptance Criteria
- [x] `PowerModeCoordinator` and `Win32PowerMonitor` transition between `PowerSaver`, `Balanced`, and `Performance` modes.
- [x] `EventCoalescer` batches high-frequency events in `Dockyra.PerformanceTests`.
- [ ] Long-running memory leak endurance suite and hardware benchmark report (`TASK-0011`).

