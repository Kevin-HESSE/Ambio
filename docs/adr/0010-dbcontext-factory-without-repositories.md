# 0010 — DbContext factory, without repositories or unit of work

- Status: Accepted
- Date: 2026-09-26

## Context

In Blazor Server, a DI scope lives as long as the user's circuit, which can be hours. A scoped `DbContext` would then be shared by every component and event handler of the circuit. `DbContext` isn't thread-safe, and its change tracker would keep growing and serve stale data. Microsoft's guidance for Blazor Server is to create a short-lived context per operation with `IDbContextFactory<TContext>` ([ASP.NET Core Blazor with EF Core](https://learn.microsoft.com/aspnet/core/blazor/blazor-ef-core)).

The Repository and Unit of Work patterns were considered on top of that. `DbContext` already is a unit of work and `DbSet<T>` already is a repository. Wrapping them adds layers without adding value to an app this size.

## Decision

- Register the context with `AddDbContextFactory<ApplicationDbContext>()`.
- Every service operation creates and disposes its own context:

  ```csharp
  await using var db = await dbFactory.CreateDbContextAsync(ct);
  ```

- **No repositories, no unit of work.** Services use the `DbContext` directly, project queries to DTOs, and call `SaveChangesAsync` once at the end of each write operation.
- Application defines service interfaces and DTOs. Their implementations live in `Postulo.Infrastructure`, which owns EF Core.
- ASP.NET Core Identity keeps its scoped `ApplicationDbContext`: since EF Core 6, `AddDbContextFactory` also registers the context as scoped, so the Identity stores keep working.

## Consequences

- No context shared across a circuit, and no concurrency errors when two components load data at the same time.
- Each operation starts with an empty change tracker, so reads are always fresh. Read queries use `AsNoTracking()` or DTO projections.
- Fewer abstractions: services are easy to read and debug.
- Tests create the factory over an in-memory SQLite connection (see [testing.md](../testing.md)).
