---
name: create-module
description: >-
  Use this skill when adding a genuinely new functional module to Dockyra that is
  not already covered by an existing module in module-registry.yaml.
---

# Dockyra Module Creation Skill

## When to Use
Use only after checking `planning/modules/module-registry.yaml` and confirming that no existing module covers the new functional domain.

## Documents to Read
1. `planning/modules/module-registry.yaml`
2. `planning/templates/module-template.md`
3. `planning/architecture/module-boundaries.md`

## Step-by-Step Procedure
1. **Verify Necessity**:
   - Confirm with `architect.md` rules that a new `MOD-<NAME>` is justified.
2. **Define Contracts**:
   - Place public cross-module interfaces and DTOs in `src/Dockyra.Contracts/` or `src/Dockyra.Application/Interfaces/`.
3. **Implement Module Registration**:
   - Create the module descriptor implementing `IModule` under `src/Dockyra.Modules/<ModuleName>/`.
   - Keep internal implementation details `internal` so other modules cannot couple to them directly.
4. **Register in DI & Module Host**:
   - Wire the module in `src/Dockyra.Desktop/Bootstrapper/ServiceRegistration.cs`.
5. **Document & Register**:
   - Create `planning/modules/<module-slug>.md` using `planning/templates/module-template.md`.
   - Add the entry to `planning/modules/module-registry.yaml`.
6. **Validate**:
   - Add unit/architecture tests and run `dotnet test Dockyra.sln` + `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/validate-planning.ps1`.

## Expected Output
New `IModule` implementation, updated `module-registry.yaml`, module spec markdown, and passing architecture/unit tests.

