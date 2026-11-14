---
name: implement-feature
description: >-
  Use this skill when implementing any new product feature or enhancement in Dockyra
  from initial analysis and planning through coding, testing, and documentation updates.
---

# Dockyra Feature Implementation Skill

## When to Use
Use when the user asks to add a new feature or enhance an existing capability in Dockyra (e.g., adding folder search, new taskbar section behavior, or profile rules).

## Prerequisites
- Onboarding completed (`AGENTS.md`, `planning/CURRENT-STATE.md`, `planning/modules/module-registry.yaml` reviewed).

## Documents to Read
1. `planning/modules/module-registry.yaml`
2. Target module documentation in `planning/modules/<module>.md`
3. `planning/architecture/dependency-rules.md`
4. `planning/workflows/new-feature.md`

## Step-by-Step Procedure
1. **Identify Module & Scope**:
   - Locate the owning module (`MOD-*`) in `module-registry.yaml`. Do not create a new module if an existing one owns the domain.
2. **Inspect Existing Layers**:
   - Review affected `Domain` models, `Contracts`, `Application` services, `Infrastructure`/`WindowsIntegration` adapters, `Presentation.Wpf` ViewModels/Views, and `tests/`.
3. **Create Feature Specification**:
   - Create `planning/features/active/FEATURE-XXXX-<slug>.md` with purpose, user scenarios, technical approach, affected files, risks, acceptance criteria, and test plan.
   - Add or update the corresponding `TASK-XXXX` entry in `planning/tasks/in-progress.md`.
4. **Implement End-to-End Across Clean Architecture**:
   - **Domain**: Entities, value objects, and pure validation/business rules.
   - **Contracts & Application**: Interfaces, DTOs, and orchestration services.
   - **Infrastructure / WindowsIntegration**: SQLite persistence or Win32 adapter implementation (if needed).
   - **Presentation.Wpf**: ViewModel commands/properties, XAML bindings, `DesignTokens`, and `tr-TR`/`en-US` localization keys.
5. **Write & Run Automated Tests**:
   - Add Unit, Integration, UI, and Architecture tests covering happy paths and edge cases.
   - Execute `dotnet test Dockyra.sln`.
6. **Update Planning & Governance**:
   - Move feature doc to `planning/features/completed/` once verified.
   - Move task to `planning/tasks/completed.md` and update `TASKS.md`, `planning/CURRENT-STATE.md`, and `CHANGELOG.md`.
   - Run `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/validate-planning.ps1`.

## Expected Output
Report to the user: what was implemented, changed files, test execution results, remaining limitations, and recommended next steps.

