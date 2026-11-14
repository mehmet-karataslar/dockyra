---
name: project-onboarding
description: >-
  Use this skill when an AI agent or developer starts a new session on Dockyra
  or takes over work from another agent without prior conversation history.
---

# Dockyra Project Onboarding Skill

## When to Use
Use this procedure whenever starting work on Dockyra for the first time in a session, or resuming after a handoff from another AI tool (Antigravity, Claude Code, Codex, Copilot).

## Prerequisites
- Workspace root is `Dockyra/`.
- `.NET 10 SDK` is available.

## Mandatory Reading Sequence
Read these files in order before inspecting source files:
1. `AGENTS.md`
2. `planning/README.md`
3. `planning/PROJECT-OVERVIEW.md`
4. `planning/CURRENT-STATE.md`
5. `planning/architecture/README.md`
6. `planning/modules/module-registry.yaml`
7. `planning/ROADMAP.md`
8. Latest handoff in `planning/handoffs/` and active tasks in `planning/tasks/in-progress.md`

## Step-by-Step Procedure
1. **Read Core Memory**: Complete the reading sequence above to identify the current phase, active tasks, and known blockers.
2. **Verify Baseline Health**:
   - Run `dotnet build Dockyra.sln`
   - Run `dotnet test Dockyra.sln`
   - Run `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/validate-planning.ps1`
3. **Locate Target Module**:
   - Match the user's request to a module ID (`MOD-*`) in `planning/modules/module-registry.yaml`.
   - Read only the corresponding `planning/modules/<module>.md` and `source_paths`.

## Verification & Expected Output
- Summarize the verified project state (build status, test count, active phase, and target module) before proposing or executing code changes.

