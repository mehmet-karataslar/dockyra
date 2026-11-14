# ADR-0001: Clean Architecture, Modular Monolith & WPF on .NET 10

- **Status**: Accepted
- **Date**: 2026-11-14
- **Affected Modules**: All (`MOD-CORE`, `MOD-TASKBAR`, `MOD-GROUPS`, `MOD-FOLDERS`, `MOD-WINDOWS`, `MOD-PROFILES`, `MOD-THEMES`, `MOD-SETTINGS`, `MOD-PERFORMANCE`, `MOD-POWER`)

## 1. Context
Dockyra requires a fast, low-memory, highly customizable desktop shell interface on Windows 11 that can be maintained over years by both human engineers and multiple AI coding agents.

## 2. Considered Options
1. **WPF on .NET 10 with Clean Architecture + Modular Monolith**
2. **WinUI 3 on Windows App SDK**
3. **Electron / WebView2 Hybrid**

## 3. Decision & Rationale
We chose **WPF on .NET 10 LTS** structured as a **Clean Architecture Modular Monolith**:
- **Why WPF over WinUI 3 initially**: WPF offers mature, battle-tested control over borderless desktop bar windows (`WindowStyle.None`, transparent popup flyouts, fast startup, per-monitor positioning, and rock-solid `dotnet test` headless compatibility) without Windows App SDK packaging friction.
- **Why not Electron/WebView2**: A browser runtime consumes `200–400+ MB` of RAM when idle, violating our `< 150 MB` RAM and `< 0.5%` idle CPU budgets.
- **Why Clean Architecture**: Isolating `Domain`, `Application`, and `Contracts` ensures business rules can be tested deterministically in milliseconds and preserves a clean migration path to `WinUI 3` in the future (see `ADR-0002`).

