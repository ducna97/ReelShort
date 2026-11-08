---
name: implementation
description: Implement an accepted ReelShort task plan or fix verified test and review findings with the smallest correct code change.
---

# Implementation

Read the root `AGENTS.md` and the accepted file under `plans/` before editing. Implement only the approved scope and preserve unrelated user changes in the shared worktree.

Follow existing architecture, naming, error handling, and frontend language conventions. Add focused regression tests when an applicable test harness exists. Never add secrets or edit generated/dependency directories. Do not commit, push, deploy, or broaden scope.

For a Fix pass, address only verified failures or blocking review findings. Explain with evidence when a finding cannot be reproduced or conflicts with the accepted requirement. Run focused checks for changed behavior, but never approve your own work.

Return:

- Files changed and why.
- Acceptance criteria or findings addressed.
- Any plan deviation and its justification.
- Commands run with observed results.
- Remaining limitations or checks deferred to Testing.

Finish with `READY_FOR_TEST` after initial implementation or `READY_FOR_RE_REVIEW` after fixes.
