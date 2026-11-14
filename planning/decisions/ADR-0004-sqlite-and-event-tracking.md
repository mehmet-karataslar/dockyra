# ADR-0004: Local SQLite Persistence & Event-Driven Win32 Window Tracking

- **Status**: Accepted
- **Date**: 2026-11-14
- **Affected Modules**: `MOD-SETTINGS`, `MOD-WINDOWS`, `MOD-PERFORMANCE`

## 1. Context
Dockyra must persist relational user configurations (layouts, sections, groups, nested folder items, profiles, and classification rules) reliably while keeping memory overhead low, and must track open desktop windows without high-frequency CPU polling.

## 2. Decision
1. **Persistence (`Microsoft.Data.Sqlite`)**:
   - Use lightweight, direct parameterized ADO.NET (`Microsoft.Data.Sqlite`) with an explicit transactional `DatabaseMigrator` instead of heavy ORM runtime reflection overhead.
   - Pair SQLite storage with schema-versioned JSON export/import (`JsonSettingsBackupService`) for portable user backups.
2. **Event-Driven Window Tracking (`SetWinEventHook` + `EventCoalescer`)**:
   - Subscribe to out-of-context WinEvents (`WINEVENT_OUTOFCONTEXT | WINEVENT_SKIPOWNPROCESS`) and batch rapid bursts through `EventCoalescer` (`80ms` default quiet window, `200ms` in `PowerSaver` mode) rather than running a continuous high-frequency polling timer.

