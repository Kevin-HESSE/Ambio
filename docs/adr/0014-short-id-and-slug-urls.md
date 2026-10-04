# 0014 — Short id and slug in application URLs

- Status: Accepted
- Date: 2026-09-26

## Context

Applications are the pages visited most often. A URL built from the GUID key (`/applications/0192f1c4-7b3e-7d21-9a5c-3e8f4b2a6d10`) is unreadable, and hard to recognise in the browser history or when shared. A slug made only from the title isn't unique: the same job title comes up at several companies, and the same offer can be applied to twice.

## Decision

- Each `JobApplication` gets a `ShortId`: a **random** string generated with the [NanoId](https://github.com/codeyu/nanoid-net) library, stored as `TEXT` with a unique index. The creating service generates it and retries while the value is taken. The domain doesn't reference the library.
- The alphabet and size are configurable. They start numeric (`0123456789`, 6 characters) and can move to lowercase alphanumeric without a migration. The alphabet never contains `-`, which separates the id from the slug.
- URLs follow the format `/applications/{ShortId}-{slug}`, for example `/applications/482913-backend-developer-acme`.
- The slug is computed from the title and the company name by a `Slug.From(...)` helper. It isn't stored.
- Pages resolve the application by `ShortId` alone. A URL whose slug differs from the current one is redirected to the canonical URL.
- `Guid Id` stays the primary key and the target of foreign keys ([ADR 0013](0013-guid-v7-primary-keys.md)).

## Consequences

- URLs are readable, and the prefix prevents any collision between identical slugs.
- Renaming an application or its offer changes the slug without breaking old links.
- A random short id doesn't reveal how many applications exist, which a sequential number would.
- One more unique column, one small dependency (NanoId), and a retry loop at creation.
- Switching to an alphanumeric alphabet increases the number of possible ids without touching existing URLs.
- The same pattern can later be applied to companies and offers if their pages need it.
