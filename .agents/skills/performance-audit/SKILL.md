---
name: performance-audit
description: >-
  Use this skill to evaluate and optimize Dockyra CPU, RAM, GPU, and battery usage,
  event coalescing, and memory leak resilience.
---

# Dockyra Performance Audit Skill

## When to Use
Use when auditing resource usage, optimizing window/power event pipelines, or validating `PHASE-06` performance targets.

## Documents to Read
1. `planning/quality/performance-budget.md`
2. `planning/modules/performance.md`
3. `planning/modules/power-management.md`

## Step-by-Step Procedure
1. **Audit Event Loops & Timers**:
   - Inspect `Dockyra.WindowsIntegration` and `Dockyra.Presentation.Wpf` for any unbounded timers or polling loops.
   - Verify `EventCoalescer` debouncing intervals for window and power state events.
2. **Audit Power Mode Adaptation**:
   - Confirm `PowerModeCoordinator` adjusts animation and refresh behavior across `PowerSaver`, `Balanced`, and `Performance` modes.
3. **Audit Memory & Disposables**:
   - Check that all `IDisposable` implementations unhook Win32 callbacks and event subscriptions.
4. **Run Automated Performance Suite**:
   - Execute `dotnet test tests/PerformanceTests/Dockyra.PerformanceTests.csproj`.

## Expected Output
Measured test metrics, identified bottlenecks, applied optimizations, and before/after comparison.

