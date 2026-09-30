# 0017 — Account creation: local or GitHub, one account at most

- Status: Accepted
- Date: 2026-09-30
- Refines: [ADR 0002](0002-single-user-application.md)

## Context

[ADR 0002](0002-single-user-application.md) allows GitHub only as a login **linked** to an existing account. Someone who self-hosts Ambio may want to use GitHub only, without creating a local password first. Registration also has to stay closed once the account exists, even when two registrations arrive at the same time.

## Decision

- The single account is created in one of three ways:
  - **local registration** with an email and a password;
  - **local registration, then GitHub linked** from **Account → External logins**, after which GitHub can be used to log in;
  - **directly with GitHub**, from the Log in page.
- **One account at most.** Whatever the way, the first account closes registration. Neither the Register page nor GitHub can create a second account. A GitHub identity that is not linked to the account is redirected to Log in with a message.
- **Email of a GitHub account.** The account is confirmed at creation when GitHub returns a verified primary email. Otherwise the usual email confirmation applies.
- **An unconfirmed account still counts as existing.** Its owner recovers it with **Resend email confirmation**.
- **Password added later.** An account created with GitHub can add a local password from **Account → Password**. GitHub can be unlinked only while another way to log in remains.
- **No allow-list of GitHub accounts.** As with local registration, the first person to register gets the account.
- **One guard for every way.** Every account creation goes through `IUserService`. It holds a singleton `SemaphoreSlim` around "check that no account exists, then create it", so two concurrent requests cannot both pass the check.

## Consequences

- The guard is **in-process**. It holds for one container per database, which is how Ambio is deployed ([ADR 0002](0002-single-user-application.md)). Several instances on the same SQLite file are not supported.
- The GitHub account creation (Phase 5) must reuse `IUserService` rather than the template's `ExternalLogin` code, which creates users directly.
- Until the account is created at first start, a freshly deployed instance belongs to whoever registers first. Deploy it and register before exposing it.
- Adding a password or linking GitHub never creates a second user, so the single-user model of [ADR 0002](0002-single-user-application.md) is unchanged.
