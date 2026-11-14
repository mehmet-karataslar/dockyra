---
trigger: always_on
---

# Dockyra Performance & Power Budget Rules

1. **Event-Driven Architecture**:
   - Never introduce high-frequency polling timers to scan all processes or windows on the UI thread.
   - Use event notifications (`SetWinEventHook` / `EventCoalescer`) with debouncing/throttling to batch bursts of window events.
2. **Power Mode Awareness**:
   - Respect the active `PowerMode` (`PowerSaver`, `Balanced`, `Performance`) managed by `IPowerManagementService`.
   - In `PowerSaver` mode, disable non-essential animations, reduce background refresh frequency, and generate previews only on demand.
3. **Memory Discipline**:
   - Avoid unnecessary allocations in hot event paths.
   - Unsubscribe event handlers on disposal to prevent long-running memory leaks.
4. **Measured Claims Only**:
   - Do not claim performance optimizations without verification via `tests/PerformanceTests/` or documented measurements.

