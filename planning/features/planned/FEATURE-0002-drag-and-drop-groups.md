# FEATURE-0002: Interactive Drag-and-Drop Group & Folder Reordering

- **Status**: `PLANNED`
- **Modules**: `MOD-GROUPS`, `MOD-FOLDERS`, `MOD-TASKBAR`
- **Related Tasks**: `TASK-0008`

## 1. Purpose
Allow users to drag and drop application icons across groups, reorder group pills on the taskbar, and create folders interactively with visual drop-target adorners and keyboard accessibility alternatives.

## 2. Technical Approach
- Attach WPF drag-and-drop behaviors in `Dockyra.Presentation.Wpf/Behaviors/`.
- Route drop completion commands to `ApplicationGroupService.MoveItemAsync` and `TaskbarOrchestrationService.ReorderSectionAsync`.
- Persist updated sort orders in SQLite.

