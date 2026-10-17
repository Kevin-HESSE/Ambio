# 0024 — Ambio becomes a proof of concept

- Status: Accepted
- Date: 2026-10-17
- Supersedes: [ADR 0003](0003-runtime-plugin-loading-at-startup.md); in part [ADR 0001](0001-record-architecture-decisions.md) (which decisions get an ADR), [ADR 0002](0002-single-user-application.md) and [ADR 0017](0017-account-creation-local-or-github.md) (the GitHub login), [ADR 0004](0004-questpdf-for-cv-generation.md) (the plugin rationale)

## Context

Ambio was planned as a finished self-hosted product: a Blazor app with CV template plugins, GitHub login, a French and English UI, and JSON export and import. The official version will instead be built in separate repositories: an API, a SPA frontend (one repository each), and maybe a desktop app.

This repository grew fast, mostly written with an AI assistant, and became hard to follow. Its most useful role is now to show what is really needed before the official version is designed.

## Decision

- **Ambio is a proof of concept.** What it teaches (needs confirmed by use, features that turn out useless, things to change) is recorded in [poc-findings.md](../poc-findings.md).
- **The ADRs are a basis for the official version.** It draws on them without necessarily taking them over entirely.
- **The plugin system is dropped.** CV templates are built into `Ambio.Infrastructure`: no `Ambio.Plugins.*` project, no `/plugins` volume, no runtime loading. How templates are extended is decided in the official version.
- **Scope.**
  - Kept: Phase 1 as it is, Docker and the demo, company research and contacts, the candidate profile and CV generation with a built-in QuestPDF template.
  - The last phase becomes **Export & showcase**: a JSON export of all data, meant as the migration format to the official version, the screenshots and the final README.
  - Dropped: CV template plugins, the Identity & i18n phase (SMTP email, GitHub login, French and English UI), and the JSON import.
- **A lighter process.**
  - An ADR is written only for a decision that can serve as a basis for the official version (domain, data, business rules). Choices specific to this proof of concept or to Blazor go in the findings.
  - The docs describe what is built. A planned feature is a roadmap line; its detailed design is written when its slice is built.
  - Component tests (bUnit) only cover components with logic: interactive islands, state, conditional rendering. A page that only displays DTOs has none. The other test projects and the end-to-end journeys don't change.

## Consequences

- QuestPDF stays for CV generation ([ADR 0004](0004-questpdf-for-cv-generation.md)). The `TemplateId` and `TemplateVersion` of a generated CV ([ADR 0007](0007-cv-snapshots-selection-and-override.md)) name a built-in template.
- The single account is created locally only ([ADR 0017](0017-account-creation-local-or-github.md)), still through `IUserService`. The external-login plumbing left by the template stays dormant.
- Email confirmation keeps the no-op sender, which shows the link on screen.
- GUID v7 keys ([ADR 0013](0013-guid-v7-primary-keys.md)) still pay off: the export keeps ids unique for the official version.
- [ADR 0010](0010-dbcontext-factory-without-repositories.md), [ADR 0016](0016-responsive-navigation-bottom-tabs-and-sidebar.md) and [ADR 0023](0023-interactive-islands-in-static-pages.md) are specific to Blazor and won't apply as they are to an API and a SPA.
- The existing tests are kept; the lighter test rule applies to new work.
