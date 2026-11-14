# Dockyra Task Registry Summary

> **Single Source of Truth**: Detailed task tracking is maintained in [`planning/tasks/`](planning/tasks/README.md) (`completed.md`, `in-progress.md`, `backlog.md`, `blocked.md`).

## Active & Completed Tasks Snapshot

| Task ID | Title | Module | Phase | Priority | Status | Verification |
|---|---|---|---|---|---|---|
| `TASK-0001` | Workspace Discovery, Clean Architecture Solution & Layer Setup | `MOD-CORE` | `PHASE-01` | Critical | `DONE` | Build + ArchTests Pass |
| `TASK-0002` | Planning Memory System (`planning/`) & Validation Script | `MOD-CORE` | `PHASE-01` | Critical | `DONE` | Validator + ArchTests Pass |
| `TASK-0003` | AI Agent Infrastructure (`AGENTS.md`, `.agents/`) | `MOD-CORE` | `PHASE-01` | Critical | `DONE` | Validator + ArchTests Pass |
| `TASK-0004` | Domain Models, Smart Grouping Rules & Layout Validation | `MOD-GROUPS` | `PHASE-01` | High | `DONE` | UnitTests Pass |
| `TASK-0005` | SQLite Persistence, Migrations & Settings Export/Import | `MOD-SETTINGS` | `PHASE-01` | High | `DONE` | IntegrationTests Pass |
| `TASK-0006` | Win32 Window Tracking, Monitor & Power Adapters | `MOD-WINDOWS` | `PHASE-02` | High | `DONE` | IntegrationTests Pass |
| `TASK-0007` | Working WPF Taskbar Prototype with Live Window Switching & Groups | `MOD-TASKBAR` | `PHASE-02` | High | `DONE` | Build + UI/UnitTests Pass |
| `TASK-0008` | Interactive Drag-and-Drop Reordering & Nested Folder Flyout UI | `MOD-FOLDERS` | `PHASE-03` | High | `READY` | Pending implementation |
| `TASK-0009` | Comprehensive 12-Section Settings Window & Live Theme Switching | `MOD-SETTINGS` | `PHASE-04` | Medium | `BACKLOG` | Pending implementation |
| `TASK-0010` | Per-Monitor Independent Taskbar Instances & DPI Scaling | `MOD-TASKBAR` | `PHASE-05` | Medium | `BACKLOG` | Pending implementation |
| `TASK-0011` | Long-Running Memory Leak & Idle CPU Benchmark Suite | `MOD-PERFORMANCE` | `PHASE-06` | Medium | `BACKLOG` | Pending implementation |
| `TASK-0012` | Windows Installer Package & Safe Uninstaller | `MOD-CORE` | `PHASE-07` | Low | `BACKLOG` | Pending implementation |

For full YAML task records and acceptance criteria, inspect:
- [Completed Tasks](planning/tasks/completed.md)
- [In-Progress & Ready Tasks](planning/tasks/in-progress.md)
- [Backlog Tasks](planning/tasks/backlog.md)
- [Blocked Tasks](planning/tasks/blocked.md)

