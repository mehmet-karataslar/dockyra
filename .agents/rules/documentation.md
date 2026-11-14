---
trigger: always_on
---

# Dockyra Documentation & Planning Governance Rules

1. **Single Source of Truth**:
   - `planning/` is the canonical engineering memory of Dockyra.
   - Keep `planning/CURRENT-STATE.md`, `planning/modules/module-registry.yaml`, and `planning/tasks/` synchronized with every code change.
2. **No Speculative Completion**:
   - Never document planned or partial features as completed in `CURRENT-STATE.md` or `tasks/completed.md`.
3. **Traceable Identifiers**:
   - Every module uses `MOD-<NAME>`, every task uses `TASK-XXXX`, every phase uses `PHASE-XX`, and every architectural decision uses `ADR-XXXX`.
4. **Handoff Readiness**:
   - Major milestones must update `planning/CURRENT-STATE.md` and `planning/handoffs/` so another AI agent can continue seamlessly without chat history.

