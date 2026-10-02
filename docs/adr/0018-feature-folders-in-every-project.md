# 0018 — Feature folders in every project

- Status: Accepted
- Date: 2026-10-02
- Supersedes: [ADR 0006](0006-persistence-ignorant-domain-and-dtos.md) (location of the mapping classes only)

## Context

[ADR 0006](0006-persistence-ignorant-domain-and-dtos.md) puts every `IEntityTypeConfiguration<T>` in a technical folder, `Ambio.Infrastructure/Persistence/Configurations/`. [architecture.md](../architecture.md) plans the Application project by feature (`Application/<Feature>/Dtos/`), but the code grew by type instead: `Dtos/`, `Interfaces/`, `Common/` in Application, `Service/` and `Persistence/` in Infrastructure, and the tests mirror that layout.

With both styles mixed, the code of one feature is spread over several technical folders in each project, and nothing tells where a new file belongs.

## Decision

- **Every project is organised by feature.** A feature folder holds everything the project needs for that feature. Code shared by several features goes in a cross-feature folder. Namespaces mirror folders.
- **Naming.** The folder is a plural noun (`Users`, `Companies`, `JobApplications`), so a namespace never clashes with an entity type name. The types inside take the singular entity name (`IUserService`, `UserErrors`, `CompanyMappingExtensions`).
- **Layout per project:**

  | Project | Feature folder | Cross-feature |
  |---|---|---|
  | `Ambio.Domain` | `<Feature>/<Entity>.cs` | `Common/` |
  | `Ambio.Application` | `<Feature>/I<Entity>Service.cs`, `<Feature>/Dtos/`, `<Feature>/<Entity>MappingExtensions.cs`, `<Feature>/<Entity>Errors.cs` | `Common/` (`ServiceResult`) |
  | `Ambio.Infrastructure` | `<Feature>/<Entity>Service.cs`, `<Feature>/<Entity>Configuration.cs`, other feature types (`Users/ApplicationUser.cs`) | `Common/` (`SemaphoreContainer`), `Persistence/` (`ApplicationDbContext`, `Migrations/`, interceptors), `InfrastructureExtensions.cs` at the root |
  | `Ambio.Web` | `Components/<Feature>/Pages/`, `Components/<Feature>/Shared/`, like `Components/Account/` | `Components/Layout/`, `Components/Shared/`, `Components/Pages/` (Home, Error, NotFound) |
  | Test projects | Mirror the project under test | `Fixtures/` |

- **EF Core mapping classes move to the feature folder** of Infrastructure, next to the service that uses the entity. They are still applied with `ApplyConfigurationsFromAssembly`, which finds them anywhere in the assembly.
- **End-to-end tests are the exception**: they don't mirror a project, and a journey spans several features, so they stay grouped by journey in `Journeys/`.
- Everything else in [ADR 0006](0006-persistence-ignorant-domain-and-dtos.md) stands: persistence-ignorant domain, one mapping class per entity, DTOs, hand-written mapping.

## Consequences

- The code of a feature sits in one folder per project, and its tests in the matching folder of the test project.
- Moving a file changes its namespace, so callers need a new `using`.
- `Persistence/` no longer grows with each entity: it only holds what every feature shares.
- The existing code (`Dtos/`, `Interfaces/`, `Service/`, `Persistence/ApplicationUser.cs`) is moved by a Phase 0 issue.
