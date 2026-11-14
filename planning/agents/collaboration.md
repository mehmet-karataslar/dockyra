# Multi-Agent Collaboration Rules (`planning/agents/collaboration.md`)

1. **Right-Sized Delegation**:
   - Do not spawn all 9 agents for a single small fix; a single agent can complete a localized bugfix or enhancement following the relevant skill.
2. **Sequential vs. Parallel Work**:
   - Execute dependent layers sequentially: `Contracts` / `Domain` -> `Application` -> `Infrastructure` & `WindowsIntegration` & `Presentation.Wpf` (parallelizable once contracts are fixed) -> `Tests` & `Documentation`.
3. **File Ownership**:
   - When running multiple agents in parallel, assign disjoint file sets (`source_paths`) to prevent conflicting edits.

