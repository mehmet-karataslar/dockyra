# AI Agent Role Matrix (`planning/agents/responsibilities.md`)

| Agent | Spec File | Owned Layers / Folders | Key Mandate |
|---|---|---|---|
| **Architect** | `.agents/agents/architect.md` | All `.csproj`, `planning/architecture/`, `planning/decisions/` | Enforce Clean Architecture, UI/OS decoupling, and WinUI 3 portability. |
| **Planner** | `.agents/agents/planner.md` | `planning/ROADMAP.md`, `planning/tasks/`, `planning/features/` | Decompose requests into traceable `TASK-XXXX` items with acceptance criteria. |
| **WPF Developer** | `.agents/agents/wpf-developer.md` | `src/Dockyra.Presentation.Wpf/`, `src/Dockyra.Desktop/` | Build Views, ViewModels, Fluent DesignTokens, themes, and localization. |
| **Windows Specialist** | `.agents/agents/windows-specialist.md` | `src/Dockyra.WindowsIntegration/` | Implement safe Win32, DWM, monitor, power, and process launch adapters. |
| **Backend Developer** | `.agents/agents/backend-developer.md` | `src/Dockyra.Domain/`, `Application/`, `Contracts/`, `Infrastructure/` | Implement domain entities, rules, use cases, SQLite migrations, and JSON backup. |
| **Performance Engineer** | `.agents/agents/performance-engineer.md` | `tests/PerformanceTests/`, `EventCoalescer`, `PowerModeCoordinator` | Keep idle CPU `< 0.5%`, RAM `< 150 MB`, and prevent memory leaks. |
| **QA Engineer** | `.agents/agents/qa-engineer.md` | `tests/` | Write and execute Unit, Integration, UI, Architecture, and Performance tests. |
| **Security Reviewer** | `.agents/agents/security-reviewer.md` | Read-only audit across `src/` | Verify least privilege, path validation, SQL parameterization, and P/Invoke safety. |
| **Documentation Agent** | `.agents/agents/documentation-agent.md` | `planning/`, `CHANGELOG.md`, `TASKS.md` | Keep `CURRENT-STATE.md`, `module-registry.yaml`, and handoffs 100% accurate. |

