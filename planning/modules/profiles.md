# Module Specification: Work Profiles (`MOD-PROFILES`)

```yaml
id: MOD-PROFILES
name: Work Profiles & Context Switching
status: in-progress
purpose: Manages workspace profiles (Development, Daily Use, Gaming) and switches taskbar layouts dynamically.
source_paths:
  - src/Dockyra.Domain/Entities/WorkProfile.cs
  - src/Dockyra.Application/Services/ProfileManagementService.cs
  - src/Dockyra.Modules/Profiles/
dependencies:
  - MOD-TASKBAR
  - MOD-SETTINGS
contracts:
  - IProfileService
  - IProfileRepository
tests:
  - tests/UnitTests/
  - tests/IntegrationTests/
owners:
  - backend-developer
  - wpf-developer
related_tasks:
  - TASK-0005
  - TASK-0007
```

## Responsibilities
- Create, clone, switch, and persist `WorkProfile` instances in SQLite.
- Provide ready-to-use default profiles (*Software Development*, *Daily Use*, *Gaming*).
- Notify `TaskbarViewModel` when the active profile changes so sections and groups update immediately without restart.

