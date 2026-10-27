# AI-Assisted Engineering Workflow — Conflux

## Principle
Codex is an implementation and review assistant. The human developer remains responsible for product scope, module boundaries, decisions, acceptance, and merges. Aim to learn from every change.

## 0. Branch and scope
- One feature or fix per branch (`feat/001-create-workspaces`, `fix/...`).
- Create a task spec containing: problem, scope, non-goals, acceptance criteria, error cases, test approach.
- Avoid stacking unrelated changes in a single Codex session or PR.

## 1. Design (human-owned)
- For significant tasks, ask Codex for **read-only** alternatives, expected file changes and trade-offs.
- Select the simplest design meeting the acceptance criteria. Important boundary or data consistency decisions should be recorded in a short ADR.
- Get an explicit human decision before implementing a contentious architecture change.

## 2. Implementation (Codex-assisted)
- Give Codex one scoped task with links to existing files and concrete testable acceptance criteria.
- Permit local code edits, build and tests for that task. Do not authorize unrelated features or deployments.
- Request targeted tests along with the implementation; don't generate tests solely to match current behavior.

## 3. Verification (observed evidence)
- Run API/unit/integration tests where relevant, frontend typecheck/build, and repo lint/format checks.
- For database features, verify real PostgreSQL using Testcontainers or equivalent container-based tests.
- For real-time and distributed tasks later, test disconnect/reconnect, duplicate delivery, and dependency outages.
- Record exact commands and pass/fail/skipped results in the PR description. Missing environment dependencies are blockers, not green results.

## 4. Independent code review (human + Codex)
- Review the diff; ask a fresh review pass to identify only actionable issues with file/line, impact and severity.
- Examine contract compatibility, boundaries, validation, authorization, secrets, data races, logging, retries and migrations (when applicable).
- A review agent is a second opinion, not a replacement for human review or CI.

## 5. Learning checkpoint (human-owned)
Answer briefly in `docs/learning-log.md`:
1. What changed in the request → domain → persistence → UI path?
2. What concrete problem does this design solve? What alternate design was rejected?
3. What happens on failure, duplication, or concurrency?
4. Could I implement a small variation without Codex?

## 6. Merge / next task
- Do not merge until acceptance criteria, meaningful tests, diff review and manual smoke check pass (or missing checks are explicitly assessed).
- Merge and start the next task only after human approval.

## Definition of Done
Correct end-to-end behavior; focused tests; no critical unresolved defects; clean diff; exact verification report; updated docs for public behavior; human understands the key trade-off.

## Guidance for later stages
- SignalR: verify missed-event resync; a successful connection isn't proof of consistency.
- Redis: document key ownership, TTL, invalidation, source of truth, duplicate processing.
- AI: protect data access per user/workspace; validate tool calls; redact secrets; evaluate outputs with repeatable test cases and budgets.
