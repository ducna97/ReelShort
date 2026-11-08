---
name: context-handoff
description: Create or resume a durable ReelShort task snapshot when context usage approaches 65-70%, before compaction, or when work must continue in another session.
---

# Context Handoff

Preserve enough verified state for another agent or session to continue immediately without repeating completed work or guessing decisions.

## When to create a snapshot

Create or refresh a snapshot when any of these conditions applies:

- The available context indicator reaches roughly 65-70% usage.
- Context compaction is imminent, even if an exact percentage is unavailable.
- A long-running task is about to change agents or sessions.
- The user asks to pause, hand off, or continue later.

Do not wait for context exhaustion. If the runtime does not expose an exact usage percentage, use the earliest reliable warning and create the snapshot before loading another large artifact or tool result.

## Snapshot location

Store the active task snapshot at `handoffs/<task-slug>.md`. Create `handoffs/` only when the first snapshot is needed. Update the same file for the same task instead of creating timestamped duplicates.

Never include secrets, credentials, access tokens, private keys, or sensitive request/response bodies.

## Required snapshot content

Write concise, evidence-based Markdown with these sections:

1. `Task`: the user's current goal and exact scope.
2. `Current state`: completed workflow stage, work in progress, and known repository state.
3. `Decisions`: decisions already made, their rationale, and contracts that must remain stable.
4. `Changed files`: files intentionally added or modified and what changed in each.
5. `Validation`: commands already run, results, and checks not yet run.
6. `Remaining work`: ordered next actions with clear completion criteria.
7. `Risks and blockers`: unresolved findings, assumptions, external dependencies, and required user input.
8. `Resume instructions`: the first files to read, the next command or agent stage, and any actions that must not be repeated.

Record exact paths, commands, error messages, and identifiers when they affect continuation. Distinguish verified facts from assumptions. Note unrelated dirty-worktree changes so the next session preserves them.

## Creating a handoff

Before writing, inspect only the minimum state required to make the snapshot accurate: the active plan, focused diff or status, relevant agent results, and latest validation output. Do not rerun expensive work solely for the snapshot.

Write or refresh the snapshot atomically where practical, then tell the user which file contains it. Creating a snapshot is a checkpoint, not a reason to stop: continue the task when sufficient context remains and no session transition was requested.

## Resuming from a handoff

At the start of a continued task:

1. Read the applicable root `AGENTS.md` and the referenced handoff file.
2. Verify the snapshot against lightweight current state such as `git status` and the focused files.
3. Preserve completed decisions unless current evidence or a new user instruction supersedes them.
4. Continue from `Remaining work`; do not repeat completed implementation or validation without a concrete reason.
5. Refresh the snapshot again when the next handoff threshold is reached, and remove it only when the task is Done or the user explicitly asks to discard it.

If the snapshot conflicts with the repository or the user's latest instruction, treat the current repository and latest instruction as authoritative and update the snapshot.
