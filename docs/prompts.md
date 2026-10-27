# Codex Prompt Templates

Adapt task name and file paths. Do not paste private tokens or production logs.

## A. Read-only design / plan — use for nontrivial tasks

```text
Inspect AGENTS.md, docs/tasks/001-workspaces-first-slice.md, and only the relevant project files and ADRs.
Do not modify any files yet.
Summarize the requirement, affected modules, proposed changes, alternatives/trade-offs,
risks, and test plan. Identify any unnecessary abstractions or future-scope features.
Finish with the precise scope of your proposed patch.
```

## B. Implement the approved slice

```text
Implement ONLY docs/tasks/001-workspaces-first-slice.md using the approved design.
Follow AGENTS.md. Keep changes small and conform to module boundaries.
Add meaningful happy-path and edge-case tests. Run available build and test commands.
Show the changed files, exact commands/results, blockers, and important trade-off.
Do not implement SignalR, Redis, AI, auth, or unrelated refactors in this task.
Finish with 3 architecture questions for me.
```

## C. Independent review — no edits

```text
Review the current git diff against AGENTS.md and docs/tasks/001-workspaces-first-slice.md.
Do not edit files. Find correctness, concurrency, security, persistence, contract, and
module-boundary problems. Report actionable findings with severity, file path,
line reference, reproduction scenario, and suggested remedy.
Do not invent issues to fill a quota. State missing or unrun tests clearly.
```

## D. Learning / architecture interview

```text
Teach me the changes in this pull request using concrete code paths.
Explain request -> use case -> domain -> persistence -> response, one alternative
architecture and its trade-offs, and two failure scenarios.
Quiz me with 3 questions, then assess my answers without flattery.
```

## E. Fix review findings

```text
Fix only the confirmed high-priority review findings. Explain the root cause,
add regression tests, run focused checks, and show what changed.
Do not refactor unrelated code. Do not claim any test passed unless you ran it.
```
