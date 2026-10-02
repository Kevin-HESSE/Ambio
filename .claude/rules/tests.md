---
paths: tests/**
---

# Tests rules

See [docs/testing.md](../../docs/testing.md) for the full approach.

## Projects

| Project | Content | Tools |
|---|---|---|
| `Ambio.Domain.Tests` | Domain rules (created in Phase 1) | xUnit |
| `Ambio.Application.Tests` | DTO, validation, mapping extensions | xUnit |
| `Ambio.Infrastructure.Tests` | Services, EF Core mappings and queries, storage, PDF snapshots | xUnit, SQLite in-memory, NSubstitute |
| `Ambio.Web.Tests` | Interactive Blazor components | bUnit, NSubstitute |
| `Ambio.E2E.Tests` | Critical user journeys | xUnit, Playwright |

Shared settings live in `tests/Directory.Build.props`, package versions in `Directory.Packages.props`.

## Conventions

- Folders and namespaces mirror the project under test, so they follow its feature folders ([ADR 0018](../../docs/adr/0018-feature-folders-in-every-project.md)): `Ambio.Application.Tests/Users/Dtos/RegisterInputValidationTests.cs` tests `Ambio.Application/Users/Dtos/RegisterInput.cs`, `Ambio.Infrastructure.Tests/Users/UserServiceTests.cs` tests `Ambio.Infrastructure/Users/UserService.cs`.
- Test helpers shared across features go in `Fixtures/`.
- End-to-end tests are the exception: they don't mirror a project, so they are grouped by journey in `Journeys/`, with their fixtures in `Fixtures/`.
- Test names follow `Method_Scenario_Result`, e.g. `RegisterUserAsync_WhenUserExists_Fails`.
- Member order: tests first, then private helpers (and helper types), then the data sets at the bottom of the class.
- Theories: more than two rows go in a `[MemberData]` backed by a `TheoryData<T>` property; with one or two rows, keep `[InlineData]`.
- No magic strings: values reused in a class are `private const` fields at the top; expected messages come from production constants (e.g. `UserErrors.AlreadyExists`), never retyped. Data set rows can stay literal.
- Pass `TestContext.Current.CancellationToken` to async calls that take a token.
- Mocks use NSubstitute, never hand-written fakes, and only for what the test doesn't own (email sender, external API, Application services consumed by a component). Services under test are resolved through the production DI extensions, not built by hand.

## EF Core integration tests

- Real SQLite engine in memory, one named database per test, through `Fixtures/SqliteServiceProvider.cs` (uses `AddDatabase()` and `AddApplicationServices()`).
- Schema created with `Database.EnsureCreated()`.
- Cover mapping classes (keys, required fields, relationships, cascade rules) and every LINQ projection to DTOs.

## Component tests

- Test classes inherit `BunitContext`. The Application services a component consumes are NSubstitute mocks registered in the bUnit `Services`, as in the Infrastructure tests.
- `Components/Account` pages are rendered statically and need `HttpContext`: cover them with E2E tests, not bUnit.

## End-to-end tests

- Every E2E class carries `[Trait("Category", "E2E")]`.
- One test per critical journey only.
- Select elements with `GetByRole` / `GetByLabel`; add a `data-testid` only when nothing else is stable. Assert with Playwright's `Expect`.
