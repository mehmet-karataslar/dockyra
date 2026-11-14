# PHASE-04: Fluent UI System, Themes, Settings & Localization

- **Phase ID**: `PHASE-04`
- **Status**: `READY`
- **Prerequisites**: `PHASE-02`, `PHASE-03`

## 1. Objective
Refine the Windows 11 Fluent visual language, complete Dark/Light theme switching, expand the Settings window to all 12 configuration sections, and verify complete `tr-TR` and `en-US` localization and keyboard accessibility.

## 2. Affected Modules
- `MOD-THEMES`, `MOD-SETTINGS`, `MOD-TASKBAR`

## 3. Acceptance Criteria
- [x] Centralized `DesignTokens/` (`Colors`, `Spacing`, `Radius`, `Typography`) and `FluentDarkTheme` / `FluentLightTheme` dictionaries exist.
- [x] `Strings.tr-TR.xaml` and `Strings.en-US.xaml` provide localized UI strings with zero missing keys.
- [ ] Full 12-section `SettingsWindow` expansion (`TASK-0009`).

