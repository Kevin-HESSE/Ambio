# Roadmap

Postulo is built in small, deployable increments.

> **Guiding principle: usable early.** The app is packaged and deployable right after the first feature (application tracking). A demo is set up in the same phase, and every later phase adds its new feature to the demo.

Each phase maps to a GitHub Milestone, and each checkbox maps to an issue.

| Phase | Goal | Outcome |
|---|---|---|
| [0](#phase-0--foundations) | Foundations | Layered solution, CI, template samples removed |
| [1](#phase-1--application-tracking-mvp) | Application tracking (MVP) | Track applications, cover letters, and uploaded CVs |
| [2](#phase-2--docker-deployment--demo) | Docker, deployment & demo | Multi-arch image on Docker Hub, running demo |
| [3](#phase-3--candidate-profile--cv-generation) | Profile & CV generation | CVs generated with QuestPDF (per application + general CV) |
| [4](#phase-4--cv-template-plugins) | CV template plugins | Third-party templates loaded from `/plugins` |
| [5](#phase-5--identity--i18n) | Identity & i18n | Real email, GitHub login, FR/EN UI |
| [6](#phase-6--data--showcase) | Data & showcase | JSON export/import, screenshots, final README |

---

## Phase 0 — Foundations

- [ ] Restructure the solution into layers (see [architecture.md](architecture.md)): `Postulo.Domain`, `Postulo.Application`, `Postulo.Infrastructure`, `Postulo.Web`
- [ ] Move `ApplicationDbContext` and migrations to `Postulo.Infrastructure`, set up `ApplyConfigurationsFromAssembly`, and register the context with `AddDbContextFactory` ([ADR 0010](adr/0010-dbcontext-factory-without-repositories.md))
- [ ] Remove the template samples (`Counter`, `Weather`, `Auth` pages, and their nav links)
- [ ] Remove passkey support (pages, endpoints, `PasskeySubmit` component)
- [ ] Single-user mode: close registration once an account exists ([ADR 0002](adr/0002-single-user-application.md))
- [ ] Add `.editorconfig` and enable `TreatWarningsAsErrors` for the new projects
- [ ] Create test projects (xUnit, bUnit) — see [testing.md](testing.md)
- [ ] GitHub Actions CI: restore, build, test on every push and pull request
- [x] Add the MIT `LICENSE`

## Phase 1 — Application tracking (MVP)

- [ ] Domain: `Company`, `JobApplication`, `ApplicationStatus`, `StatusChange` (history)
- [ ] Status workflow: `Draft → Applied → Interview → Offer / Rejected / Withdrawn`, with every change timestamped
- [ ] EF Core mapping classes and migration
- [ ] Application services returning DTOs, with the mapping written as extension methods
- [ ] Pages: list of applications (filters by status/company, sort by date), detail, create/edit
- [ ] `Home` dashboard: counts by status, recent activity
- [ ] **Cover letters**: plain text, 1..n per application, textarea editor, copy to clipboard
- [ ] **CV upload**: attach the PDF that was actually sent (`CvDocument` with `Source = Uploaded`), stored through `IDocumentStorage` under `/data/documents`
  - PDF only, size limit configurable
  - Download and in-browser preview
  - Keeps a record of every CV sent until generation lands in Phase 3
- [ ] Unit + EF integration tests for the tracking feature

## Phase 2 — Docker, deployment & demo

- [ ] Multi-stage `Dockerfile` (SDK build → ASP.NET runtime), non-root user
- [ ] Volumes: `/data` (SQLite database, documents, DataProtection keys) and `/plugins` (prepared, empty for now)
- [ ] EF Core migrations applied at startup
- [ ] Forwarded headers enabled for running behind a reverse proxy
- [ ] `docker-compose.yml` example
- [ ] GitHub Actions: multi-arch build (`linux/amd64`, `linux/arm64`) with buildx, pushed to Docker Hub on version tags
- [ ] **Demo**: `DemoDataSeeder` enabled with `Postulo__Demo__Enabled=true`, which seeds a demo account and sample companies, applications, cover letters, and an uploaded CV
- [ ] README: quick start with Docker and a demo section
- [ ] [deployment.md](deployment.md): volumes, environment variables, backup of `/data`

## Phase 3 — Candidate profile & CV generation

- [ ] Domain: `CandidateProfile` (identity, headline, summary, experiences, education, skills, languages, links)
- [ ] Optional profile photo stored in `/data/photos` and passed to templates (templates may ignore it)
- [ ] **CV per application — selection + override** ([ADR 0007](adr/0007-cv-snapshots-selection-and-override.md)): choose profile items, override the title and summary for the job offer, then freeze the result as a snapshot
- [ ] **General CV**: `CvDocument` with no application (`JobApplicationId = null`), meant for sharing on LinkedIn and other networks, with a "My CVs" page
- [ ] QuestPDF rendering (`Source = Generated`), stored under `/data/documents`, with preview and download
- [ ] Built-in template `Postulo.Plugins.CV.Default`
- [ ] Linux fonts bundled in the Docker image
- [ ] `tools/Postulo.CvPlayground`: console app using the QuestPDF Companion to design templates by hand
- [ ] PDF snapshot tests for the default template
- [ ] Extend demo: seeded profile, a generated general CV, and a generated CV per demo application

## Phase 4 — CV template plugins

- [ ] `Postulo.Plugins.CV.Abstractions`: `ICvTemplate`, `CvModel` (see [plugins.md](plugins.md))
- [ ] Plugin loader: scans `/plugins/<Name>/` at startup, one `AssemblyLoadContext` per plugin, sharing the contract and QuestPDF with the host
- [ ] Faulty-plugin isolation: a plugin that fails to load or render is logged and disabled, and the app keeps running
- [ ] Template picker when generating a CV, and a template list page (name, version, source)
- [ ] Sample external plugin (for example `Postulo.Plugins.CV.Modern`) built and dropped into `/plugins` in CI
- [ ] PDF snapshot tests run against every bundled template
- [ ] Extend demo: second template available in the demo image

## Phase 5 — Identity & i18n

- [ ] MailKit SMTP email sender configured through environment variables, with the no-op sender kept when SMTP is not configured
- [ ] Remove the no-op branch in `RegisterConfirmation.razor` when a real sender is configured
- [ ] Keep 2FA (authenticator app) and recovery codes
- [ ] GitHub OAuth login: can only be **linked** to the existing account, never used to create a second account
- [ ] FR/EN UI localization with `IStringLocalizer` and a language switcher
- [ ] Extend demo: language switcher visible in the demo

## Phase 6 — Data & showcase

- [ ] JSON export of all data (profile, applications, cover letters, CV metadata) and import from the UI
- [ ] Screenshots in `docs/screenshots/` and embedded in the README
- [ ] Final README polish (badges, feature tour, demo link)
- [ ] Extend demo: downloadable sample export
