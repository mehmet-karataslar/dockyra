# Module Specification: Fluent Design Tokens, Themes & Localization (`MOD-THEMES`)

```yaml
id: MOD-THEMES
name: Fluent Design Tokens, Themes & Localization
status: in-progress
purpose: Centralizes Windows 11 Fluent design tokens, Dark/Light themes, and tr-TR/en-US localization resources.
source_paths:
  - src/Dockyra.Modules/Themes/
  - src/Dockyra.Presentation.Wpf/DesignTokens/
  - src/Dockyra.Presentation.Wpf/Localization/
dependencies:
  - MOD-CORE
contracts:
  - IThemeService
  - ILocalizationService
tests:
  - tests/UITests/
owners:
  - wpf-developer
related_tasks:
  - TASK-0007
  - TASK-0009
```

## Responsibilities
- Define centralized XAML token dictionaries (`Colors.xaml`, `Spacing.xaml`, `Radius.xaml`, `Typography.xaml`).
- Support runtime switching between `FluentDarkTheme.xaml` and `FluentLightTheme.xaml`.
- Provide complete localization keys in `Strings.tr-TR.xaml` and `Strings.en-US.xaml` with runtime language switching.

