# Module Specification: Settings, SQLite Persistence & Backup (`MOD-SETTINGS`)

```yaml
id: MOD-SETTINGS
name: Settings, SQLite Persistence & Backup
status: in-progress
purpose: Manages user preferences, SQLite database migrations, repositories, and JSON backup/restore.
source_paths:
  - src/Dockyra.Domain/Entities/UserPreferences.cs
  - src/Dockyra.Infrastructure/Persistence/
  - src/Dockyra.Infrastructure/Backup/
  - src/Dockyra.Modules/Settings/
  - src/Dockyra.Presentation.Wpf/Views/SettingsWindow.xaml
dependencies:
  - MOD-CORE
contracts:
  - IUserSettingsRepository
  - ISettingsBackupService
tests:
  - tests/IntegrationTests/
owners:
  - backend-developer
  - wpf-developer
related_tasks:
  - TASK-0005
  - TASK-0009
```

## Responsibilities
- Persist and load `UserPreferences`, `TaskbarLayout`, `ApplicationGroup`, and `WorkProfile` records in SQLite (`dockyra.db`).
- Run transactional schema migrations via `DatabaseMigrator`.
- Export and import full user settings in schema-versioned JSON via `JsonSettingsBackupService`.
- Present user configuration options in `SettingsWindow`.

