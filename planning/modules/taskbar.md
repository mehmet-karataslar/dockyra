# Module Specification: Taskbar Shell & Layout (`MOD-TASKBAR`)

```yaml
id: MOD-TASKBAR
name: Taskbar Shell & Layout
status: in-progress
purpose: Manages taskbar dock edge, section partitioning, alignment, sizing, and primary bar presentation.
source_paths:
  - src/Dockyra.Domain/Entities/TaskbarLayout.cs
  - src/Dockyra.Application/Services/TaskbarOrchestrationService.cs
  - src/Dockyra.Modules/Taskbar/
  - src/Dockyra.Presentation.Wpf/ViewModels/TaskbarViewModel.cs
  - src/Dockyra.Presentation.Wpf/Views/TaskbarWindow.xaml
dependencies:
  - MOD-CORE
  - MOD-WINDOWS
  - MOD-GROUPS
  - MOD-PROFILES
  - MOD-THEMES
contracts:
  - ITaskbarService
  - ITaskbarLayoutRepository
  - IMonitorService
  - ITaskbarVisibilityService
tests:
  - tests/UnitTests/
  - tests/UITests/
owners:
  - wpf-developer
  - backend-developer
related_tasks:
  - TASK-0007
  - TASK-0010
```

## Responsibilities
- Maintain `TaskbarLayout` and ordered `TaskbarSection` definitions (`StartAndGroups`, `RunningApplications`, `SystemStatus`).
- Support dock edge (`Bottom`, `Top`, `Left`, `Right`), icon sizes (`24px`–`64px`), bar height, corner radius, and opacity.
- Validate layout invariants via `TaskbarLayoutValidator`.
- Position the docked `TaskbarWindow` on the selected monitor's work area (`IMonitorService`).

