# CLAUDE.md

## Project state

Ambio is at the "fresh template" stage: a .NET 10 Blazor Web App (`dotnet new blazor --auth Individual`) with ASP.NET Core Identity, split into layered projects (Domain, Application, Infrastructure, Web). `Counter`, `Weather`, and `Auth` pages are template samples; the Domain, Application and Infrastructure projects are still empty and there is no test project yet.

## Stack

- .NET 10
- Blazor Web App with bootstrap
- ASP.NET Core Identity
- SQLite with EF Core

## Commands

Run from the repo root :

```bash
# build the solution
dotnet build

# run (http profile: http://localhost:5160)
dotnet run --project src/Ambio.Web

# https://localhost:7009
dotnet run --project src/Ambio.Web -lp https

# hot reload
dotnet watch --project src/Ambio.Web
```

EF Core migrations

```bash
dotnet ef migrations add <Name> --project src/Ambio.Infrastructure --startup-project src/Ambio.Web --output-dir Persistence/Migrations
dotnet ef database update --project src/Ambio.Infrastructure --startup-project src/Ambio.Web
```

There are no tests or linters configured yet.

## Architecture

All main projects are in the `src` folder.

- `src/Ambio.Domain`: entities and domain rules, references nothing
- `src/Ambio.Application`: service interfaces, DTOs, mapping extensions; references Domain
- `src/Ambio.Infrastructure`: service implementations, EF Core, storage; references Application and Domain
- `src/Ambio.Web`: Blazor UI (Interactive Server, no web assembly), Identity, composition root; references Application and Infrastructure

See `docs/architecture.md` for the full dependency rules.

### Routing

- `Components/Routes.razor` uses `AuthorizeRouteView`; unauthenticated access to `[Authorize]` pages redirects via `RedirectToLogin`. 
- `IdentityRevalidatingAuthenticationStateProvider` periodically revalidates the security stamp for interactive circuits. `MapAdditionalIdentityEndpoints()` (in `Components/Account/IdentityComponentsEndpointRouteBuilderExtensions.cs`) adds the non-Razor endpoints (logout, external login, passkeys, personal data download).

### Identity
- `ApplicationUser : IdentityUser` in `Ambio.Infrastructure/Persistence` (add profile fields there, then add a migration). `RequireConfirmedAccount = true`, Identity schema version 3 (includes passkeys). Email is a no-op (`IdentityNoOpEmailSender`); `RegisterConfirmation.razor` special-cases it to show the confirmation link on screen — remove that branch when a real sender is added.

### Static assets

- Served via `MapStaticAssets()` and referenced through `@Assets["..."]` in `App.razor`. 
- Bootstrap is vendored in `wwwroot/lib`.

### Data

- SQLite, connection string `DefaultConnection` in `appsettings.json`
- `ApplicationDbContext` and migrations live in `src/Ambio.Infrastructure/Persistence` (`Persistence/Migrations`), registered with `AddDbContextFactory` by `AddDatabase()` in `InfrastructureExtensions.cs`
## Documentation

- Roadmap and phases: `docs/roadmap.md` (each phase = GitHub Milestone). Tick checkboxes as tasks land.
- Target architecture: `docs/architecture.md`; data model: `docs/data-model.md`; design system (colors, typography, status badges): `docs/design-system.md`; screens, navigation and responsive layout: `docs/ui.md` (HTML mockups in `docs/mockups/`); plugins: `docs/plugins.md`; deployment: `docs/deployment.md`; tests: `docs/testing.md`.
- Decisions: `docs/adr/` — add a new ADR for any significant decision, never rewrite an accepted one.
- Docs are written in English.

## Architecture rules

- Domain entities have no reference to the database: no EF Core package, no data annotations, no `DbContext`.
- Table configuration only through `IEntityTypeConfiguration<T>` classes in `Ambio.Infrastructure/Persistence/Configurations/`.
- Data access: `IDbContextFactory<ApplicationDbContext>` with one short-lived context per operation (Microsoft's Blazor Server guidance). No repositories, no unit of work — keep it simple.
- Services return DTOs (records), never entities. Mapping is written by hand as `ToDto()` extension methods in a `<Feature>MappingExtensions` class. No mapping library.
- Plugin projects are named `Ambio.Plugins.<Domain>.<Purpose>` (e.g. `Ambio.Plugins.CV.Abstractions`).
