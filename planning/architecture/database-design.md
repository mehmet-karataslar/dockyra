# SQLite Database & Persistence Design (`planning/architecture/database-design.md`)

## 1. Local-First Storage Principle
Dockyra stores all user configurations locally in SQLite (`%LocalAppData%\Dockyra\dockyra.db`) with zero cloud account requirement. In-memory or custom file paths can be injected for automated integration tests.

## 2. Database Schema (Schema Version 1)

| Table Name | Primary Key | Purpose |
|---|---|---|
| `SchemaMigrations` | `Version (INTEGER)` | Tracks applied schema versions and UTC timestamps. |
| `UserSettings` | `SettingKey (TEXT)` | Key-value and structured user preferences (theme, language, power mode, dock edge, auto-hide). |
| `TaskbarLayouts` | `Id (TEXT)` | Named taskbar layouts (`DockEdge`, `Alignment`, `IconSize`, `Opacity`, `CornerRadius`, `IsLocked`). |
| `TaskbarSections` | `Id (TEXT)` | Ordered sections within a `TaskbarLayout` (`SectionKind`, `Name`, `SortOrder`, `IsVisible`, `IsPinned`). |
| `ApplicationGroups` | `Id (TEXT)` | Custom user application groups (`Name`, `Icon`, `ColorHex`, `Description`, `SortOrder`, `ViewMode`). |
| `ApplicationGroupItems` | `Id (TEXT)` | Applications and nested folder items belonging to an `ApplicationGroup` (`ExecutablePath`, `DisplayName`, `Arguments`, `ParentFolderId`, `SortOrder`). |
| `WorkProfiles` | `Id (TEXT)` | Saved workspace profiles (`Name`, `Description`, `LayoutId`, `IsDefault`, `SortOrder`). |
| `ClassificationRules` | `Id (TEXT)` | User and smart auto-grouping rules (`Pattern`, `TargetGroupId`, `IsUserDefined`, `Priority`). |

## 3. Migration & Recovery Strategy
- `DatabaseMigrator` runs inside a transaction on startup and upgrades the schema incrementally.
- All SQL queries use parameterized commands (`$id`, `$name`, etc.) via `Microsoft.Data.Sqlite`.
- Users can export and import their complete configuration as a schema-versioned JSON document (`SchemaVersion: 1`) via `JsonSettingsBackupService`.

