# AGENTS.md — Dockyra AI Agent Entry Point & Operating Manual

Welcome to **Dockyra**, a modular Windows 11 taskbar and desktop workspace management system built with **C#**, **.NET 10**, **WPF**, **Win32/Shell APIs**, **SQLite**, **MVVM**, and **Clean Architecture**.

Dockyra consists of three interconnected systems:
1. **Application System (`src/`, `tests/`)**: The production Windows 11 desktop application and automated verification suites.
2. **Planning System (`planning/`)**: The single source of truth for product identity, architecture, module boundaries, tasks, roadmap, ADRs, and agent handoffs.
3. **Agent System (`AGENTS.md`, `.agents/`)**: Role-specialized AI agents, permanent rules, and step-by-step reusable workflows (skills).

---

## 1. Mandatory Reading Order Before Making Changes

Every AI agent (Antigravity, Claude Code, OpenAI Codex, GitHub Copilot, Cursor, etc.) **must** inspect the project in this order before writing or modifying code:

1. `AGENTS.md` (this file)
2. `planning/README.md`
3. `planning/PROJECT-OVERVIEW.md`
4. `planning/CURRENT-STATE.md`
5. `planning/architecture/README.md`
6. `planning/modules/module-registry.yaml`
7. `planning/ROADMAP.md`
8. Relevant task (`planning/tasks/`) and module (`planning/modules/<module>.md`) documents
9. Affected source files in `src/`
10. Relevant test suites in `tests/`

> **Context Discipline**: Do not read every file in the repository on every turn. Read the map first (`CURRENT-STATE.md` and `module-registry.yaml`), then inspect only the files relevant to your active task.

---

## 2. Non-Negotiable Architectural Rules

1. **Strict UI and OS Separation**:
   - `Dockyra.Presentation.Wpf` **must never** reference `Dockyra.WindowsIntegration` or `Dockyra.Infrastructure`, and **must never** invoke Win32 P/Invoke (`user32.dll`, `dwmapi.dll`, `shell32.dll`), `HWND`/`IntPtr` manipulation, or SQL queries directly.
   - `Dockyra.WindowsIntegration` **must never** reference `Dockyra.Presentation.Wpf`.
2. **Clean Architecture Layer Dependencies**:
   - `Dockyra.Domain`: Pure business entities, value objects, enums, and rules. Zero external layer or platform dependencies.
   - `Dockyra.Contracts`: Stable cross-layer and cross-module interfaces, DTOs, and result types (`WindowId`, `WindowInfo`, `OperationResult`, `IWindowManager`, `IPowerMonitor`, `IMonitorService`, `IApplicationLauncher`). No native `HWND` or WPF types allowed.
   - `Dockyra.Application`: Use cases, application orchestration services, and repository/adapter contracts. Depends only on `Dockyra.Domain` and `Dockyra.Contracts`.
   - `Dockyra.Infrastructure`: SQLite persistence, migrations, settings serialization, logging, and file/recovery stores. Implements `Application`/`Contracts` interfaces.
   - `Dockyra.WindowsIntegration`: Win32, DWM, Shell, monitor, power, and process launch implementations behind `Contracts`/`Application` interfaces.
   - `Dockyra.Modules`: Feature module descriptors and isolated module services. Modules communicate only via contracts/events, never via another module's internal classes.
   - `Dockyra.Presentation.Wpf`: WPF Views, ViewModels (`CommunityToolkit.Mvvm`), Fluent design tokens, themes, and localization.
   - `Dockyra.Desktop`: Composition Root (`App.xaml.cs`, `Bootstrapper/ServiceRegistration.cs`), Dependency Injection wiring, single-instance guard, and safe-mode startup coordinator.
3. **Future UI Portability (`WinUI 3` Readiness)**:
   - Business rules, SQLite persistence, and `WindowsIntegration` services must remain reusable if `Dockyra.Presentation.WinUI` is added in the future. See `planning/architecture/ui-portability.md`.

---

## 3. Windows 11 Safety & Resource Rules

- **Never** terminate `explorer.exe`, inject DLLs into Windows Shell processes, patch system binaries, or perform irreversible registry edits.
- Always preserve the user's ability to restore or access the native Windows 11 taskbar, Start menu, and system tray (`Safe Mode` and graceful exit).
- **Event-Driven Performance**: Avoid high-frequency polling loops on the UI thread. Coalesce/debounce window and power events. Respect battery-saver and power modes (`PowerSaver`, `Balanced`, `Performance`).
- Validate all executable paths and launch arguments before starting external processes.

---

## 4. Operating Modes & Specialized Agents

Select the appropriate operating mode and consult the matching agent/skill in `.agents/`:

| Mode | Primary Agent (`.agents/agents/`) | Primary Skill (`.agents/skills/`) | Purpose |
|---|---|---|---|
| **PLAN MODE** | `planner.md`, `architect.md` | `project-onboarding`, `create-module` | Analyze requests, check `module-registry.yaml`, define tasks and acceptance criteria before coding. |
| **IMPLEMENT MODE** | `backend-developer.md`, `wpf-developer.md`, `windows-specialist.md` | `implement-feature`, `create-module` | Build end-to-end features across Domain, Application, Infrastructure/WindowsIntegration, WPF UI, and Tests. |
| **DEBUG MODE** | `qa-engineer.md`, `windows-specialist.md` | `fix-bug` | Reproduce issues, identify root causes, apply minimal safe fixes, and add regression tests. |
| **REVIEW MODE** | `architect.md`, `security-reviewer.md` | `architecture-review`, `release-check` | Verify layer boundaries, P/Invoke safety, error handling, and test coverage. |
| **PERFORMANCE MODE** | `performance-engineer.md` | `performance-audit` | Measure CPU, RAM, GPU, and battery impact; eliminate allocations and leaks. |
| **DOCUMENTATION MODE** | `documentation-agent.md` | `project-onboarding`, `release-check` | Synchronize `planning/CURRENT-STATE.md`, `planning/tasks/`, `module-registry.yaml`, and `planning/handoffs/`. |

---

## 5. Mandatory Verification & Definition of Done

Before marking any task as `DONE`:
1. Build the entire solution: `dotnet build Dockyra.sln`
2. Run all automated tests (including architecture & planning verification): `dotnet test Dockyra.sln`
3. Run the planning consistency check script: `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/validate-planning.ps1`
4. Update `planning/CURRENT-STATE.md`, the active task entry in `planning/tasks/`, `planning/modules/module-registry.yaml` (if modules/contracts changed), and `CHANGELOG.md`.
5. Never claim unverified Windows behavior as tested; clearly distinguish verified test results from pending manual verification.
6. Never push to a remote Git repository, delete major modules, or run destructive migrations without explicit user confirmation.

