# Workflow: Bug Fixing (`planning/workflows/bug-fixing.md`)

1. **Identify Affected Module**: Use `planning/modules/module-registry.yaml` to locate the module and its test suite.
2. **Reproduce**: Trace the failing scenario and write an automated test in `tests/` that reproduces the failure.
3. **Fix Root Cause**: Apply the smallest safe fix that preserves Clean Architecture boundaries and handles errors explicitly via `OperationResult`.
4. **Verify**: Run `dotnet test Dockyra.sln` and confirm zero regressions.
5. **Document**: Update `planning/CURRENT-STATE.md` and `CHANGELOG.md`.

