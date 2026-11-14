# PHASE-03: Application Groups, Folders & Smart Classification

- **Phase ID**: `PHASE-03`
- **Status**: `IN_PROGRESS` (Core Engine Complete; Drag-and-Drop UI Next)
- **Prerequisites**: `PHASE-01`, `PHASE-02`

## 1. Objective
Enable users to organize applications into named, color-coded groups and expandable folder flyouts with instant search, grid/list switching, bounded subfolder depth, and rule-based smart classification.

## 2. Affected Modules
- `MOD-GROUPS`, `MOD-FOLDERS`

## 3. Technical Risks & Mitigations
- **Risk**: Deeply nested folders degrading UX and causing cyclic references.
  - **Mitigation**: `FolderHierarchyRules` enforces `MaxAllowedDepth = 3` and cycle detection.
- **Risk**: Smart auto-classification overwriting user customizations.
  - **Mitigation**: `SmartGroupingRuleEngine` prioritizes user rules (`IsUserDefined = true`) and never mutates existing groups automatically without user confirmation.

## 4. Acceptance Criteria
- [x] Groups and folder items persist in SQLite across application restarts.
- [x] `SmartGroupingRuleEngine` classifies applications deterministically with user rules first.
- [x] `FolderFlyoutControl` supports instant search filtering and Grid/List view toggle.
- [ ] Interactive drag-and-drop visual reordering (`TASK-0008`).

