# Workflow: Refactoring (`planning/workflows/refactoring.md`)

1. **Confirm Test Baseline**: Ensure all existing tests in `dotnet test Dockyra.sln` pass before starting.
2. **Check Layer Boundaries**: Review `planning/architecture/dependency-rules.md` to ensure the refactoring strengthens decoupling.
3. **Apply Incremental Changes**: Refactor in small steps without altering external behavior.
4. **Verify**: Run `dotnet test Dockyra.sln` (including `Dockyra.ArchitectureTests`) and `scripts/validate-planning.ps1`.
5. **Update Registry**: If file paths or contracts moved, update `planning/modules/module-registry.yaml`.

