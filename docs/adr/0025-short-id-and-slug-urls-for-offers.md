# 0025 — Short id and slug in offer URLs

- Status: Accepted
- Date: 2026-10-21

## Context

Offer pages used the GUID key in their URL (`/offers/0192f1c4-7b3e-7d21-9a5c-3e8f4b2a6d10`), which is unreadable and hard to recognise in the browser history. [ADR 0014](0014-short-id-and-slug-urls.md) gives applications a short id followed by a computed slug, and leaves the same pattern open to offers. Unlike company names ([ADR 0021](0021-slug-urls-for-companies.md)), offer titles aren't unique: the same title comes up at several companies, and a company can publish the same title twice. A stored slug alone can't identify an offer.

## Decision

This completes ADR 0014 for offers:

- Each `JobOffer` gets a `ShortId`, drawn as for applications: random, from the same NanoId generator and the same `Ambio:ShortId` setting (alphabet and size), stored as `TEXT` with a unique index. The creating service draws again while the value is taken. The domain only holds the value.
- The unique index is per table: an offer and an application can share a short id, as the first segment of their URL tells them apart.
- URLs follow the format `/offers/{ShortId}-{slug}` and `/offers/{ShortId}-{slug}/edit`, for example `/offers/482913-backend-developer-kelvio`.
- The slug is computed from the title and the company name by the shared `Slug.From(...)` helper, with `offer` as the fallback. It isn't stored.
- Pages resolve the offer by `ShortId` alone. A URL whose slug differs from the current one is redirected to the canonical URL.
- Links that pass an offer to another page use its short id too: `/applications/new?offer=482913`.
- `Guid Id` stays the primary key and the target of foreign keys ([ADR 0013](0013-guid-v7-primary-keys.md)). Services still update an offer and its postings by its id.

## Consequences

- Offer URLs are readable, and two offers with the same title and company still get distinct URLs.
- Renaming an offer, or its company, changes the slug without breaking old links. The URLs of its applications change with it, since their slug is built from the same title.
- Offers and applications share one short id setting: widening the alphabet applies to both.
- One more unique column, and a retry loop when an offer is created.
