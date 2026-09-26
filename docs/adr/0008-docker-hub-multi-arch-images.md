# 0008 — Multi-arch Docker images on Docker Hub

- Status: Accepted
- Date: 2026-09-26

## Context

Postulo runs on a VPS (usually amd64) or a NAS / single-board computer (often arm64), behind a reverse proxy.

## Decision

- GitHub Actions builds the image with `docker buildx` for `linux/amd64` and `linux/arm64` on every version tag.
- The image is published to **Docker Hub**.
- The container runs as non-root on HTTP port 8080. TLS is terminated by the reverse proxy, and forwarded headers are enabled.
- Migrations are applied at startup.

## Consequences

- One image name works on every target.
- Docker Hub credentials are stored as GitHub Actions secrets.
- arm64 builds under emulation are slower. They only run on tags.
