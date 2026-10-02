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

- `src/Ambio.Domain`: entities and domain rules
- `src/Ambio.Application`: service interfaces, DTOs, mapping extensions
- `src/Ambio.Infrastructure`: service implementations, EF Core, storage
- `src/Ambio.Web`: Blazor UI, Identity, composition root

See `docs/architecture.md` for the full dependency rules.

## Documentation

- Roadmap and phases: `docs/roadmap.md` (each phase = GitHub Milestone). Tick checkboxes as tasks land.
- Target architecture: `docs/architecture.md`; data model: `docs/data-model.md`; design system (colors, typography, status badges): `docs/design-system.md`; screens, navigation and responsive layout: `docs/ui.md` (HTML mockups in `docs/mockups/`); plugins: `docs/plugins.md`; deployment: `docs/deployment.md`; tests and test conventions: `docs/testing.md`.
- Decisions: `docs/adr/` — add a new ADR for any significant decision, never rewrite an accepted one.
- Docs are written in English.
