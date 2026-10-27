# Conflux — Codex Best Practices Starter (v2)

Conflux is a **learning project** for modular monolith, vertical slices, SignalR, Redis, AI and reliable .NET systems.
This starter contains **instructions, tasks, and review templates, not application source code**.

## Getting started
1. Unzip into a new `conflux` directory and open it in Codex.
2. Inspect `AGENTS.md` (persistent repo conventions).
3. Read `docs/ai-development-workflow.md` (human/Codex review cycle).
4. Start with `docs/tasks/001-workspaces-first-slice.md` (only approved feature).
5. Use the **read-only planning prompt** in `docs/prompts.md`; check the proposed architecture.
6. Use the implementation prompt, then run/review tests and the code diff.
7. Record actual evidence in the PR template and write learning notes yourself.

## First milestone
- ASP.NET Core 10 + EF Core PostgreSQL; create and list workspaces via Minimal APIs.
- React + TypeScript frontend, loading/error/empty states.
- Docker Compose PostgreSQL; xUnit and Testcontainers integration tests.
- Not in scope: SignalR, Redis, AI, authentication, microservices.

## Repository guide
- `AGENTS.md`: short operational instructions Codex automatically recognizes.
- `docs/adr/`: decisions and trade-offs; `TEMPLATE.md` for future decisions.
- `docs/tasks/`: feature acceptance contracts.
- `docs/ai-development-workflow.md`: safe, learning-first AI-assisted engineering.
- `docs/prompts.md`: separate planning / coding / reviewing / learning prompts.
- `docs/learning-log.md`: human-owned learning outcomes.
- `.github/pull_request_template.md`: evidence-backed acceptance and review.

## Notes
- Do not include secrets or internal employer code/data in prompts or the repo.
- Keep planned, tested, and verified separate; an AI statement is not proof of passing tests.
- Later, add CI workflow once an actual .NET solution and frontend exist, so the checks are real and reproducible.
