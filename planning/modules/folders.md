# Module Specification: Folder Flyouts & Hierarchy (`MOD-FOLDERS`)

```yaml
id: MOD-FOLDERS
name: Folder Flyouts & Hierarchy
status: in-progress
purpose: Provides expandable flyout panels for application groups with search, grid/list toggle, and bounded nested folders.
source_paths:
  - src/Dockyra.Domain/Entities/FolderItem.cs
  - src/Dockyra.Domain/Rules/FolderHierarchyRules.cs
  - src/Dockyra.Modules/Folders/
  - src/Dockyra.Presentation.Wpf/ViewModels/FolderPopupViewModel.cs
  - src/Dockyra.Presentation.Wpf/Views/FolderFlyoutControl.xaml
dependencies:
  - MOD-GROUPS
  - MOD-THEMES
contracts:
  - IFolderService
  - IApplicationLauncher
tests:
  - tests/UnitTests/
  - tests/UITests/
owners:
  - wpf-developer
  - backend-developer
related_tasks:
  - TASK-0004
  - TASK-0008
```

## Responsibilities
- Enforce subfolder hierarchy constraints via `FolderHierarchyRules` (`MaxAllowedDepth = 3`, cycle detection).
- Filter folder items instantaneously by display name or executable name inside `FolderPopupViewModel`.
- Support switching between `Grid` and `List` presentation modes.
- Launch applications safely via `IApplicationLauncher`.

