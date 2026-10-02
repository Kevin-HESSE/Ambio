---
paths: src/Ambio.Application/**
---

# Application rules

- `Ambio.Application` references Domain only (and `Ambio.Plugins.CV.Abstractions` once it exists): no EF Core, no Infrastructure.
- Code is organised by feature ([docs/architecture.md](../../docs/architecture.md#conventions)):
  - `<Feature>/I<Feature>Service.cs`: service interface
  - `<Feature>/Dtos/`: DTOs
  - `<Feature>/<Feature>MappingExtensions.cs`: mapping
  - `<Feature>/<Feature>Errors.cs`: error messages
  - `Common/`: cross-feature types (`ServiceResult`)
- Namespaces mirror folders.
- DTOs are `record` types. Input DTOs may carry DataAnnotations validation (allowed here, forbidden in Domain).
- Mapping is written by hand as `ToDto()` extension methods, one `<Feature>MappingExtensions` class per feature. No mapping library (AutoMapper, Mapster, Mapperly) ([ADR 0006](../../docs/adr/0006-persistence-ignorant-domain-and-dtos.md)).
- Services return DTOs, `ServiceResult` or `ServiceResult<T>`, never entities.
- Async methods end with `Async` and take a `CancellationToken cancellationToken = default`.
- No magic strings: user-facing error messages are `public const` fields in `<Feature>Errors` (e.g. `UserErrors.AlreadyExists`), referenced by the service and by its tests.
