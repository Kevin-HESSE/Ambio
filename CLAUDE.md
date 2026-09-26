# CLAUDE.md

## Project state

Postulo is at the "fresh template" stage: a .NET 10 Blazor Web App (`dotnet new blazor --auth Individual`) with ASP.NET Core Identity. `Counter`, `Weather`, and `Auth` pages are template samples; there is no domain code or test project yet.

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
dotnet run --project src/Postulo.Web

# https://localhost:7009
dotnet run --project src/Postulo.Web -lp https

# hot reload
dotnet watch --project src/Postulo.Web
```

EF Core migrations

```bash
dotnet ef migrations add <Name> --project src/Postulo.Web --output-dir Data/Migrations
dotnet ef database update --project src/Postulo.Web
```

There are no tests or linters configured yet.

## Architecture

All main projects are in the `src` folder.

- `src/Postulo.Web`, Interactive Server (no web assembly)

### Routing

- `Components/Routes.razor` uses `AuthorizeRouteView`; unauthenticated access to `[Authorize]` pages redirects via `RedirectToLogin`. 
- `IdentityRevalidatingAuthenticationStateProvider` periodically revalidates the security stamp for interactive circuits. `MapAdditionalIdentityEndpoints()` (in `Components/Account/IdentityComponentsEndpointRouteBuilderExtensions.cs`) adds the non-Razor endpoints (logout, external login, passkeys, personal data download).

### Identity
- `ApplicationUser : IdentityUser` (add profile fields there, then add a migration). `RequireConfirmedAccount = true`, Identity schema version 3 (includes passkeys). Email is a no-op (`IdentityNoOpEmailSender`); `RegisterConfirmation.razor` special-cases it to show the confirmation link on screen — remove that branch when a real sender is added.

### Static assets

- Served via `MapStaticAssets()` and referenced through `@Assets["..."]` in `App.razor`. 
- Bootstrap is vendored in `wwwroot/lib`.

### Data

- SQLite, connection string `DefaultConnection` in `appsettings.json`
- Migrations live in `Data/Migrations`