---
paths: src/Ambio.Web/**
---

# Web rules

`Ambio.Web` references Application and Infrastructure.

- Blazor Interactive Server only, no WebAssembly. Pages are static; only the smallest component that needs interactivity gets `@rendermode InteractiveServer`, set by its page, with serializable parameters and its buttons disabled until `RendererInfo.IsInteractive` ([ADR 0023](../../docs/adr/0023-interactive-islands-in-static-pages.md)).
- Components consume Application interfaces and DTOs only, never `ApplicationDbContext` or domain entities. The template Identity pages in `Components/Account` are the only exception.
- Code is organised by feature ([ADR 0018](../../docs/adr/0018-feature-folders-in-every-project.md)): `Components/<Feature>/Pages/` for routable pages and `Components/<Feature>/Shared/` for the feature's components, like the existing `Components/Account/` (the Identity feature). Cross-feature code goes in `Components/Layout/`, `Components/Shared/`, and `Components/Pages/` only for app-level pages (Home, Error, NotFound).

## Routing and Identity

- `Components/Routes.razor` uses `AuthorizeRouteView`; unauthenticated access to `[Authorize]` pages redirects via `RedirectToLogin`.
- `IdentityRevalidatingAuthenticationStateProvider` periodically revalidates the security stamp for interactive circuits. `MapAdditionalIdentityEndpoints()` (in `Components/Account/IdentityComponentsEndpointRouteBuilderExtensions.cs`) adds the non-Razor endpoints (logout, external login, personal data download).
- Account creation goes only through `IUserService.RegisterUserAsync`; registration is open only while no user exists (ADR 0002, ADR 0017).
- `ExternalLogin.razor` has its `@page` commented out: GitHub login is out of the proof of concept (ADR 0024).
- Email is a no-op (`IdentityNoOpEmailSender`); `RegisterConfirmation.razor` special-cases it to show the confirmation link on screen. There is no real sender in the proof of concept (ADR 0024).

## UI

- Colors, typography and spacing come from the `--ambio-*` CSS variables mapped onto Bootstrap. No hard-coded colors in components ([docs/design-system.md](../../docs/design-system.md), [ADR 0015](../../docs/adr/0015-neutral-visual-identity-with-status-colors.md)).
- Size text fields in lines with the `lh` unit (`min-height: 6lh` for a textarea), not in pixels, so they follow the font and its line height.
- Layout: bottom tab bar + "More" sheet below 992 px, sidebar above. Lists are tables on desktop and cards on mobile; detail pages put their tab in the URL (`?tab=`) ([docs/ui.md](../../docs/ui.md), mockups in `docs/mockups/`, [ADR 0016](../../docs/adr/0016-responsive-navigation-bottom-tabs-and-sidebar.md)).
- The shell is rendered statically; its behaviors (sheets, theme) are in `wwwroot/js/shell.js` and the `<head>` script of `App.razor`. A new page adds its section to `Components/Layout/AppNavigation.cs` and sets its mobile title with `<AppBarTitle>`; a page below a list (detail, new, edit) adds `<AppBarBack>` towards its parent.
- Each feature lists its base-relative URLs in `Components/<Feature>/<Feature>Routes.cs` (constants, plus builders such as `CompanyRoutes.Details(slug)`). Links, redirects, tests and `AppNavigation` use it; pages keep their `@page` template.
- Company URLs are `/companies/{slug}`: resolve by the stored slug; a new literal route under `/companies/` adds its segment to the reserved slugs of `CompanySlugs` ([ADR 0021](../../docs/adr/0021-slug-urls-for-companies.md)).
- Application and offer URLs are `/applications/{ShortId}-{slug}` and `/offers/{ShortId}-{slug}`, the key built and read by `Components/Shared/ShortIdKey.cs`: resolve by `ShortId`, redirect to the canonical slug ([ADR 0014](../../docs/adr/0014-short-id-and-slug-urls.md), [ADR 0025](../../docs/adr/0025-short-id-and-slug-urls-for-offers.md)).
- Static assets are served by `MapStaticAssets()` and referenced through `@Assets["..."]` in `App.razor`; Bootstrap is vendored in `wwwroot/lib`.
