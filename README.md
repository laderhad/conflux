# Conflux

Conflux is a learning-focused collaborative workspace. Task 001 provides a first end-to-end Workspaces feature: ASP.NET Core Minimal API, a module-owned PostgreSQL schema, and a React frontend.

## Requirements

- .NET 10 SDK
- Node.js and npm
- Docker with a running daemon
- `dotnet-ef` 10.x for migration commands (`dotnet tool install --global dotnet-ef --version 10.0.9` if needed)

## Run locally

1. Copy `.env.example` to `.env` and set a local PostgreSQL password. `.env` is ignored by Git.
2. Start PostgreSQL:

   ```sh
   docker compose up -d
   ```

3. In a shell at the repository root, load the local password into the shell and configure the API connection:

   ```sh
   set -a
   . ./.env
   set +a
   export ConnectionStrings__Workspaces="Host=localhost;Port=5432;Database=conflux;Username=conflux;Password=${POSTGRES_PASSWORD}"
   ```

4. Apply the Workspaces migration, then run the API:

   ```sh
   dotnet ef database update --project src/Conflux.Modules.Workspaces --startup-project src/Conflux.Api --context WorkspacesDbContext
   dotnet run --project src/Conflux.Api --launch-profile http
   ```

5. In another shell, start the frontend:

   ```sh
   cd frontend
   npm ci
   npm run dev
   ```

Open `http://localhost:5173`. Vite proxies `/api` to the API at `http://localhost:5080`. For a separately hosted frontend, set `VITE_API_BASE_URL` to the API origin when building it. The API allows requests from `http://localhost:5173` during local development.

`GET /health` reports that the API host is running. It does not test PostgreSQL connectivity.

## Verify

With Docker running:

```sh
dotnet test tests/Conflux.IntegrationTests/Conflux.IntegrationTests.csproj
cd frontend && npm run build
```

The integration tests start a disposable PostgreSQL container and apply migrations. To check local persistence manually, create a workspace named `Engineering`, reload the page, then restart Compose with `docker compose down` followed by `docker compose up -d`. The named PostgreSQL volume remains in place. A whitespace-only name should return validation details and leave the list unchanged.

## Structure

- `src/Conflux.Api`: host, OpenAPI setup, CORS, and host health endpoint.
- `src/Conflux.Modules.Workspaces`: Workspaces domain rule, create/list slices, DbContext, and migration in the `workspaces` schema.
- `tests/Conflux.IntegrationTests`: API-level tests against PostgreSQL via Testcontainers.
- `frontend`: React and TypeScript client.
- `docs/tasks/001-workspaces-first-slice.md`: feature acceptance criteria.
- `docs/adr/0001-modular-monolith-vertical-slices.md`: architecture decision.
