# Dockyra Planning & Engineering Memory (`planning/`)

`planning/` is the canonical, tool-agnostic engineering knowledge base for **Dockyra**. Any developer or AI agent (Google Antigravity, Claude Code, OpenAI Codex, GitHub Copilot, Cursor) can understand, audit, and extend Dockyra by reading this directory without relying on prior chat transcripts.

---

## Quick Navigation Map

### 1. Core Identity & Live State
- [PROJECT-OVERVIEW.md](PROJECT-OVERVIEW.md) — What Dockyra is, who it serves, tech stack, and architectural principles.
- [CURRENT-STATE.md](CURRENT-STATE.md) — Live engineering state, verified build/test status, working modules, known limitations, and next priorities.
- [ROADMAP.md](ROADMAP.md) — Phase-by-phase development plan (`PHASE-01` through `PHASE-07`) with dependencies and exit criteria.
- [CHANGELOG.md](CHANGELOG.md) — Chronological engineering log of verified milestones.

### 2. Architecture (`architecture/`)
- [architecture/README.md](architecture/README.md) — Index of architectural specifications.
- [architecture/system-overview.md](architecture/system-overview.md) — High-level layered architecture and runtime composition.
- [architecture/dependency-rules.md](architecture/dependency-rules.md) — Strict project reference rules enforced by `Dockyra.ArchitectureTests`.
- [architecture/module-boundaries.md](architecture/module-boundaries.md) — Rules for modular isolation and `IModule` contracts.
- [architecture/data-flow.md](architecture/data-flow.md) — How user actions, domain rules, and SQLite persistence flow across layers.
- [architecture/event-flow.md](architecture/event-flow.md) — Event-driven window, monitor, and power notification pipelines.
- [architecture/ui-portability.md](architecture/ui-portability.md) — Strategy and boundaries for future `WPF` -> `WinUI 3` migration.
- [architecture/windows-integration.md](architecture/windows-integration.md) — Windows 11 APIs, DWM cloak filtering, and safe taskbar coexistence.
- [architecture/database-design.md](architecture/database-design.md) — SQLite schema, migrations, and JSON export/import.
- [architecture/security.md](architecture/security.md) — Least-privilege execution, path validation, P/Invoke safety, and Safe Mode.

### 3. Modules (`modules/`)
- [modules/README.md](modules/README.md) — How Dockyra modules are structured and documented.
- [modules/module-registry.yaml](modules/module-registry.yaml) — **Central machine-readable registry** of all modules, source paths, contracts, and tests.
- Module Specs: [taskbar.md](modules/taskbar.md), [groups.md](modules/groups.md), [folders.md](modules/folders.md), [windows.md](modules/windows.md), [profiles.md](modules/profiles.md), [themes.md](modules/themes.md), [settings.md](modules/settings.md), [performance.md](modules/performance.md), [power-management.md](modules/power-management.md).

### 4. Phases, Tasks & Features
- [phases/README.md](phases/README.md) — Phase specifications (`PHASE-01` to `PHASE-07`).
- [tasks/README.md](tasks/README.md) — Task tracking (`completed.md`, `in-progress.md`, `backlog.md`, `blocked.md`).
- [features/README.md](features/README.md) — Feature specs (`planned/`, `active/`, `completed/`).

### 5. Decisions, Workflows, Quality & Handoffs
- [decisions/README.md](decisions/README.md) — Architecture Decision Records (`ADR-0001` through `ADR-0004`).
- [agents/README.md](agents/README.md) — Multi-agent onboarding, responsibilities, handoff protocol, and context discipline.
- [workflows/new-feature.md](workflows/new-feature.md) — Standard engineering workflows (`new-feature`, `bug-fixing`, `refactoring`, `architecture-change`, `testing`, `release`).
- [quality/acceptance-criteria.md](quality/acceptance-criteria.md) — Quality gates, test strategy, performance budget, and known limitations.
- [handoffs/README.md](handoffs/README.md) — Cross-agent session handoff logs.

