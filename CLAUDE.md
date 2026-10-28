# CLAUDE.md

## Project state

Ambio is a proof of concept for an official version built in separate repositories: an API, a SPA, maybe a desktop app ([ADR 0024](docs/adr/0024-proof-of-concept.md)). It ended with Phase 2, released as `v0.1.0` ([ADR 0029](docs/adr/0029-proof-of-concept-ends-after-phase-2.md), `docs/roadmap.md`), and is now used for a real job search: a .NET 10 Blazor Web App with ASP.NET Core Identity, split into layered projects (Domain, Application, Infrastructure, Web).

Every project is organised by feature folders ([ADR 0018](docs/adr/0018-feature-folders-in-every-project.md)).
- Features built so far: `Companies`, `JobOffers` (with their postings), `JobApplications` (to an offer or spontaneous), `CoverLetters`, `CvDocuments` (uploaded CVs), `Settings` and `Dashboard` (the home page).
- Tests live in `tests/` and mirror the project under test: Domain, Application, Infrastructure, Web (bUnit) and E2E (Playwright)

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
# the migrations are applied and the default account of appsettings.Development.json is created at start
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

Docker (http://localhost:8080, data in `./data`, account in `.env` from `.env.example`, see `docs/deployment.md`)

```bash
docker compose up -d --build
```

EF Core migrations

```bash
dotnet ef migrations add <Name> --project src/Ambio.Infrastructure --startup-project src/Ambio.Web --output-dir Persistence/Migrations
dotnet ef database update --project src/Ambio.Infrastructure --startup-project src/Ambio.Web
```

Formatting

```bash
# fix the formatting of the whole solution
dotnet format

# check only, as the pre-commit hook and the CI do
dotnet format --verify-no-changes
```

@docs/git-hooks.md

## Architecture

All main projects are in the `src` folder.

- `src/Ambio.Domain`: entities and domain rules
- `src/Ambio.Application`: service interfaces, DTOs, mapping extensions
- `src/Ambio.Infrastructure`: service implementations, EF Core, storage
- `src/Ambio.Web`: Blazor UI, Identity, composition root

See `docs/architecture.md` for the full dependency rules.

## Documentation

- Roadmap and phases: `docs/roadmap.md` (each phase = GitHub Milestone). Tick checkboxes as tasks land.
- Proof of concept findings: `docs/poc-findings.md`
- Architecture: `docs/architecture.md`
- Data model: `docs/data-model.md`
- Design system (colors, typography, status badges): `docs/design-system.md`
- Screens, navigation and responsive layout: `docs/ui.md` (HTML mockups in `docs/mockups/`)
- Deployment: `docs/deployment.md`
- Tests and test conventions: `docs/testing.md`
- CI/CD: `docs/ci-cd.md`
- Decisions: `docs/adr/` — add an ADR only for a decision that can serve as a basis for the official version (domain, data, business rules); other choices go in `docs/poc-findings.md`. Never rewrite an accepted ADR.
- Docs describe what is built: a planned feature is a roadmap line, its design is written when its slice is built. A slice ends with an entry in `docs/poc-findings.md` and updates the docs it touches.
- Docs are written in English.

## Git

- Branches are named `type/short-name` (`feat/`, `fix/`, `refactor/`, `docs/`, `test/`, `chore/`, `ci/`, `build/`).
- Feature and fix branches merge into `dev` only: their pull requests target `dev`, never `main`. Only `dev` is merged into `main`.
- Never commit without the user's explicit validation: stop at an uncommitted working tree and wait to be asked. A previous "commit" request does not cover later work. Don't end replies with a reminder that nothing is committed.
- Implement multi-step plans one step at a time: build and test the step, then hand over for review (the user may edit the code). Once it is validated, re-read the touched files to pick up those edits and commit that step on its own before starting the next one.
- Commit messages follow Conventional Commits (`type(scope): subject`, e.g. `feat(applications): add status history`), subject line only, no body, no trailing period. Types: `build`, `chore`, `ci`, `docs`, `feat`, `fix`, `perf`, `refactor`, `revert`, `style`, `test`. Required trailers such as `Co-Authored-By` are still appended. The `commit-msg` hook checks the subject; a body is accepted after a blank line, but only the user writes one.
- Merges and reverts of a whole branch only go through pull requests, never locally.
- `dev` and `main` are protected by rulesets: changes only arrive through a pull request; no force push or deletion. Until the proof of concept is finished, the CI only runs for `main`, whose pull requests need its checks: run `dotnet format --verify-no-changes` and `dotnet test` (end-to-end included) before opening a pull request into `dev` (see `docs/ci-cd.md`).
