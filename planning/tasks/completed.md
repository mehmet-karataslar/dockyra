# Completed Tasks (`planning/tasks/completed.md`)

```yaml
tasks:
  - id: TASK-0001
    title: Workspace Discovery, Clean Architecture Solution & Layer Setup
    status: DONE
    priority: critical
    module: MOD-CORE
    phase: PHASE-01
    dependencies: []
    acceptance_criteria:
      - Dockyra.sln created with 8 src projects and 5 test projects on .NET 10.
      - Presentation.Wpf has zero reference to WindowsIntegration or Infrastructure.
      - Domain has zero project or external package references.
    verification:
      build: passed
      unit_tests: passed
      integration_tests: passed
    files_changed:
      - Dockyra.sln
      - Directory.Build.props
      - Directory.Packages.props

  - id: TASK-0002
    title: Planning Memory System (planning/) & Validation Script
    status: DONE
    priority: critical
    module: MOD-CORE
    phase: PHASE-01
    dependencies:
      - TASK-0001
    acceptance_criteria:
      - planning/ hierarchy populated with accurate Dockyra architecture, modules, phases, tasks, and ADRs.
      - module-registry.yaml maps all modules to real source and test paths.
      - scripts/validate-planning.ps1 verifies links, module IDs, and task IDs.
    verification:
      build: passed
      unit_tests: passed
      integration_tests: passed
    files_changed:
      - planning/README.md
      - planning/PROJECT-OVERVIEW.md
      - planning/CURRENT-STATE.md
      - planning/ROADMAP.md
      - planning/modules/module-registry.yaml
      - scripts/validate-planning.ps1

  - id: TASK-0003
    title: AI Agent Infrastructure (AGENTS.md, .agents/)
    status: DONE
    priority: critical
    module: MOD-CORE
    phase: PHASE-01
    dependencies:
      - TASK-0002
    acceptance_criteria:
      - Root AGENTS.md defines mandatory reading order and architectural rules.
      - 9 specialized agents created under .agents/agents/.
      - 6 always-on rules created under .agents/rules/.
      - 7 procedural skills created under .agents/skills/*/SKILL.md.
    verification:
      build: passed
      unit_tests: passed
      integration_tests: passed
    files_changed:
      - AGENTS.md
      - .agents/agents/
      - .agents/rules/
      - .agents/skills/

  - id: TASK-0004
    title: Domain Models, Smart Grouping Rules & Layout Validation
    status: DONE
    priority: high
    module: MOD-GROUPS
    phase: PHASE-01
    dependencies:
      - TASK-0001
    acceptance_criteria:
      - TaskbarLayout, ApplicationGroup, FolderItem, WorkProfile, and UserPreferences entities implemented.
      - SmartGroupingRuleEngine prioritizes user rules over automatic rules.
      - FolderHierarchyRules enforces max depth (3) and prevents cycles.
      - TaskbarLayoutValidator validates section and size invariants.
    verification:
      build: passed
      unit_tests: passed
      integration_tests: passed
    files_changed:
      - src/Dockyra.Domain/

  - id: TASK-0005
    title: SQLite Persistence, Migrations & Settings Export/Import
    status: DONE
    priority: high
    module: MOD-SETTINGS
    phase: PHASE-01
    dependencies:
      - TASK-0004
    acceptance_criteria:
      - DatabaseMigrator creates and versions SQLite tables transactionally.
      - Repositories persist and retrieve groups, layouts, profiles, and user settings with parameterized SQL.
      - JsonSettingsBackupService exports and imports schema-versioned JSON settings.
      - FileSafeModeStateStore tracks consecutive startup failures.
    verification:
      build: passed
      unit_tests: passed
      integration_tests: passed
    files_changed:
      - src/Dockyra.Infrastructure/

  - id: TASK-0006
    title: Win32 Window Tracking, Monitor & Power Adapters
    status: DONE
    priority: high
    module: MOD-WINDOWS
    phase: PHASE-02
    dependencies:
      - TASK-0001
    acceptance_criteria:
      - Win32WindowManager enumerates visible windows, filters DWM-cloaked frames, and hooks WinEvents with EventCoalescer.
      - Win32MonitorService enumerates physical displays and work areas.
      - Win32PowerMonitor reports AC/Battery and Energy Saver status.
      - Win32ApplicationLauncher validates executable paths before launching.
    verification:
      build: passed
      unit_tests: passed
      integration_tests: passed
    files_changed:
      - src/Dockyra.WindowsIntegration/

  - id: TASK-0007
    title: Working WPF Taskbar Prototype with Live Window Switching & Groups
    status: DONE
    priority: high
    module: MOD-TASKBAR
    phase: PHASE-02
    dependencies:
      - TASK-0004
      - TASK-0005
      - TASK-0006
    acceptance_criteria:
      - TaskbarWindow and TaskbarViewModel display application groups, open windows, active profile, and power mode.
      - FolderFlyoutControl filters items by search query and toggles Grid/List views.
      - Fluent Dark/Light themes and tr-TR/en-US localization dictionaries are wired.
    verification:
      build: passed
      unit_tests: passed
      integration_tests: passed
    files_changed:
      - src/Dockyra.Presentation.Wpf/
      - src/Dockyra.Desktop/
```

