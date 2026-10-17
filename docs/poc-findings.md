# Proof of concept findings

Ambio is a proof of concept for an official version built in separate repositories: an API, a SPA frontend, maybe a desktop app ([ADR 0024](adr/0024-proof-of-concept.md)). This file records what it teaches.

- Write down what real use shows: a need confirmed, a feature never used, something missing or awkward.
- Every slice ends with an entry in the [slice log](#slice-log).
- A finding is a fact or an observation, not a decision: the official version decides.

## Basis for the official version

These ADRs are worth reading again when the official version is designed. They are a basis, not decisions to copy.

| ADR | Topic | Note |
|---|---|---|
| [0002](adr/0002-single-user-application.md) | Single user | To reconsider if the API serves several people or devices |
| [0005](adr/0005-sqlite-and-filesystem-storage.md) | SQLite and files on disk | |
| [0006](adr/0006-persistence-ignorant-domain-and-dtos.md) | Persistence-ignorant domain, DTOs | The DTOs are a starting point for the API contracts |
| [0011](adr/0011-company-centric-model-with-job-offers.md) | Company-centric model, application hierarchy | |
| [0012](adr/0012-archive-instead-of-delete.md) | Archive instead of delete | |
| [0013](adr/0013-guid-v7-primary-keys.md) | GUID v7 keys | Lets the official version import the export without remapping ids |
| [0014](adr/0014-short-id-and-slug-urls.md) | Short id and slug URLs | |
| [0015](adr/0015-neutral-visual-identity-with-status-colors.md) | Visual identity and status colors | |
| [0017](adr/0017-account-creation-local-or-github.md) | One account, created locally | The GitHub part is dropped ([ADR 0024](adr/0024-proof-of-concept.md)) |
| [0021](adr/0021-slug-urls-for-companies.md) | Slug URLs for companies | |

## Specific to this proof of concept

These choices follow from Blazor Server and won't apply as they are to an API and a SPA.

- [ADR 0010](adr/0010-dbcontext-factory-without-repositories.md): a `DbContext` factory with one context per operation, as Blazor Server circuits need. An API usually has one scoped context per request.
- [ADR 0016](adr/0016-responsive-navigation-bottom-tabs-and-sidebar.md) and [ADR 0023](adr/0023-interactive-islands-in-static-pages.md): the navigation and the interactive islands are built with Blazor components. The navigation pattern itself can be reused.
- The Identity pages scaffolded from the template: a SPA and a desktop app need another way to authenticate.
- The external-login plumbing left by the template stays dormant, since GitHub login is out of the proof of concept.

## Open questions for the official version

- What replaces the CV template plugins, if anything.
- One way to authenticate for both the SPA and the desktop app.
- Whether the UI is translated.

## Slice log

Newest first. Each entry follows this format:

```markdown
### <date> — <slice> (#<issue>)

- Kept: what proved useful
- Not needed: what was built and turned out useless
- Change in the official version: what to do differently
```
