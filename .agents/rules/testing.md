---
trigger: always_on
---

# Dockyra Testing & Verification Rules

1. **Test Along With Code**:
   - Every new Domain rule, Application use case, SQLite repository, or ViewModel change must include corresponding automated tests in `tests/`.
2. **Mandatory Pre-Completion Checks**:
   - Run `dotnet build Dockyra.sln` and `dotnet test Dockyra.sln` before marking any task `DONE`.
   - Run `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/validate-planning.ps1` when modifying modules, tasks, or documentation.
3. **Honest Reporting**:
   - Never report a test as passed unless it was actually executed and succeeded.
   - Clearly separate automated test verification from interactive Windows 11 desktop verification.

