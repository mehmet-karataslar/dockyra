# Dockyra Performance & Power Budget (`planning/quality/performance-budget.md`)

Dockyra operates as an always-on desktop companion and enforces strict resource budgets:

## 1. Resource Targets
- **Idle CPU Usage**: `< 0.5%` average on standard Windows 11 x64/ARM64 hardware.
- **Working Set (RAM)**: `< 150 MB` during normal daily usage.
- **GPU Overhead**: Zero continuous background animations when idle or in `PowerSaver` mode.
- **Event Coalescing**: Window event bursts (`SetWinEventHook`) coalesced within `80ms` (`Balanced`/`Performance`) or `200ms` (`PowerSaver`).

## 2. Adaptive Power Modes
- **`PowerSaver`**: Disables non-essential animations, reduces background refresh cadence (`200ms` coalesce window), and generates previews only on demand.
- **`Balanced`**: Enables standard Fluent transitions and `80ms` event coalesce window.
- **`Performance`**: Enables rich visual transitions and responsive `50ms` event coalesce window with zero busy-wait loops.

