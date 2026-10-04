# 0006 — Persistence-ignorant domain, EF mapping classes, DTOs

- Status: Accepted
- Date: 2026-09-26

## Context

The template puts entities and the `DbContext` in the web project. As the domain grows, mixing persistence concerns with business rules and exposing entities directly to Blazor components couples the layers together.

## Decision

- **Domain entities have no reference to the database**: no EF Core package, no data annotations (`[Key]`, `[MaxLength]`…), no `DbContext`.
- **Table configuration uses EF Core mapping classes**: one `IEntityTypeConfiguration<T>` per entity in `Ambio.Infrastructure/Persistence/Configurations/`, applied with `ApplyConfigurationsFromAssembly`.
- **Services return DTOs** (records). Blazor components never receive domain entities.
- **Mapping is written by hand** as C# extension methods (`ToDto()`) in one `<Feature>MappingExtensions` class per feature, so the mapping is centralized. No mapping library.

## Consequences

- The domain can be tested without any infrastructure.
- Persistence details are in one place per entity.
- Some boilerplate for DTOs and mappings, which is explicit and easy to debug.
- Services use a short-lived `DbContext` from `IDbContextFactory`, without repositories or a unit of work ([ADR 0010](0010-dbcontext-factory-without-repositories.md)).
