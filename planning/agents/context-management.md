# Context Management Rules (`planning/agents/context-management.md`)

1. **Map First, Drill Down Second**:
   - Read `planning/CURRENT-STATE.md` and `planning/modules/module-registry.yaml` first.
   - Use `source_paths` and `tests` in `module-registry.yaml` to open only the files belonging to the target module.
2. **Avoid Full-Repo Scans**:
   - Do not read unrelated modules, test suites, or archived phase documents when working on a scoped task.
3. **Keep Root Docs Concise**:
   - Root files (`AGENTS.md`, `ARCHITECTURE.md`, `ROADMAP.md`, `TASKS.md`) act as concise indexes pointing to `planning/`.

