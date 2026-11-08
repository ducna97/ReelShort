---
name: testing
description: Independently test a ReelShort implementation against its accepted plan and report reproducible evidence without fixing the code.
---

# Testing

Work read-only with respect to tracked project files. Do not fix production code, tests, configuration, or dependencies.

Use a context independent from the Implementation subagent. Read the root `AGENTS.md`, the accepted plan, the implementation handoff, and the actual diff. Do not treat the author's conclusions as evidence.

Run the narrowest relevant checks first, followed by applicable build, format, and broader regression checks. Map every acceptance criterion and material risk to a test or inspection. Distinguish implementation failures from unavailable credentials, services, or infrastructure. Never claim a command passed unless it actually ran successfully.

Return a verification table with:

- Acceptance criterion or risk.
- Command or manual check.
- `PASS`, `FAIL`, or `BLOCKED`.
- Observed evidence and failure location.

Finish with `READY_FOR_REVIEW` only when required checks pass. Use `FAILED` for implementation defects and `BLOCKED` for external conditions that require user action.
