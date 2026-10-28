# Deployment

Ambio runs as a Docker container, built from this repository with `docker compose`. All its state lives in one folder, mounted at `/data`.

## Image

The `Dockerfile` at the root builds the image in two stages:

- The .NET 10 SDK restores and publishes `src/Ambio.Web` for the target architecture. It runs on the build machine's platform and cross-compiles with `-a $TARGETARCH`, so a `linux/arm64` image builds without emulation. `HUSKY=0` skips the git hooks, since the build has no `.git`.
- The ASP.NET Core runtime image runs the app as the non-root user `app` (uid `1654`), on HTTP port `8080`. TLS is handled by the reverse proxy.

At start, the app applies the EF Core migrations the database hasn't got yet, then creates the account while none exists ([First run](#first-run)).

## Volumes

| Path | Content |
|---|---|
| `/data` | `ambio.db` (SQLite database, with its `-wal` and `-shm` files while it runs), `documents/` (uploaded CVs), `keys/` (ASP.NET Core DataProtection keys) |

`compose.yaml` mounts `./data`, next to the clone; git and the Docker build ignore it. On Linux, the folder must belong to the container's user, otherwise the app can't create its database:

```bash
mkdir -p data && sudo chown 1654:1654 data
```

Docker Desktop (macOS, Windows) needs no change.

## Environment variables

| Variable | Purpose | Default |
|---|---|---|
| `Ambio__Account__Email`, `Ambio__Account__Password` | The single account, created at first start; required while no account exists, ignored afterwards ([ADR 0028](adr/0028-account-created-at-first-start.md)) | — |
| `Ambio__Storage__Root` | Root folder for the documents and the DataProtection keys; a relative path starts from the app's folder | `/data` (set by the image) |
| `Ambio__Storage__MaxUploadSizeInMegabytes` | Largest CV PDF accepted on upload, in megabytes | `5` |
| `Ambio__ShortId__Alphabet`, `Ambio__ShortId__Size` | Alphabet and length of the short ids in application and offer URLs: lowercase, never `-` (see [data-model.md](data-model.md#application-hierarchy)) | `0123456789`, `6` |
| `ConnectionStrings__DefaultConnection` | SQLite connection string | `Data Source=/data/ambio.db` (set by the image) |
| `ASPNETCORE_FORWARDEDHEADERS_ENABLED` | Honours `X-Forwarded-For` and `X-Forwarded-Proto` from any proxy | `true` (set by the image) |

The image also lowers the log level of the SQL commands to `Warning`, so the logs don't list every query.

## docker compose

`compose.yaml` builds the image (`ambio:local`) and runs it:

```yaml
services:
  ambio:
    build: .
    image: ambio:local
    restart: unless-stopped
    ports:
      - "8080:8080"
    env_file:
      - path: .env
        required: false
    volumes:
      - ./data:/data
```

The variables come from `.env`, which git ignores. Copy `.env.example` to start it. Write a value that contains `$` in single quotes, such as `Ambio__Account__Password='pa$$word'`, otherwise Compose reads `$…` as a variable.

## Reverse proxy

Run Ambio behind Caddy, Traefik, Nginx or your NAS's proxy.

- Forwarded headers (`X-Forwarded-For`, `X-Forwarded-Proto`) are honoured, so that redirects use `https`. Every proxy is trusted: publish port `8080` only to the proxy or the local network.
- Blazor Server needs **WebSockets**: make sure the proxy forwards the `Upgrade` and `Connection` headers.
- The clipboard needs a secure context: served over plain HTTP on the local network, the Copy buttons fall back to an older browser API.

## First run

> Run **one instance per database**. The guard around the account creation is in-process: two containers sharing the same SQLite file could each create an account ([ADR 0017](adr/0017-account-creation-local-or-github.md), [ADR 0028](adr/0028-account-created-at-first-start.md)).

1. Copy `.env.example` to `.env`, then set `Ambio__Account__Email` and `Ambio__Account__Password`. The password must have at least 6 characters, with an uppercase letter, a lowercase letter, a digit and a symbol.
2. Start the container with `docker compose up -d --build`. While no account exists, Ambio creates it from these variables before accepting requests, with its email already confirmed. Without them, or when they are refused, it stops and its log names the problem (`docker compose logs ambio`). There is no registration page: every page asks to log in.
3. Open <http://localhost:8080> and log in, then remove `Ambio__Account__Password` from `.env`: once the account exists, the variables are ignored. Change the password from **Account → Password**.
4. Enable 2FA from **Account → Two-factor authentication**.

In development, `appsettings.Development.json` sets a default account, so a new local database gets one at its first start. That file is never loaded in Production, which is the image's environment.

Deleting the account from **Account → Personal data** leaves the instance without an account: its next start creates it again from the variables, or refuses to start without them.

## Update

The migrations run at start, so an update is a new build. Back up `/data` first: a migration changes the database, and there is no way back.

```bash
git pull
docker compose up -d --build
```

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

Stop the container before a backup: SQLite keeps the latest changes in `ambio.db-wal` while it runs. Keep `/data/keys` in the backup: without it, existing cookies and 2FA tokens become invalid.
