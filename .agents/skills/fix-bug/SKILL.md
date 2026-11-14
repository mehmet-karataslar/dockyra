---
name: fix-bug
description: >-
  Use this skill when diagnosing, reproducing, fixing, and regression-testing a bug
  or unexpected behavior in Dockyra.
---

# Dockyra Bug Fixing Skill

## When to Use
Use when a build error, test failure, runtime exception, Win32 edge case, or UI defect is reported.

## Documents to Read
1. `planning/CURRENT-STATE.md`
2. `planning/workflows/bug-fixing.md`
3. `planning/modules/module-registry.yaml` (to locate affected module and tests)

## Step-by-Step Procedure
1. **Reproduce & Isolate**:
   - Trace the data and event flow across layers (`Presentation.Wpf` -> `Application` -> `Domain` / `WindowsIntegration` / `Infrastructure`).
   - Write a failing unit or integration test that reproduces the defect when possible.
2. **Determine Root Cause**:
   - Verify whether the issue stems from domain rule logic, null/boundary handling, Win32 return codes (e.g., `SetForegroundWindow` focus lock or cloaked window), or WPF dispatcher synchronization.
3. **Apply Minimal Safe Fix**:
   - Fix the root cause without violating layer dependencies or swallowing exceptions.
4. **Verify & Regression Test**:
   - Run `dotnet build Dockyra.sln` and `dotnet test Dockyra.sln`.
   - Confirm the new regression test passes and zero existing tests regress.
5. **Update Records**:
   - Update `planning/CURRENT-STATE.md`, `planning/tasks/`, and `CHANGELOG.md`.

## Expected Output
Root cause explanation, list of modified files, regression test results, and updated state summary.

