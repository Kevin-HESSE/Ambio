# 0028 — Account created at first start

- Status: Accepted
- Date: 2026-10-28
- Supersedes in part: [ADR 0002](0002-single-user-application.md) (registration open while no account exists) and [ADR 0017](0017-account-creation-local-or-github.md) (the Register page, the first person to register gets the account, an unconfirmed account recovered with Resend email confirmation)

## Context

[ADR 0002](0002-single-user-application.md) and [ADR 0017](0017-account-creation-local-or-github.md) keep registration open until the single account exists. A freshly deployed instance therefore belongs to whoever reaches it first, and its owner has to register before exposing it. Registration also relies on an email confirmation, which the proof of concept can only show on screen, since it sends no email ([ADR 0024](0024-proof-of-concept.md)).

## Decision

- **The account comes from the configuration.** `Ambio:Account:Email` and `Ambio:Account:Password` (`Ambio__Account__Email`, `Ambio__Account__Password` as environment variables) are read only while no account exists.
- **It is created before the app accepts requests**, through `IUserService.RegisterUserAsync` and its guard ([ADR 0017](0017-account-creation-local-or-github.md)). Its email is confirmed at creation: it is given by whoever deploys the instance.
- **No account, no start.** When no account exists and the settings are missing or refused (invalid email, password rejected by Identity), the app stops with a message that names the settings.
- **Once the account exists, the settings are ignored.** Changing them doesn't change the account.
- **There is no registration page.**
- **Every page requires the logged-in user**, through a fallback authorization policy. The exceptions are explicit: the pages shown before logging in (log in, two-factor, password reset, email confirmation), the error page, and the static files.

## Consequences

- There is no longer a window during which a stranger can take a fresh instance.
- The guard is still in-process: one instance per database ([ADR 0017](0017-account-creation-local-or-github.md)).
- The password stays in the configuration until it is removed. The owner changes it from **Account → Password**; the setting doesn't follow.
- An account deleted from **Account → Personal data** leaves the instance without an account until its next start, which creates it again from the settings or refuses to start.
- Steps that touch the database at start (migrations, the demo seeder, [roadmap](../roadmap.md) Phase 2) run before this one. The demo account will take the single account's place.
- A new page is private by default; making it public is a visible choice.
- The official version can reuse the principle: an API that creates its account from its configuration and exposes no public registration endpoint.
