---
paths: src/Ambio.Domain/**
---

# Domain rules

- `Ambio.Domain` references nothing: no project reference, no persistence or infrastructure package (no EF Core, no NanoId).
- Code is organised by feature ([ADR 0018](../../docs/adr/0018-feature-folders-in-every-project.md)): `<Feature>/<Entity>.cs` (e.g. `Companies/Company.cs`, `JobApplications/JobApplication.cs`); cross-feature types go in `Common/`. `<Feature>` is a plural noun, so a namespace never clashes with an entity type name. Namespaces mirror folders.
- Entities are persistence-ignorant: no data annotations (`[Key]`, `[MaxLength]`…), no `DbContext`, no EF Core type ([ADR 0006](../../docs/adr/0006-persistence-ignorant-domain-and-dtos.md)).
- Every entity has a `Guid Id`, set in its constructor with `Guid.CreateVersion7()` ([ADR 0013](../../docs/adr/0013-guid-v7-primary-keys.md)).
- Timestamps are UTC `DateTime` (never `DateTimeOffset`, which SQLite can't order); calendar dates are `DateOnly`. `CreatedAt` / `UpdatedAt` are set by Infrastructure, not by the domain.
- `Company`, `Contact`, `JobOffer` and `JobApplication` carry a nullable `ArchivedAt` (null = active): archive and restore instead of deleting ([ADR 0012](../../docs/adr/0012-archive-instead-of-delete.md)).
- Many-to-many relationships have no join-entity class; they are configured in Infrastructure.
- Business rules and invariants (status workflow…) live in the entities and are covered by `tests/Ambio.Domain.Tests`.
- Entities never reach the UI: Application services expose DTOs.

See [docs/data-model.md](../../docs/data-model.md) for the model.
