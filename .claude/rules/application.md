---
paths: src/Ambio.Application/**
---

# Application rules

- `Ambio.Application` references Domain only (and `Ambio.Plugins.CV.Abstractions` once it exists): no EF Core, no Infrastructure.
- Code is organised by feature ([ADR 0018](../../docs/adr/0018-feature-folders-in-every-project.md)), e.g. for `Users/`:
  - `Users/IUserService.cs`: service interface
  - `Users/Dtos/RegisterInput.cs`: DTOs
  - `Users/UserMappingExtensions.cs`: mapping
  - `Users/UserErrors.cs`: error messages
  - `Common/`: cross-feature types (`ServiceResult`)
- The folder `<Feature>` is a plural noun (`Users`, `Companies`) so a namespace never clashes with an entity type name; the types inside take the singular entity name (`IUserService`, `UserErrors`, `CompanyMappingExtensions`).
- Namespaces mirror folders.
- DTOs are `record` types. Input DTOs may carry DataAnnotations validation (allowed here, forbidden in Domain).
- Mapping is written by hand as `ToDto()` extension methods, one `<Entity>MappingExtensions` class in the feature folder. No mapping library (AutoMapper, Mapster, Mapperly) ([ADR 0006](../../docs/adr/0006-persistence-ignorant-domain-and-dtos.md)).
- Services return DTOs, `ServiceResult` or `ServiceResult<T>`, never entities.
- Async methods end with `Async` and take a `CancellationToken cancellationToken = default`.
- No magic strings: user-facing error messages are `public const` fields in `<Feature>Errors` (e.g. `UserErrors.AlreadyExists`), referenced by the service and by its tests.
