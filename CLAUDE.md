# CLAUDE.md

## Project state

Ambio is at the "fresh template" stage: a .NET 10 Blazor Web App (`dotnet new blazor --auth Individual`) with ASP.NET Core Identity, split into layered projects (Domain, Application, Infrastructure, Web). The Domain project is still empty. Application holds `IUserService`, `ServiceResult`, `UserErrors` and the `RegisterInput` DTO; Infrastructure holds the Identity `ApplicationDbContext`, its migrations, and `UserService` with its singleton `SemaphoreContainer` (registered by `AddApplicationServices()`). Tests live in `tests/`: Application, Infrastructure, Web (bUnit) and E2E (Playwright); `Ambio.Domain.Tests` comes in Phase 1 (see `docs/testing.md`).

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

# every test, end-to-end included
dotnet test

# fast loop: everything but the end-to-end tests
dotnet test -- --filter-not-trait "Category=E2E"

# end-to-end tests only (Chromium is installed on the first run)
dotnet test --project tests/Ambio.E2E.Tests
```

EF Core migrations

```bash
dotnet ef migrations add <Name> --project src/Ambio.Infrastructure --startup-project src/Ambio.Web --output-dir Persistence/Migrations
dotnet ef database update --project src/Ambio.Infrastructure --startup-project src/Ambio.Web
```

No linters are configured yet.

## Architecture

All main projects are in the `src` folder.

- `src/Ambio.Domain`: entities and domain rules, references nothing
- `src/Ambio.Application`: service interfaces, DTOs, mapping extensions; references Domain
- `src/Ambio.Infrastructure`: service implementations, EF Core, storage; references Application and Domain
- `src/Ambio.Web`: Blazor UI (Interactive Server, no web assembly), Identity, composition root; references Application and Infrastructure

See `docs/architecture.md` for the full dependency rules.

### Routing

- `Components/Routes.razor` uses `AuthorizeRouteView`; unauthenticated access to `[Authorize]` pages redirects via `RedirectToLogin`. 
- `IdentityRevalidatingAuthenticationStateProvider` periodically revalidates the security stamp for interactive circuits. `MapAdditionalIdentityEndpoints()` (in `Components/Account/IdentityComponentsEndpointRouteBuilderExtensions.cs`) adds the non-Razor endpoints (logout, external login, personal data download).

### Identity
- `ApplicationUser : IdentityUser` in `Ambio.Infrastructure/Persistence` (add profile fields there, then add a migration). `RequireConfirmedAccount = true`, Identity schema version 3 (its passkey table is unused: passkey pages and endpoints were removed, see ADR 0002). Email is a no-op (`IdentityNoOpEmailSender`); `RegisterConfirmation.razor` special-cases it to show the confirmation link on screen — remove that branch when a real sender is added.
- Single account (ADR 0002, ADR 0017): registration is open only while no user exists, and account creation goes through `IUserService.RegisterUserAsync`, guarded by an in-process semaphore. `ExternalLogin.razor` has its `@page` commented out until the GitHub OAuth issue, which must create accounts through `IUserService` too.

### Static assets

- Served via `MapStaticAssets()` and referenced through `@Assets["..."]` in `App.razor`. 
- Bootstrap is vendored in `wwwroot/lib`.

### Data

- SQLite, connection string `DefaultConnection` in `appsettings.json`
- `ApplicationDbContext` and migrations live in `src/Ambio.Infrastructure/Persistence` (`Persistence/Migrations`), registered with `AddDbContextFactory` by `AddDatabase()` in `InfrastructureExtensions.cs`
## Documentation

- Roadmap and phases: `docs/roadmap.md` (each phase = GitHub Milestone). Tick checkboxes as tasks land.
- Target architecture: `docs/architecture.md`; data model: `docs/data-model.md`; design system (colors, typography, status badges): `docs/design-system.md`; screens, navigation and responsive layout: `docs/ui.md` (HTML mockups in `docs/mockups/`); plugins: `docs/plugins.md`; deployment: `docs/deployment.md`; tests and test conventions: `docs/testing.md`.
- Decisions: `docs/adr/` — add a new ADR for any significant decision, never rewrite an accepted one.
- Docs are written in English.

## Architecture rules

- Domain entities have no reference to the database: no EF Core package, no data annotations, no `DbContext`.
- Table configuration only through `IEntityTypeConfiguration<T>` classes in `Ambio.Infrastructure/Persistence/Configurations/`.
- Data access: `IDbContextFactory<ApplicationDbContext>` with one short-lived context per operation (Microsoft's Blazor Server guidance). No repositories, no unit of work — keep it simple.
- Services return DTOs (records), never entities. Mapping is written by hand as `ToDto()` extension methods in a `<Feature>MappingExtensions` class. No mapping library.
- Plugin projects are named `Ambio.Plugins.<Domain>.<Purpose>` (e.g. `Ambio.Plugins.CV.Abstractions`).
