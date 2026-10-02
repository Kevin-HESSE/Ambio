---
paths: src/Ambio.Infrastructure/**
---

# Infrastructure rules

## Layout

Code is organised by feature, like Application ([ADR 0018](../../docs/adr/0018-feature-folders-in-every-project.md)):

- `<Feature>/`: the service, the EF Core configurations of the feature's entities and its other types, e.g. `Users/UserService.cs`, `Users/ApplicationUser.cs`, later `Companies/CompanyService.cs` and `Companies/CompanyConfiguration.cs`.
- `Common/`: cross-feature services, e.g. `SemaphoreContainer`.
- `Persistence/`: only what is shared by every feature: `ApplicationDbContext`, `Migrations/`, interceptors.
- `InfrastructureExtensions.cs` stays at the project root.
- Namespaces mirror folders.

## Services

- Infrastructure implements the Application interfaces. Implementations are `internal`, live in their feature folder, and are registered in `AddApplicationServices()`; the database and Identity are registered in `AddDatabase()` (both in `InfrastructureExtensions.cs`).
- Expected failures return `ServiceResult.Failure(<Feature>Errors.X)`. Unexpected exceptions are logged with `ILogger`, then mapped to a failure.
- `ShortId` is generated with NanoId by the creating service, which retries while the value is taken ([ADR 0014](../../docs/adr/0014-short-id-and-slug-urls.md)).

## Data access

- Inject `IDbContextFactory<ApplicationDbContext>` and create one short-lived context per operation, then call `SaveChangesAsync` once at the end of a write ([ADR 0010](../../docs/adr/0010-dbcontext-factory-without-repositories.md)):

  ```csharp
  await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);
  ```

- No repositories, no unit of work: services use the `DbContext` directly.
- Reads are LINQ projections to DTOs (so EF translates them to SQL) with `AsNoTracking()`.
- Archived rows are filtered out with an explicit `Where`. No global query filter ([ADR 0012](../../docs/adr/0012-archive-instead-of-delete.md)).

## EF Core mapping

- One `<Feature>/<Entity>Configuration.cs` per entity, implementing `IEntityTypeConfiguration<T>` and picked up by `ApplyConfigurationsFromAssembly`. Never configure tables in the domain or in `OnModelCreating`.
- Keys: `Property(e => e.Id).ValueGeneratedNever()` (the domain creates GUID v7 keys).
- Enums stored as strings: `HasConversion<string>()`.
- Many-to-many configured with `UsingEntity`.
- Foreign keys between `Company`, `Contact`, `JobOffer` and `JobApplication` use `Restrict`; rows owned by a parent are deleted in `Cascade`.
- `CreatedAt` / `UpdatedAt` are set by a `SaveChangesInterceptor`.
- Migrations live in `Persistence/Migrations`, created with the `dotnet ef migrations add` command from `CLAUDE.md`.

See [docs/data-model.md](../../docs/data-model.md#conventions).

## Identity

- `ApplicationUser` lives in `Users/`; Identity tables keep their `string` keys.
- Single account: accounts are only created through `IUserService.RegisterUserAsync`, guarded by the `Common/SemaphoreContainer` semaphore (ADR 0002, ADR 0017).
