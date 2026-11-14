# Workflow: Architecture Change (`planning/workflows/architecture-change.md`)

1. **Evaluate Impact**: Consult `.agents/agents/architect.md` and `planning/architecture/dependency-rules.md`.
2. **Draft ADR**: Create a new `ADR-XXXX-<slug>.md` in `planning/decisions/` explaining context, options considered, decision, and trade-offs. Never delete historical ADRs.
3. **Update Architecture Tests**: Add or update rules in `tests/ArchitectureTests/Dockyra.ArchitectureTests.csproj`.
4. **Execute & Validate**: Apply the structural change, run `dotnet test Dockyra.sln`, and run `scripts/validate-planning.ps1`.

