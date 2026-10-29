# Task 002 — Rename a workspace

## Goal
Allow a workspace to be renamed through the existing Workspaces module and React UI.

## Scope
- `PATCH /api/workspaces/{id}` accepts `{ "name": "New name" }` and returns the updated workspace contract with HTTP 200.
- Apply the same trimmed, 3–80 character domain rule used for creation. Invalid input returns a structured HTTP 400 validation problem and leaves the saved name unchanged.
- An unknown workspace ID returns HTTP 404. A malformed ID does not match the route.
- The UI lets the user edit an existing workspace name, shows validation or request errors, and refreshes the list after a successful rename.
- Add domain, PostgreSQL API, and UI tests for success and relevant failures.

## Boundaries
- Keep the use case in `Workspaces/Features/RenameWorkspace`; the Workspaces module owns the rule and persistence.
- Keep API contracts separate from the EF entity.
- Reuse the current database schema and migration. No new table or column is needed.
- Do not add authentication, uniqueness, history, real-time updates, or a new module.

## Concurrency decision
Concurrent valid renames use last-write-wins. Optimistic concurrency needs an additional version contract and migration; add it when a concrete editing conflict requirement exists.

## Acceptance
Create `Engineering`, rename it to `  Platform  `, and see `Platform` in the response and list after reload. A blank or too-long name is rejected without changing the stored value. Renaming an unknown ID returns 404.
