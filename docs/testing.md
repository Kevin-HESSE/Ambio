# Testing

> Status: the test projects exist since [Phase 0](roadmap.md#phase-0--foundations), except `Ambio.Domain.Tests`, created in [Phase 1](roadmap.md#phase-1--application-tracking-mvp) with the first domain rule.

```bash
# every test, end-to-end included
dotnet test

# fast loop: everything but the end-to-end tests
dotnet test -- --filter-not-trait "Category=E2E"

# end-to-end tests only
dotnet test --project tests/Ambio.E2E.Tests
```

Tests run on xUnit v3 with Microsoft.Testing.Platform, enabled for `dotnet test` by `global.json`. A trait filter can leave a project with zero tests. Its exit code (8) is ignored through `tests/Directory.Build.props`, so a filtered run still succeeds.

## Approach

- **Critical points first.** There is no coverage target. Test business rules, guards (such as the single-account semaphore of [ADR 0017](adr/0017-account-creation-local-or-github.md)), data access, integrations (such as email sending) and the journeys that would hurt if they broke. Don't test trivial getters or framework behavior.
- **A test project is created when its layer has something worth testing.** `Ambio.Domain.Tests` comes with the first domain rule, not before.
- **Real dependencies where they are cheap.** Services run against a real SQLite engine in memory, wired with the production DI extensions, rather than the EF in-memory provider or mocked contexts. Mocks are kept for what the test doesn't own, and are always NSubstitute mocks, never hand-written fakes.
- **A few end-to-end journeys** cover what unit and component tests can't reach, such as the statically rendered Identity pages.

## Test projects

The content listed below is typical, not exhaustive: each project grows with its layer.

| Project | Typical content | Tools |
|---|---|---|
| `tests/Ambio.Domain.Tests` | Domain rules (status workflow, invariants) — created in Phase 1 | xUnit |
| `tests/Ambio.Application.Tests` | DTO validation, `ServiceResult`, mapping extensions | xUnit |
| `tests/Ambio.Infrastructure.Tests` | Service implementations (user management, email sending…), EF Core mappings and queries, file storage, plugin loader, PDF snapshots | xUnit, SQLite in-memory, NSubstitute |
| `tests/Ambio.Web.Tests` | Interactive Blazor components | bUnit, NSubstitute |
| `tests/Ambio.E2E.Tests` | Critical user journeys in a real browser | xUnit, Playwright |

Shared settings (target framework, xUnit package, global `using Xunit;`) live in `tests/Directory.Build.props`, and package versions in `Directory.Packages.props`.

## Conventions

- **Layout.** Test folders and namespaces mirror the project under test, so they follow its feature folders ([ADR 0018](adr/0018-feature-folders-in-every-project.md)): `Users/UserServiceTests.cs` tests `Users/UserService.cs`. Helpers shared across features go in `Fixtures/`. End-to-end tests don't mirror a project: they are grouped by journey in `Journeys/`.
- **Names.** `Method_Scenario_Result`, e.g. `RegisterUserAsync_WhenUserExists_Fails`.
- **Member order.** Tests first, then private helpers (and helper types), then the data sets at the bottom of the class.
- **Theories.** More than two rows go in a `[MemberData]` backed by a `TheoryData<T>` property. With one or two rows, keep `[InlineData]`.
- **No magic strings.** Values reused in a class are named constants. Expected messages come from the production constants (e.g. `UserErrors.AlreadyExists`), never retyped. Data set rows can stay literal.
- **Mocks.** Use NSubstitute, never hand-written fakes, only for what the test doesn't own (an email sender, an external API, the Application services consumed by a component). Services under test are resolved through the production DI extensions rather than built by hand.

## EF Core integration tests

Tests run against a real SQLite engine in memory, so they catch mapping and translation issues that the EF in-memory provider would hide.

- Each test gets its own named in-memory database (`DataSource=file:<guid>?mode=memory&cache=shared`). A keeper `SqliteConnection` stays open for the test's lifetime, so the database survives while contexts open and close their own connections.
- Services are registered with the production extensions (`AddDatabase()`, `AddApplicationServices()`), so tests use the same `IDbContextFactory<ApplicationDbContext>` and Identity setup as the app. See `tests/Ambio.Infrastructure.Tests/Fixtures/SqliteServiceProvider.cs`.
- Create the schema with `Database.EnsureCreated()`.
- Test mapping classes (keys, required fields, relationships, cascade rules) and every LINQ projection to DTOs.

## Component tests

bUnit renders components with NSubstitute mocks of the Application services, registered in the test context's `Services`, and asserts on markup and interactions (filters, forms, validation messages). Test classes inherit `BunitContext`.

The Identity pages under `Components/Account` are rendered statically on the server and read `HttpContext`, which bUnit doesn't provide. They are covered by the end-to-end tests instead.

## End-to-end tests

Playwright drives a headless Chromium against the real app, hosted in the test process.

- `AmbioAppFactory` is a `WebApplicationFactory<Program>` that starts Kestrel on a free port (`UseKestrel(0)`) and overrides `ConnectionStrings:DefaultConnection`.
- `AmbioAppFixture` creates a temporary SQLite file, applies the migrations, starts the browser, and deletes the file at the end. The development database is never touched.
- Chromium is installed on the first run (`Microsoft.Playwright.Program.Main(["install", "chromium"])`), so no separate install step is needed locally. On a Linux CI runner, the browser's system dependencies must be installed too (see [ci-cd.md](ci-cd.md)).
- Every E2E test class carries `[Trait("Category", "E2E")]`, so the fast loop can exclude it.
- Only **critical journeys** get an E2E test, as one test per journey. The current one is `Journeys/SingleAccountJourneyTests`: register, confirm the email, log in, log out, then check that registration is closed.
- Select elements by role or label (`GetByRole`, `GetByLabel`), and add a `data-testid` only when nothing else is stable. Assert with Playwright's `Expect`, which waits for the page instead of failing at once.

## PDF snapshot tests

Every CV template (built-in or bundled plugin) is rendered with a fixed `CvModel` fixture.

- The test checks that the PDF is valid and has the expected page count.
- Pages are rasterized to images and compared with approved images stored next to the test. A differing snapshot fails the test and writes the new image for review.

## Designing templates by hand

`tools/Ambio.CvPlayground` renders a template with sample data in the QuestPDF Companion for live preview. It isn't a test, but it's the fastest way to iterate on a layout before approving new snapshots. See [plugins.md](plugins.md#developing-a-template).
