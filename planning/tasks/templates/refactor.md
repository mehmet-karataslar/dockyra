# Refactor Task Template

```yaml
id: TASK-XXXX
title: Refactor <component> without behavior change
status: BACKLOG
priority: low
module: MOD-<NAME>
phase: PHASE-XX
dependencies: []
acceptance_criteria:
  - Layer boundaries preserved.
  - Existing unit and integration tests pass without regression.
verification:
  build: not-run
  unit_tests: not-run
  integration_tests: not-run
files_changed: []
```

