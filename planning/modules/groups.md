# Module Specification: Application Groups (`MOD-GROUPS`)

```yaml
id: MOD-GROUPS
name: Application Groups
status: in-progress
purpose: Organizes applications into user-defined groups and applies deterministic smart classification rules.
source_paths:
  - src/Dockyra.Domain/Entities/ApplicationGroup.cs
  - src/Dockyra.Domain/Rules/SmartGroupingRuleEngine.cs
  - src/Dockyra.Application/Services/ApplicationGroupService.cs
  - src/Dockyra.Modules/Groups/
dependencies:
  - MOD-CORE
  - MOD-SETTINGS
contracts:
  - IGroupService
  - IApplicationGroupRepository
tests:
  - tests/UnitTests/
  - tests/IntegrationTests/
owners:
  - backend-developer
  - wpf-developer
related_tasks:
  - TASK-0004
  - TASK-0008
```

## Responsibilities
- Manage user-created `ApplicationGroup` entities (Name, Icon, ColorHex, Description, SortOrder, ViewMode, Items).
- Evaluate `SmartGroupingRuleEngine`:
  - User-defined classification rules (`IsUserDefined = true`) **always** take priority over built-in automatic rules.
  - Built-in rules classify common tools (e.g., `Code.exe` / `WindowsTerminal.exe` -> *Software Development*, `chrome.exe` / `msedge.exe` -> *Browsers*, `Discord.exe` -> *Communication*).
  - Never mutate existing user groups without explicit user action.

