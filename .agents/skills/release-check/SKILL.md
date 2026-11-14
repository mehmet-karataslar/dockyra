---
name: release-check
description: >-
  Use this skill before cutting a new Dockyra version or milestone to verify build,
  tests, safe mode recovery, documentation sync, and changelog completeness.
---

# Dockyra Release Check Skill

## When to Use
Use prior to tagging a release candidate or completing a development phase.

## Documents to Read
1. `planning/workflows/release.md`
2. `planning/quality/acceptance-criteria.md`
3. `planning/CURRENT-STATE.md`
4. `CHANGELOG.md`

## Step-by-Step Procedure
1. **Clean Build & Full Test Suite**:
   - Run `dotnet build Dockyra.sln -c Release`
   - Run `dotnet test Dockyra.sln -c Release`
2. **Validate Planning & Registry Integrity**:
   - Run `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/validate-planning.ps1`
3. **Verify Safety & Recovery Mechanisms**:
   - Confirm `SafeModeStartupCoordinator` tests pass.
   - Confirm settings JSON export/import and SQLite migration tests pass.
4. **Synchronize Release Documentation**:
   - Update `CHANGELOG.md`, `planning/CHANGELOG.md`, `planning/CURRENT-STATE.md`, and `planning/releases/README.md`.

## Expected Output
Release readiness report with build/test summary, planning validator output, and known limitations list.

