# PHASE-01: Foundation, Clean Architecture, Planning & AI Agent System

- **Phase ID**: `PHASE-01`
- **Status**: `IN_PROGRESS` (Core Foundation Completed)
- **Prerequisites**: None

## 1. Objective
Establish the `.NET 10` Clean Architecture solution (`Dockyra.sln`), enforce strict UI and WindowsIntegration separation, build the SQLite persistence layer, create the `planning/` engineering memory system, and configure the `.agents/` AI agent and skill ecosystem.

## 2. Affected Modules
- `MOD-CORE`, `MOD-SETTINGS`, `MOD-THEMES`

## 3. Technical Risks & Mitigations
- **Risk**: Accidental coupling of WPF ViewModels to Win32 APIs.
  - **Mitigation**: Automated enforcement in `Dockyra.ArchitectureTests`.
- **Risk**: Documentation drift across sessions.
  - **Mitigation**: `scripts/validate-planning.ps1` and `PlanningConsistencyTests`.

## 4. Acceptance Criteria
- [x] `Dockyra.sln` compiles cleanly on `.NET 10`.
- [x] `Dockyra.Presentation.Wpf` has zero project reference to `Dockyra.WindowsIntegration` or `Dockyra.Infrastructure`.
- [x] `planning/` directory and `module-registry.yaml` are populated and verified.
- [x] `AGENTS.md`, `.agents/agents/`, `.agents/rules/`, and `.agents/skills/` are created and validated.
- [x] SQLite schema migrations and repositories pass automated integration tests.

