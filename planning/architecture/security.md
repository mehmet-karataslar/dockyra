# Security & Recovery Architecture (`planning/architecture/security.md`)

## 1. Least-Privilege Execution
- Dockyra runs with standard user privileges (`asInvoker`).
- Administrator elevation is never requested for normal operation.

## 2. Process Launching & File Path Validation
- `Win32ApplicationLauncher` validates every `ApplicationLaunchRequest`:
  - Rejects empty or whitespace paths.
  - Requires a rooted, valid local file path (`Path.IsPathFullyQualified`) and verifies `File.Exists`.
  - Blocks disallowed file extensions or suspicious shell-chaining patterns unless explicitly confirmed.

## 3. SQLite & Data Safety
- Every SQL command in `Dockyra.Infrastructure` uses `SqliteParameter` bindings. String interpolation in SQL statements is prohibited.
- JSON backup imports validate `SchemaVersion`, maximum collection bounds, and entity invariants via Domain validators before writing to SQLite.

## 4. Safe Mode & Crash Recovery (`SafeModeStartupCoordinator`)
- On startup, `SafeModeStartupCoordinator` increments an ungraceful startup counter in `%LocalAppData%\Dockyra\safemode.json`.
- Once the main taskbar window initializes cleanly, or on graceful shutdown, the counter resets to `0`.
- If the counter reaches `3` consecutive failed startups:
  1. Dockyra enters **Safe Mode**.
  2. Non-essential modules are disabled.
  3. Heavy visual effects and animations are turned off (`PowerSaver` visual profile).
  4. The user can reset layout/settings to factory defaults with one click.

