# Workflow: New Feature Development (`planning/workflows/new-feature.md`)

1. **Understand Context**: Read `AGENTS.md` and `planning/CURRENT-STATE.md`.
2. **Locate Module**: Look up the owning module in `planning/modules/module-registry.yaml`.
3. **Inspect Existing Code**: Review `Domain`, `Contracts`, `Application`, `Infrastructure`/`WindowsIntegration`, `Presentation.Wpf`, and `tests/` for that module.
4. **Plan & Specify**: Create `planning/features/active/FEATURE-XXXX-<slug>.md` and register a `TASK-XXXX` in `planning/tasks/in-progress.md`.
5. **Implement End-to-End**: Build Domain rules -> Application use case -> Adapter/Repository -> WPF ViewModel/View with DesignTokens & Localization.
6. **Verify**: Run `dotnet build Dockyra.sln` and `dotnet test Dockyra.sln`.
7. **Update Memory**: Update `module-registry.yaml` (if paths/contracts changed), `planning/CURRENT-STATE.md`, `planning/tasks/`, and `CHANGELOG.md`, then run `scripts/validate-planning.ps1`.

