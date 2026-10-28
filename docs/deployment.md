# Deployment

Ambio ships as a Docker image published on Docker Hub for `linux/amd64` and `linux/arm64`, so it runs on a VPS as well as on an ARM NAS or a Raspberry Pi.

> Status: planned for [Phase 2](roadmap.md#phase-2--docker-deployment--demo). Image name, tags and variables below are the target design.

## Image

- Built by GitHub Actions with `docker buildx` on every version tag (`v*`).
- Tags: `latest`, `<major>.<minor>.<patch>`, `<major>.<minor>`.
- Runs as a non-root user and listens on port `8080` (HTTP). TLS is handled by the reverse proxy.
- EF Core migrations are applied at startup.

## Volumes

| Path | Content |
|---|---|
| `/data` | SQLite database, CV files, profile photo, DataProtection keys |

## Environment variables

| Variable | Purpose | Default |
|---|---|---|
| `ConnectionStrings__DefaultConnection` | SQLite connection string | `Data Source=/data/ambio.db` |
| `Ambio__Account__Email`, `Ambio__Account__Password` | The single account, created at first start; required while no account exists, ignored afterwards ([ADR 0028](adr/0028-account-created-at-first-start.md)) | — |
| `Ambio__Storage__Root` | Root folder for documents and photos; a relative path starts from the app's folder | `/data` |
| `Ambio__Storage__MaxUploadSizeInMegabytes` | Largest CV PDF accepted on upload, in megabytes | `5` |
| `Ambio__Demo__Enabled` | Seed a demo account and data | `false` |
| `Ambio__ShortId__Alphabet`, `Ambio__ShortId__Size` | Alphabet and length of the short ids in application URLs: lowercase, never `-` (see [data-model.md](data-model.md#application-hierarchy)) | `0123456789`, `6` |

## docker compose

```yaml
services:
  ambio:
    image: <dockerhub-user>/ambio:latest
    restart: unless-stopped
    ports:
      - "8080:8080"
    environment:
      Ambio__Account__Email: you@example.com
      Ambio__Account__Password: "<a strong password; write $ as $$>"
    volumes:
      - ./data:/data
```

## Reverse proxy

Run Ambio behind Caddy, Traefik, Nginx or your NAS's proxy.

- Forwarded headers (`X-Forwarded-For`, `X-Forwarded-Proto`) are honoured so that redirects use `https`.
- Blazor Server needs **WebSockets**: make sure the proxy forwards the `Upgrade` and `Connection` headers.

## First run

> Run **one instance per database**. The guard around the account creation is in-process: two containers sharing the same SQLite file could each create an account ([ADR 0017](adr/0017-account-creation-local-or-github.md), [ADR 0028](adr/0028-account-created-at-first-start.md)).

1. Set `Ambio__Account__Email` and `Ambio__Account__Password`. The password must have at least 6 characters, with an uppercase letter, a lowercase letter, a digit and a symbol.
2. Start the container. While no account exists, Ambio creates it from these variables before accepting requests, with its email already confirmed. Without them, or when they are refused, it stops and its log names the problem. There is no registration page: every page asks to log in.
3. Log in, then remove `Ambio__Account__Password` from the configuration: once the account exists, the variables are ignored. Change the password from **Account → Password**.
4. Enable 2FA from **Account → Two-factor authentication**.

In development, `appsettings.Development.json` sets a default account, so a new local database gets one at its first start. That file is never loaded in Production.

Deleting the account from **Account → Personal data** leaves the instance without an account: its next start creates it again from the variables, or refuses to start without them.

## Demo

Set `Ambio__Demo__Enabled=true` to seed a demo account with sample companies, job offers, applications, contacts, cover letters and CVs. The demo is extended in every phase of the [roadmap](roadmap.md).

## Backup and restore

All state lives in `/data`.

```bash
# backup
docker compose stop ambio
tar czf ambio-backup-$(date +%F).tar.gz ./data
docker compose start ambio

# restore
docker compose stop ambio
tar xzf ambio-backup-YYYY-MM-DD.tar.gz
docker compose start ambio
```

Keep `/data/keys` in the backup: without it, existing cookies and 2FA tokens become invalid.

From [Phase 4](roadmap.md#phase-4--export--showcase), a JSON export is also available from the UI.
