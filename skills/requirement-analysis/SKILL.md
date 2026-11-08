---
name: requirement-analysis
description: Analyze a ReelShort change request, resolve material ambiguity, choose a solution, and create an implementation-ready plan before code is modified.
---

# Requirement Analysis

Work only on requirements and planning. Do not modify application code, tests, dependencies, configuration, or Git state.

Read the root `AGENTS.md`, the user's request, and enough relevant source to ground the analysis. Separate explicit requirements from assumptions. Ask for clarification only when the answer materially changes behavior, security, data, public contracts, or scope.

Create or update `plans/<task-slug>.md` with:

- Problem statement and desired outcome.
- In-scope and out-of-scope behavior.
- Observable acceptance criteria and important edge cases.
- Relevant current behavior and repository constraints.
- Selected solution and meaningful trade-offs.
- Ordered, file-specific implementation steps.
- Test cases and exact validation commands.
- Risks, assumptions, blockers, and rollback or compatibility concerns when relevant.

Keep the plan small enough to implement and verify as one coherent task. Do not include unrelated refactoring. Finish with `READY` only when the Implementation subagent can proceed without making a material product or architecture decision; otherwise return `NEEDS_INPUT` with the blocking question.
