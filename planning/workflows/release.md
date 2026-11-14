# Workflow: Release Preparation (`planning/workflows/release.md`)

1. **Release Build & Test**:
   - `dotnet build Dockyra.sln -c Release`
   - `dotnet test Dockyra.sln -c Release`
2. **Planning Integrity Check**:
   - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/validate-planning.ps1`
3. **Safety Verification**:
   - Verify `SafeModeStartupCoordinator` recovery flow and JSON settings backup/import.
4. **Documentation & Changelog**:
   - Synchronize `CHANGELOG.md`, `planning/CHANGELOG.md`, `planning/CURRENT-STATE.md`, and `planning/releases/README.md`.

