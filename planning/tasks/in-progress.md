# In-Progress & Ready Tasks (`planning/tasks/in-progress.md`)

```yaml
tasks:
  - id: TASK-0008
    title: Interactive Drag-and-Drop Reordering & Nested Folder Flyout UI
    status: READY
    priority: high
    module: MOD-FOLDERS
    phase: PHASE-03
    dependencies:
      - TASK-0004
      - TASK-0007
    acceptance_criteria:
      - Users can drag applications between groups and reorder icons with visual drop-target adorners.
      - Changes persist immediately via ApplicationGroupService and IApplicationGroupRepository.
      - Keyboard reordering shortcuts work for accessibility.
    verification:
      build: not-run
      unit_tests: not-run
      integration_tests: not-run
    files_changed: []
```

