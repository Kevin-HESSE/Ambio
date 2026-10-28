# 0021 — Slug URLs for companies

- Status: Accepted
- Date: 2026-10-11

## Context

Company pages used the GUID key in their URL (`/companies/0192f1c4-7b3e-7d21-9a5c-3e8f4b2a6d10`), which is unreadable and hard to recognise in the browser history. [ADR 0014](0014-short-id-and-slug-urls.md) gives applications a short id followed by a computed slug, and leaves the same pattern open to companies. Unlike application titles, company names are already unique, so the name alone can identify the company in a URL.

## Decision

This completes ADR 0014 for companies:

- URLs follow the format `/companies/{slug}` and `/companies/{slug}/edit`, for example `/companies/brightwave`. There is no short id.
- The slug is **stored** in `Company.Slug`, as `TEXT` with a unique index. The company services generate it from the name; the domain only holds the value.
- The slug comes from the `Slug.From(...)` helper in `Ambio.Application/Common`, shared with applications: lowercase ASCII letters and digits only, accents removed, `œ`, `æ` and `ß` spelled out (`oe`, `ae`, `ss`), every other run of characters replaced by one `-`, truncated to 60 characters. When nothing is left, the caller's fallback is used: `company` for companies.
- A slug already held by another company, archived or not, gets the first free numeric suffix: `Acme Inc.` → `acme-inc`, then `Acme, Inc` → `acme-inc-2`, `acme-inc-3`…
- Slugs that a page route would catch first (`new`, for `/companies/new`) are reserved and get a suffix too: `new-2`.
- Only a **rename** recomputes the slug, leaving the company itself out of the check, so a rename that gives the same slug keeps it. Editing other fields never changes the URL.
- **Old links aren't kept**: after a rename, the previous slug leads to the Not Found page. This is acceptable for a single-user app ([ADR 0002](0002-single-user-application.md)).
- `Guid Id` stays the primary key and the target of foreign keys ([ADR 0013](0013-guid-v7-primary-keys.md)). Services still update a company by its id; only pages look companies up by slug.

## Consequences

- Company URLs are readable and short, and need no extra id or lookup table.
- A rename breaks bookmarks and links to the old URL. A slug history with redirects could be added later if that becomes a problem.
- Each create or rename runs one query per slug candidate. Collisions are rare, so this stays negligible.
- Two simultaneous creations could compute the same slug: the unique index rejects the second one, as it already does for the name.
- Adding a page under `/companies/` with a literal segment requires adding that segment to the reserved slugs.
