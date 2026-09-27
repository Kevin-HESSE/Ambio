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
| `/plugins` | CV template plugins (see [plugins.md](plugins.md)) |

## Environment variables

| Variable | Purpose | Default |
|---|---|---|
| `ConnectionStrings__DefaultConnection` | SQLite connection string | `Data Source=/data/ambio.db` |
| `Ambio__Storage__Root` | Root folder for documents and photos | `/data` |
| `Ambio__Plugins__Path` | Plugin folder | `/plugins` |
| `Ambio__Demo__Enabled` | Seed a demo account and data | `false` |
| `Smtp__Host`, `Smtp__Port`, `Smtp__Username`, `Smtp__Password`, `Smtp__From` | Email sending; no email sent if unset | — |
| `Authentication__GitHub__ClientId`, `Authentication__GitHub__ClientSecret` | GitHub login, disabled if unset | — |

## docker compose

```yaml
services:
  ambio:
    image: <dockerhub-user>/ambio:latest
    restart: unless-stopped
    ports:
      - "8080:8080"
    environment:
      - Smtp__Host=smtp.example.com
      - Smtp__Port=587
      - Smtp__Username=me@example.com
      - Smtp__Password=${SMTP_PASSWORD}
      - Smtp__From=me@example.com
    volumes:
      - ./data:/data
      - ./plugins:/plugins
```

## Reverse proxy

Run Ambio behind Caddy, Traefik, Nginx or your NAS's proxy.

- Forwarded headers (`X-Forwarded-For`, `X-Forwarded-Proto`) are honoured so that redirects and OAuth callbacks use `https`.
- Blazor Server needs **WebSockets**: make sure the proxy forwards the `Upgrade` and `Connection` headers.
- The GitHub OAuth callback URL is `https://<your-domain>/signin-github`.

## First run

1. Start the container.
2. Open the app and register: the first account becomes the only account and registration closes ([ADR 0002](adr/0002-single-user-application.md)).
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

From [Phase 6](roadmap.md#phase-6--data--showcase), a JSON export/import is also available from the UI.
