# Backlog Tasks (`planning/tasks/backlog.md`)

```yaml
tasks:
  - id: TASK-0009
    title: Comprehensive 12-Section Settings Window & Live Theme Switching
    status: BACKLOG
    priority: medium
    module: MOD-SETTINGS
    phase: PHASE-04
    dependencies:
      - TASK-0007
    acceptance_criteria:
      - All 12 Settings sections (General, Taskbar, Appearance, Groups, Apps, Shortcuts, Multi-Monitor, Power, Notifications, Advanced, Backup, About) are navigable.
      - Setting changes apply live without requiring application restart.
    verification:
      build: not-run
      unit_tests: not-run
      integration_tests: not-run
    files_changed: []

  - id: TASK-0010
    title: Per-Monitor Independent Taskbar Instances & DPI Scaling
    status: BACKLOG
    priority: medium
    module: MOD-TASKBAR
    phase: PHASE-05
    dependencies:
      - TASK-0007
    acceptance_criteria:
      - Secondary monitors can host independent TaskbarWindow instances with per-monitor settings.
      - Display connect/disconnect events reposition bars without off-screen window loss.
    verification:
      build: not-run
      unit_tests: not-run
      integration_tests: not-run
    files_changed: []

  - id: TASK-0011
    title: Long-Running Memory Leak & Idle CPU Benchmark Suite
    status: BACKLOG
    priority: medium
    module: MOD-PERFORMANCE
    phase: PHASE-06
    dependencies:
      - TASK-0006
      - TASK-0007
    acceptance_criteria:
      - Automated endurance test simulates 1,000+ window open/close cycles with bounded memory growth.
      - Hardware test report documents idle CPU (< 0.5% target) and RAM (< 150 MB target).
    verification:
      build: not-run
      unit_tests: not-run
      integration_tests: not-run
    files_changed: []

  - id: TASK-0012
    title: Windows Installer Package & Safe Uninstaller
    status: BACKLOG
    priority: low
    module: MOD-CORE
    phase: PHASE-07
    dependencies:
      - TASK-0009
      - TASK-0010
      - TASK-0011
    acceptance_criteria:
      - Installer packages Dockyra without requiring permanent shell modifications.
      - Uninstaller restores default taskbar visibility and offers optional user data cleanup.
    verification:
      build: not-run
      unit_tests: not-run
      integration_tests: not-run
    files_changed: []
```

