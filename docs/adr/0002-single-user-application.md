# 0002 — Single-user application

- Status: Accepted
- Date: 2026-09-26

## Context

Ambio tracks one person's job search. It is self-hosted, one instance per user. Multi-tenancy would add data isolation on every query, user management, and quotas, with no benefit for this use case.

## Decision

- An instance has exactly one account.
- Registration is open only while no account exists, then closed.
- Entities are not scoped by user.
- ASP.NET Core Identity is kept for authentication: password, 2FA (authenticator), recovery codes, and a GitHub login that can only be **linked** to the existing account.
- Passkeys from the template are removed to reduce surface area.

## Consequences

- Simpler domain and queries.
- Several people need several instances (one container each).
- Moving to multi-user later would require adding an owner to the aggregates and a migration.
