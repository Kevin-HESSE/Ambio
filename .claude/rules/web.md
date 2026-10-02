---
paths: src/Ambio.Web/**
---

# Web rules

- Blazor Interactive Server only, no WebAssembly.
- Components consume Application interfaces and DTOs only, never `ApplicationDbContext` or domain entities. The template Identity pages in `Components/Account` are the only exception.
- Code is organised by feature ([ADR 0018](../../docs/adr/0018-feature-folders-in-every-project.md)): `Components/<Feature>/Pages/` for routable pages and `Components/<Feature>/Shared/` for the feature's components, like the existing `Components/Account/` (the Identity feature). Cross-feature code goes in `Components/Layout/`, `Components/Shared/`, and `Components/Pages/` only for app-level pages (Home, Error, NotFound).
- Account creation goes only through `IUserService.RegisterUserAsync`; registration is open only while no user exists (ADR 0002, ADR 0017).

## UI

- Colors, typography and spacing come from the `--ambio-*` CSS variables mapped onto Bootstrap. No hard-coded colors in components ([docs/design-system.md](../../docs/design-system.md), [ADR 0015](../../docs/adr/0015-neutral-visual-identity-with-status-colors.md)).
- Layout: bottom tab bar + "More" sheet below 992 px, sidebar above. Lists are tables on desktop and cards on mobile; detail pages put their tab in the URL (`?tab=`) ([docs/ui.md](../../docs/ui.md), mockups in `docs/mockups/`, [ADR 0016](../../docs/adr/0016-responsive-navigation-bottom-tabs-and-sidebar.md)).
- Application URLs are `/applications/{ShortId}-{slug}`: resolve by `ShortId`, redirect to the canonical slug ([ADR 0014](../../docs/adr/0014-short-id-and-slug-urls.md)).
- Static assets are served by `MapStaticAssets()` and referenced through `@Assets["..."]`; Bootstrap is vendored in `wwwroot/lib`.
