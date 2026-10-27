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
    volumes:
      - ./data:/data
```

## Reverse proxy

Run Ambio behind Caddy, Traefik, Nginx or your NAS's proxy.

- Forwarded headers (`X-Forwarded-For`, `X-Forwarded-Proto`) are honoured so that redirects use `https`.
- Blazor Server needs **WebSockets**: make sure the proxy forwards the `Upgrade` and `Connection` headers.

## First run

> Run **one instance per database**. The guard that closes registration is in-process: two containers sharing the same SQLite file could each create an account ([ADR 0017](adr/0017-account-creation-local-or-github.md)).

1. Start the container.
2. Open the app and register with an email and a password: the first account becomes the only account and registration closes ([ADR 0002](adr/0002-single-user-application.md), [ADR 0017](adr/0017-account-creation-local-or-github.md)). Until then, anyone who reaches the instance can register, so do it before exposing the app.
3. Enable 2FA from **Account → Two-factor authentication**.

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
