# 0013 — GUID v7 primary keys

- Status: Accepted
- Date: 2026-09-26

## Context

Entities need a primary key. The usual choice with SQLite is an auto-increment `INTEGER`. That value is only known after the row is inserted, it can be guessed from a URL, and it collides as soon as data moves between two databases, which is exactly what the JSON export/import planned for Phase 6 does.

## Decision

- Every domain entity has a `Guid` key, created by the domain with `Guid.CreateVersion7()` when the entity is constructed.
- The mapping classes declare `ValueGeneratedNever()` on the key.
- ASP.NET Core Identity tables keep their default `string` keys.

## Consequences

- Entities have their identity before being saved, so object graphs, file names (`documents/<yyyy>/<id>.pdf`) and returned ids need no database round trip.
- Export and import between instances need no id remapping. Demo data can use fixed ids.
- URLs don't reveal how many rows exist.
- Because v7 GUIDs are time-ordered, inserts stay at the end of the index, and keys sort by creation time.
- Keys take more space (36-character `TEXT` in SQLite) and are harder to read. This is negligible for a single-user app.

See [data-model.md](../data-model.md#keys-guid-v7-rather-than-auto-increment-integers).
