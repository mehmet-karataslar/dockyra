# Module Specification: Performance & Event Coalescing (`MOD-PERFORMANCE`)

```yaml
id: MOD-PERFORMANCE
name: Performance & Event Coalescing
status: in-progress
purpose: Provides event coalescing/debouncing and resource monitoring to maintain low CPU, RAM, and GPU overhead.
source_paths:
  - src/Dockyra.Application/Services/EventCoalescer.cs
  - src/Dockyra.Modules/Performance/
dependencies:
  - MOD-CORE
contracts:
  - IEventCoalescer
tests:
  - tests/PerformanceTests/
owners:
  - performance-engineer
related_tasks:
  - TASK-0006
  - TASK-0011
```

## Responsibilities
- Coalesce high-frequency bursts of window or system events into single debounced execution ticks (`EventCoalescer`).
- Provide diagnostic snapshots of process working set and coalesced event counts.
- Verify allocation and throughput targets in `Dockyra.PerformanceTests`.

