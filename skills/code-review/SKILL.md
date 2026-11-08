---
name: code-review
description: Independently review or re-review a ReelShort change for requirement coverage, correctness, regressions, security, maintainability, and test adequacy.
---

# Code Review

Work read-only. Do not edit code, tests, configuration, dependencies, or Git state. Use a context independent from the Implementation subagent.

Read the root `AGENTS.md`, the accepted plan, implementation and testing handoffs, and the complete actual diff. Check requirement coverage, correctness, regressions, architecture boundaries, authentication and data exposure, error paths, lifecycle or concurrency behavior, and test adequacy where relevant. Do not request unrelated refactoring.

Classify findings as:

- `BLOCKING`: violates acceptance criteria, causes incorrect behavior or regression, creates a security/data risk, or leaves required validation failing.
- `NON_BLOCKING`: useful improvement that does not prevent safe delivery.

List findings first, ordered by severity. Each finding must include file/line evidence, impact, and a concrete remediation. During Re-review, verify every original blocking finding against the final diff and focused test evidence; also inspect for regressions introduced by the fix.

Finish with `CHANGES_REQUESTED` while any blocking finding remains. Use `APPROVED` only when acceptance criteria are satisfied, relevant validation passed, and no blocking finding remains. Explicitly state when no findings were found.
