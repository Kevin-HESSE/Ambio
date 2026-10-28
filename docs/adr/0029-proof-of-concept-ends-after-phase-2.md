# 0029 — The proof of concept ends after Phase 2

- Status: Accepted
- Date: 2026-10-28
- Supersedes: [ADR 0008](0008-docker-hub-multi-arch-images.md), [ADR 0009](0009-early-deployment-and-incremental-demo.md); in part [ADR 0024](0024-proof-of-concept.md) (the scope kept: the demo, company research and contacts, the candidate profile and CV generation, the export)

## Context

Phase 1 delivers application tracking, and Phase 2 runs it in Docker. That is enough for a real job search. The official version is about to be designed in another repository ([ADR 0024](0024-proof-of-concept.md)), and what this proof of concept still has to teach comes from real use more than from new features.

## Decision

- **The proof of concept ends with Phase 2, released as `v0.1.0`.** No feature is added after it.
- **It is used for a real job search.** What that use shows is recorded in [poc-findings.md](../poc-findings.md), under "Real use".
- **Docker runs from a local build.** `compose.yaml` builds the image from the repository ([deployment.md](../deployment.md)). The image isn't published on Docker Hub, and there is no CD workflow.
- **The demo is dropped.** The official version will have its own.
- **Company research and contacts, the candidate profile, CV generation and the JSON export are left to the official version.** Their design (the [roadmap](../roadmap.md), [data-model.md](../data-model.md), [ADR 0004](0004-questpdf-for-cv-generation.md), [ADR 0007](0007-cv-snapshots-selection-and-override.md)) stays as a basis.

## Consequences

- The migrations are squashed into a single `InitialCreate` before `v0.1.0`. From then on, real data exists: a migration must keep it.
- Without the JSON export, the data of real use lives in the SQLite database and the `documents/` folder. How it moves to the official version is decided there; the GUID v7 keys ([ADR 0013](0013-guid-v7-primary-keys.md)) and the enums stored as text make it easier.
- The Dockerfile cross-compiles for the target architecture, so a multi-arch image remains a matter of a CD workflow.
- No demo seeder has to be kept up to date.
