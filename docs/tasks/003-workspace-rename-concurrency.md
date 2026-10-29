# Task 003 — Prevent lost workspace renames

## Goal
Reject a rename based on an outdated workspace version instead of silently overwriting another user's change.

## Scope and contract
- Add a `Version` number to Workspaces, starting at 1. Migrate existing rows to version 1.
- Include `version` in create, list, and rename responses.
- Require `expectedVersion` in `PATCH /api/workspaces/{id}`. A missing or non-positive value returns HTTP 400 validation details.
- A valid rename increments the version by 1. A stale version returns HTTP 409 and leaves the saved name and version unchanged.
- The UI sends the version it displayed when editing began. On conflict, it closes the editor, refreshes the list, and explains that the user must review the current name before trying again.
- Add domain, real PostgreSQL API, and UI tests for success, invalid input, and competing updates.

## Boundaries and trade-off
- Keep persistence and the rename rule in the Workspaces module. Use EF Core's application-managed concurrency token so the expected version is checked in the database update.
- This changes the PATCH request contract. Existing clients must send `expectedVersion`.
- Do not add authentication, locks, retries, a second module, or real-time delivery. A conflict requires an explicit user decision before another rename.

## Acceptance
Two clients read version 1. The first renames the workspace and receives version 2. The second submits version 1 and receives 409; the first name remains saved. The UI displays the current name after the conflict.
