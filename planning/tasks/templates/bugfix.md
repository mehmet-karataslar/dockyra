# Bugfix Task Template

```yaml
id: TASK-XXXX
title: Fix <defect summary>
status: READY
priority: high
module: MOD-<NAME>
phase: PHASE-XX
dependencies: []
acceptance_criteria:
  - Root cause identified and documented.
  - Regression test added and passing.
verification:
  build: not-run
  unit_tests: not-run
  integration_tests: not-run
files_changed: []
```

