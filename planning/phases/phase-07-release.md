# PHASE-07: Safe Mode Hardening, Packaging/Installer & v1.0 Release

- **Phase ID**: `PHASE-07`
- **Status**: `BACKLOG`
- **Prerequisites**: `PHASE-06`

## 1. Objective
Finalize crash recovery hardening, build the Windows installer/uninstaller with optional user data cleanup, and prepare the v1.0 Release Candidate.

## 2. Affected Modules
- `MOD-CORE`, `MOD-SETTINGS`

## 3. Acceptance Criteria
- [x] `SafeModeStartupCoordinator` detects consecutive startup failures and activates Safe Mode after 3 crashes.
- [x] Schema-versioned JSON settings export/import works end-to-end.
- [ ] Installer package and clean uninstaller verification (`TASK-0012`).

