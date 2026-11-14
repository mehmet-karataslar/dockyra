# Module Specification Template (`planning/templates/module-template.md`)

```yaml
id: MOD-<NAME>
name: <Human-Readable Module Name>
status: planned | in-progress | active
purpose: >
  <Concise description of module responsibility>
source_paths:
  - src/Dockyra.Modules/<Name>/
dependencies:
  - MOD-CORE
contracts:
  - I<Name>Service
tests:
  - tests/UnitTests/
owners:
  - backend-developer
related_tasks:
  - TASK-XXXX
```

## Responsibilities
- Detail the domain models, application contracts, and boundaries of this module.

