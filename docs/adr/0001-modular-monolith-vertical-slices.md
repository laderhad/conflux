# ADR 0001 — Start as Modular Monolith with Vertical Slices

**Status:** Accepted for Sprint 01  
**Date:** 2026-10-27

## Context
Conflux is primarily a learning project for serious software-engineering topics: real-time collaboration, caching, consistency, asynchronous processing and AI. The developer already knows Layered and Clean Architecture. A premature microservice split would increase complexity before business boundaries or operational needs are understood.

## Decision
- Ship one ASP.NET Core API process initially.
- Implement each bounded business capability as an explicit module. Sprint 01 has only `Workspaces`.
- Implement application use cases as vertical slices (`CreateWorkspace`, `GetWorkspaces`).
- Keep module-owned data access in that module (`WorkspacesDbContext`, a `workspaces` database schema).
- Keep domain rules in the module's domain model; avoid artificial generic repository/service layers.
- Separate the React frontend, which communicates via HTTP now and SignalR later.
- Add module contracts and domain/integration events only when an actual second module/use case needs them.

## Consequences
**Benefits:** low operational overhead; testable, understandable feature flow; module boundaries can be improved incrementally.  
**Costs:** explicit discipline is needed to keep module boundaries; process-level isolation is absent; schema separation does not guarantee independence on its own.

## Alternatives considered
- Classic layered controller/service/repository application: familiar, but encourages horizontal changes for each feature and may generate unnecessary forwarding layers.
- Full Clean Architecture project per module: valid, but expensive at this size; revisit if complexity requires it.
- Microservices from day one: rejected due to operational complexity and too little domain evidence.

## Revisit when
More than one business module exists, or actual scaling / team autonomy / deployment constraints emerge.
