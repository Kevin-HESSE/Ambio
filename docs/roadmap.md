# Roadmap

Ambio was built in small, deployable increments.

> **Proof of concept.** Ambio is a proof of concept for an official version built in separate repositories: an API, a SPA frontend, maybe a desktop app ([ADR 0024](adr/0024-proof-of-concept.md)). What it teaches goes into [poc-findings.md](poc-findings.md).

> **The proof of concept ends with Phase 2 (`v0.1.0`).** It is used for a real job search, and the later phases are [left to the official version](#left-to-the-official-version) ([ADR 0029](adr/0029-proof-of-concept-ends-after-phase-2.md)).

> **Guiding principle: usable early.** The app is packaged and deployable right after the first feature (application tracking).

Each phase maps to a GitHub Milestone, and each checkbox maps to an issue.

| Phase | Goal | Outcome |
|---|---|---|
| [0](#phase-0--foundations) | Foundations | Layered solution, CI, template samples removed |
| [1](#phase-1--application-tracking-mvp) | Application tracking (MVP) | Track applications, cover letters, and uploaded CVs |
| [2](#phase-2--docker--deployment) | Docker & deployment | Image built with `docker compose`, all data in one volume, `v0.1.0` |

---

## Phase 0 — Foundations

- [x] Restructure the solution into layers (see [architecture.md](architecture.md)): `Ambio.Domain`, `Ambio.Application`, `Ambio.Infrastructure`, `Ambio.Web`
- [x] Move `ApplicationDbContext` and migrations to `Ambio.Infrastructure`, set up `ApplyConfigurationsFromAssembly`, and register the context with `AddDbContextFactory` ([ADR 0010](adr/0010-dbcontext-factory-without-repositories.md))
- [x] Remove the template samples (`Counter`, `Weather`, `Auth` pages, and their nav links)
- [x] Remove passkey support (pages, endpoints, `PasskeySubmit` component)
- [x] Single-user mode: close registration once an account exists ([ADR 0002](adr/0002-single-user-application.md))
- [x] Add `.editorconfig` and enable `TreatWarningsAsErrors` for the new projects
- [x] Git hooks with Husky.Net: `dotnet format` check on pre-commit, Conventional Commits check on commit-msg ([ADR 0019](adr/0019-git-hooks-with-husky-net.md))
- [x] Create test projects (xUnit, bUnit, Playwright) — see [testing.md](testing.md)
- [x] Apply the strict email validation of registration to every account form (Log in, Forgot password, Resend confirmation, Reset password, Change email)
- [x] GitHub Actions CI: conventions check (formatting, commit messages), then build and tests on every pull request and push to `dev` and `main`, with `dev` and `main` protected by the CI checks ([ADR 0020](adr/0020-ci-and-protected-branches.md))
- [x] Organize every project by feature folders ([ADR 0018](adr/0018-feature-folders-in-every-project.md))
- [x] Add the MIT `LICENSE`

## Phase 1 — Application tracking (MVP)

Each item is a vertical slice: domain, persistence, service, page and tests for one usable feature, delivered in its own pull request. Shared components are built by the first slice that needs them. The model is in [data-model.md](data-model.md) ([ADR 0011](adr/0011-company-centric-model-with-job-offers.md)) and the screens in [ui.md](ui.md) ([ADR 0016](adr/0016-responsive-navigation-bottom-tabs-and-sidebar.md)).

- [x] Design tokens and theme ([design-system.md](design-system.md), [ADR 0015](adr/0015-neutral-visual-identity-with-status-colors.md)): `--ambio-*` variables and their Bootstrap mapping in `app.css`, light/dark theme, self-hosted Source Sans 3 and Bootstrap Icons
- [x] Responsive app shell: sidebar ≥ 992 px, app bar + bottom tab bar + "More" sheet below, logo and favicon, theme selector (System / Light / Dark); each later slice adds its navigation entry
- [x] Companies: create and list
  - `Company` profile (name, kind, website, industry, size, location), and the creation of `tests/Ambio.Domain.Tests`
  - EF Core mapping class and migration, `SaveChangesInterceptor` for `CreatedAt`/`UpdatedAt`
  - Application service returning DTOs, with the mapping written as extension methods
- [x] Companies: detail and edit
- [x] Companies: readable URLs such as `/companies/brightwave`, with a unique slug stored from the name (suffix on collision, recomputed on rename)
- [x] Offers: create and list, `JobOffer` with a plain-text copy of the ad and its first `JobOfferPosting`, offers shown on the company page
- [x] Refactor: shared list styles (responsive list, list table, tag row) and test fixtures built with Bogus, before the next lists and detail pages
- [x] Offers: detail with their postings (the same offer seen on several sites), edit
- [x] Offers: duplicate suggestion when creating an offer (same company, similar title)
- [x] Applications from an offer: create and list, abstract `JobApplication` and `OfferApplication` mapped as TPH, `Channel` (online form, email, referral…), `ShortId` generated with NanoId (numeric for now), `StatusBadge` and `Tag` components
- [x] Spontaneous applications: `SpontaneousApplication`, TPH check constraint, "From an offer / Spontaneous" toggle
- [x] Application detail and edit, with readable URLs `/applications/482913-backend-developer-acme` (`ShortId` + slug) and a redirect to the canonical slug ([ADR 0014](adr/0014-short-id-and-slug-urls.md))
- [x] Offers: readable URLs `/offers/482913-backend-developer-kelvio` (`ShortId` + slug) with a redirect to the canonical slug ([ADR 0025](adr/0025-short-id-and-slug-urls-for-offers.md))
- [x] Status change and history: `Draft → Applied → Interview → Offer / Rejected / Withdrawn`, every change recorded as a timestamped `StatusChange`, and an end-to-end journey from a company to a status change
- [x] Applications list: filters by status, company and kind, search, sort by date
- [x] Archiving instead of deleting ([ADR 0012](adr/0012-archive-instead-of-delete.md)) for companies, offers and applications, with an "Archived" filter, an Undo toast and a restore action
- [x] **Cover letters**: plain text, any number per application, textarea editor, copy to clipboard
- [x] **CV upload**: attach the PDF that was actually sent (`CvDocument` with `Source = Uploaded`), stored through `IDocumentStorage` under `/data/documents`
  - PDF only, size limit configurable
  - Download and in-browser preview
  - Keeps a record of every CV sent until generation lands in Phase 3
- [x] Settings: follow-up delay (`UserSettings`), theme
- [x] `Home` dashboard: counts by status, **Follow up** (applications still Applied after the configured delay), recent activity
- [x] Restyled Log in and Account pages
- [x] Create the account at first start, before the app is usable, instead of leaving registration open to whoever comes first ([ADR 0028](adr/0028-account-created-at-first-start.md))

## Phase 2 — Docker & deployment

- [x] Multi-stage `Dockerfile` (SDK build → ASP.NET runtime), non-root user
- [x] Volumes: `/data` (SQLite database, documents, DataProtection keys)
- [x] EF Core migrations applied at startup
- [x] Forwarded headers enabled for running behind a reverse proxy
- [x] `compose.yaml` that builds and runs the image
- [x] Squash the migrations before the first release
- [x] README: features as built and a quick start with Docker
- [x] [deployment.md](deployment.md): volumes, environment variables, update, backup of `/data`

## Left to the official version

The proof of concept stops here ([ADR 0029](adr/0029-proof-of-concept-ends-after-phase-2.md)). These features were planned for it and are left to the official version, as a starting point. Their design is in [data-model.md](data-model.md) and the ADRs.

### Company research & contacts

Enriches the company, which is shared by every application to it.

- `CompanyNote`: dated research notes in plain text, shown as a timeline on the company page
- `Contact`: CRUD per company, optional linking to applications (never required: form applications have no contact)
- Recruiter company on offers (`RecruiterCompanyId`): agencies and ESNs, with a possibly unknown end client
- `Interaction`: interviews, calls, emails, follow-ups per application, with the contacts involved
- Dashboard: upcoming interviews

### Candidate profile & CV generation

- `CandidateProfile` (identity, headline, summary, experiences, education, skills, languages, links), with an optional photo
- CV per application, selection + override ([ADR 0007](adr/0007-cv-snapshots-selection-and-override.md)): choose profile items, override the title and summary for the job offer, then freeze the result as a snapshot
- General CV, with no application, meant for sharing on LinkedIn and other networks
- PDF rendering with a built-in template ([ADR 0004](adr/0004-questpdf-for-cv-generation.md)), with preview and download

### Export & showcase

- JSON export of all data (profile, applications, cover letters, CV metadata)
- Multi-arch image (`linux/amd64`, `linux/arm64`) published by a CD workflow
- Demo data, screenshots
