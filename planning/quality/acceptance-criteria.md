# Dockyra Acceptance Criteria (`planning/quality/acceptance-criteria.md`)

Every task (`TASK-XXXX`) and feature (`FEATURE-XXXX`) in Dockyra must satisfy the following baseline quality gates before being marked `DONE`:

1. **End-to-End Completeness**:
   - Domain model & invariants implemented.
   - Application orchestration service / use case wired.
   - SQLite persistence or Win32 adapter implemented (if applicable).
   - WPF ViewModel & View wired with `DesignTokens` and `tr-TR`/`en-US` localization keys.
   - Explicit error handling (`OperationResult` + `ILogger`).
   - Automated tests added and passing.
2. **Zero Architectural Violations**:
   - `Dockyra.ArchitectureTests` passes with zero errors.
3. **Documentation Synchronization**:
   - `planning/CURRENT-STATE.md`, `planning/tasks/`, and `planning/modules/module-registry.yaml` updated and verified by `scripts/validate-planning.ps1`.

