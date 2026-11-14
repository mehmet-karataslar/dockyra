# Module Specification: Adaptive Power Management (`MOD-POWER`)

```yaml
id: MOD-POWER
name: Adaptive Power Management
status: in-progress
purpose: Monitors Windows AC/Battery and Energy Saver status and coordinates PowerSaver, Balanced, and Performance modes.
source_paths:
  - src/Dockyra.Application/Services/PowerModeCoordinator.cs
  - src/Dockyra.WindowsIntegration/PowerManagement/Win32PowerMonitor.cs
  - src/Dockyra.Modules/PowerManagement/
dependencies:
  - MOD-CORE
  - MOD-PERFORMANCE
contracts:
  - IPowerMonitor
  - IPowerManagementService
tests:
  - tests/UnitTests/
  - tests/IntegrationTests/
owners:
  - windows-specialist
  - performance-engineer
related_tasks:
  - TASK-0006
  - TASK-0011
```

## Responsibilities
- Query and monitor Windows power status (`GetSystemPowerStatus` + power mode notifications) via `Win32PowerMonitor`.
- Coordinate `PowerMode.PowerSaver`, `PowerMode.Balanced`, and `PowerMode.Performance` in `PowerModeCoordinator`.
- Automatically transition to `PowerSaver` when running on battery or Windows Energy Saver if `AutoPowerSaveOnBattery` is enabled.

