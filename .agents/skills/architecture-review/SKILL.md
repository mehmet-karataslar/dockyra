---
name: architecture-review
description: >-
  Use this skill to audit Dockyra layer dependencies, UI/WindowsIntegration separation,
  WinUI 3 portability, and planning documentation consistency.
---

# Dockyra Architecture Review Skill

## When to Use
Use before completing major features, after refactoring, or when requested to audit architectural compliance.

## Documents to Read
1. `planning/architecture/dependency-rules.md`
2. `planning/architecture/ui-portability.md`
3. `planning/decisions/ADR-0001-architecture.md`
4. `planning/decisions/ADR-0002-ui-separation.md`

## Step-by-Step Procedure
1. **Verify Project References**:
   - Confirm `Dockyra.Domain` has zero project references.
   - Confirm `Dockyra.Application` references only `Dockyra.Domain` and `Dockyra.Contracts`.
   - Confirm `Dockyra.Presentation.Wpf` has no reference to `Dockyra.WindowsIntegration` or `Dockyra.Infrastructure`.
   - Confirm `Dockyra.WindowsIntegration` has no reference to `Dockyra.Presentation.Wpf`.
2. **Verify Type Isolation**:
   - Check that no `IntPtr`, `HWND`, or P/Invoke declarations exist outside `Dockyra.WindowsIntegration`.
   - Check that no `System.Windows.*` types exist in `Domain`, `Contracts`, or `Application`.
3. **Run Automated Architecture & Planning Checks**:
   - Execute `dotnet test tests/ArchitectureTests/Dockyra.ArchitectureTests.csproj`.
   - Execute `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/validate-planning.ps1`.

## Expected Output
Structured compliance report listing verified rules, any detected violations, and test output.

