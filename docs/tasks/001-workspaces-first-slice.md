# Task 001 — First end-to-end Workspaces vertical slice

## Goal
Bootstrap Conflux and deliver the first production-shaped feature from PostgreSQL to React. **No SignalR, Redis, AI, authentication or background workers in this task.**

## Tech baseline
- .NET 10 SDK (LTS)
- ASP.NET Core Minimal API
- EF Core / Npgsql / PostgreSQL (database schema `workspaces`)
- React + TypeScript + Vite
- Docker Compose for local PostgreSQL
- xUnit + Testcontainers.PostgreSql integration tests

## Backend project boundaries
- `src/Conflux.Api/` — host, composition root, health endpoint, OpenAPI setup
- `src/Conflux.Modules.Workspaces/` — feature slices, domain, module-owned DbContext and DI registration
- `tests/Conflux.IntegrationTests/` — real PostgreSQL API-level integration tests

No generic repository. No unnecessary MediatR or custom abstraction framework. The main flow should be easy to follow in code.

## Data model
Workspace:
- `Id` (UUID)
- `Name` (required, trimmed, 3–80 chars)
- `CreatedAtUtc` (UTC timestamp)

Keep uniqueness and ownership rules out for now: identities have not been introduced yet.

## API behavior
1. `POST /api/workspaces`
   - Body: `{ "name": "Engineering" }`
   - Valid name: returns HTTP 201 with `{ "id", "name", "createdAtUtc" }`.
   - Missing, blank, less than 3, or greater than 80 characters after trimming: HTTP 400 with structured validation details.
   - Save to PostgreSQL, not in-memory collections.
2. `GET /api/workspaces`
   - Returns HTTP 200 with stable JSON array, sorted most recently created first (use ID as tie-breaker).
   - Empty database returns `[]`.
3. `GET /health`
   - Returns healthy response when host is running. Distinguish host health from DB connectivity if needed; don't misrepresent one as the other.

## Frontend behavior
- Workspace creation form.
- Workspace list with loading, empty, success and error states.
- Refresh list after successful creation.
- Responsive but minimal UI; no speculative real-time state management.

## Testing / verification
- Build backend and frontend.
- Run migrations against local Compose PostgreSQL; restart services and verify data persists.
- Integration tests against Testcontainers.PostgreSql: create/read, trim rule, invalid name, empty list.
- Run tests and report actual output. If Docker/Node/.NET unavailable, give precise unblock instructions and DO NOT claim tests passed.
- Document local setup in README, including environment configuration and commands.

## Acceptance demonstration
- Open frontend, create a workspace named `Engineering`, see it in the list.
- Reload page; workspace still exists.
- Reject whitespace-only name.
- Tests pass against PostgreSQL.

## Architecture review questions
1. Why is a WorkspacesDbContext in the Workspaces module rather than a shared global DbContext?
2. What does a vertical slice contain, and what duplication are we willing to tolerate?
3. Why aren't Redis and SignalR needed yet, and what concrete feature will justify them?
