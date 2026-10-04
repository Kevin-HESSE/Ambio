# CLAUDE.md

## Project state

Ambio is at the "fresh template" stage: a .NET 10 Blazor Web App (`dotnet new blazor --auth Individual`) with ASP.NET Core Identity, split into layered projects (Domain, Application, Infrastructure, Web).

Every project is organised by feature folders ([ADR 0018](docs/adr/0018-feature-folders-in-every-project.md)).
- Domain project is still empty.
- Tests live in `tests/` and mirror the project under test: Application, Infrastructure, Web (bUnit) and E2E (Playwright)
- `Ambio.Domain.Tests` comes in Phase 1 (see `docs/testing.md`).

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
- Target architecture: `docs/architecture.md`
- Data model: `docs/data-model.md`
- Design system (colors, typography, status badges): `docs/design-system.md`
- Screens, navigation and responsive layout: `docs/ui.md` (HTML mockups in `docs/mockups/`)
- Plugins: `docs/plugins.md`
- Deployment: `docs/deployment.md`
- Tests and test conventions: `docs/testing.md`
- CI/CD: `docs/ci-cd.md`
- Decisions: `docs/adr/` — add a new ADR for any significant decision, never rewrite an accepted one.
- Docs are written in English.

## Git

- Branches are named `type/short-name` (`feat/`, `fix/`, `refactor/`, `docs/`, `test/`, `chore/`, `ci/`, `build/`).
- Feature and fix branches merge into `dev` only: their pull requests target `dev`, never `main`. Only `dev` is merged into `main`.
- Never commit without the user's explicit validation: stop at an uncommitted working tree and wait to be asked. A previous "commit" request does not cover later work. Don't end replies with a reminder that nothing is committed.
- Implement multi-step plans one step at a time: build and test the step, then hand over for review (the user may edit the code). Once it is validated, re-read the touched files to pick up those edits and commit that step on its own before starting the next one.
- Commit messages follow Conventional Commits (`type(scope): subject`, e.g. `feat(applications): add status history`), subject line only, no body, no trailing period. Types: `build`, `chore`, `ci`, `docs`, `feat`, `fix`, `perf`, `refactor`, `revert`, `style`, `test`. Required trailers such as `Co-Authored-By` are still appended. The `commit-msg` hook checks the subject; a body is accepted after a blank line, but only the user writes one.
- Merges and reverts of a whole branch only go through pull requests, never locally.
- `dev` and `main` are protected by a ruleset: changes only arrive through a pull request whose CI checks pass; no force push or deletion (see `docs/ci-cd.md`).
