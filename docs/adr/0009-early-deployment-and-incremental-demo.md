# 0009 — Early deployment and an incremental demo

- Status: Accepted
- Date: 2026-09-26

## Context

The app should be useful for an ongoing job search as soon as possible, and showcase-ready on GitHub throughout development.

## Decision

- The Docker and deployment phase comes right after the application-tracking MVP, before the candidate profile and CV generation.
- The same phase adds a demo: a seeder enabled by `Postulo__Demo__Enabled` that creates a demo account and sample data.
- Every following phase ends with a task that extends the demo with the new feature.
- CV upload is part of the MVP, so sent CVs are kept before generation exists.

## Consequences

- A deployed, usable app early on.
- The demo seeder must be maintained alongside every feature.
- Deployment concerns (volumes, migrations at startup, proxies) are tackled early, when the app is still small.
