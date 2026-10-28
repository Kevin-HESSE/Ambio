# Ambio

**Track your job applications, and keep every CV and cover letter you sent.**

*Ambio* comes from the Latin *ambire*: to go around canvassing, to seek an office.

[![CI](https://github.com/Kevin-HESSE/Ambio/actions/workflows/ci.yml/badge.svg)](https://github.com/Kevin-HESSE/Ambio/actions/workflows/ci.yml)
![.NET 10](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet)
![Blazor](https://img.shields.io/badge/Blazor-Interactive%20Server-512BD4?logo=blazor)
![SQLite](https://img.shields.io/badge/SQLite-EF%20Core-003B57?logo=sqlite)
![Docker](https://img.shields.io/badge/Docker-compose-2496ED?logo=docker)
![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)

> 🧪 **Proof of concept, ended with Phase 2 (`v0.1.0`).** Ambio is used for a real job search, and what it teaches goes into the [findings](docs/poc-findings.md). The official version is built in another repository ([ADR 0024](docs/adr/0024-proof-of-concept.md), [ADR 0029](docs/adr/0029-proof-of-concept-ends-after-phase-2.md)); see the [roadmap](docs/roadmap.md) for what was built and what is left to it.

## Features

- **Companies**: a profile per company (kind, website, industry, size, location), shared by every offer and application to it, at a readable URL such as `/companies/brightwave`.
- **Job offers**: a plain-text copy of the ad, so it survives when the ad goes offline, and every site it was posted on. A new offer similar to an existing one is flagged as a possible duplicate.
- **Applications**: to an offer or spontaneous, with the channel used. A status workflow (Draft → Applied → Interview → Offer / Rejected / Withdrawn) records every change in a history. The list filters by status, company and kind, with a search.
- **Cover letters**: any number of plain-text letters per application, copied to the clipboard in one click.
- **CVs**: the PDF actually sent with each application, kept with a preview and a download.
- **Dashboard**: counts by status, the applications to follow up after a delay you set, and the recent activity.
- **Archive instead of delete**: companies, offers and applications are archived with their related rows, with an Undo and a restore.
- **Self-hosted and private**: one account, created at first start, with 2FA; all data in one folder.

## Quick start

### Docker

Requires Docker with Compose.

```bash
git clone https://github.com/Kevin-HESSE/Ambio.git
cd Ambio
cp .env.example .env    # set Ambio__Account__Email and Ambio__Account__Password
docker compose up -d --build
```

Open <http://localhost:8080> and log in with the account from `.env`, created at the first start. Then remove the password from `.env`: once the account exists, it is ignored. The data lives in `./data`.

See [deployment](docs/deployment.md) for the variables, a reverse proxy, updates and backups.

### From source

Requires the [.NET 10 SDK](https://dotnet.microsoft.com/download).

```bash
dotnet run --project src/Ambio.Web
```

The migrations are applied at start, and a new local database gets the development account of `src/Ambio.Web/appsettings.Development.json`.

The first restore also installs the git hooks ([Husky.Net](https://alirezanet.github.io/Husky.Net/)), which check the formatting and the commit messages.

## Documentation

- [Roadmap](docs/roadmap.md)
- [Proof of concept findings](docs/poc-findings.md)
- [Architecture](docs/architecture.md)
- [Data model](docs/data-model.md)
- [Design system](docs/design-system.md)
- [UI and screens](docs/ui.md)
- [Deployment](docs/deployment.md)
- [Testing](docs/testing.md)
- [Architecture Decision Records](docs/adr/README.md)
- [Git hooks](docs/git-hooks.md)
- [CI/CD](docs/ci-cd.md)

## Tech stack

- .NET 10
- Blazor Web App (Interactive Server)
- ASP.NET Core Identity
- EF Core + SQLite
- xUnit, bUnit, NSubstitute, Bogus, Playwright
- Docker
- GitHub Actions

## License

Ambio is released under the [MIT License](LICENSE).
