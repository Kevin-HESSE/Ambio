# Testing

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
| `tests/Ambio.Infrastructure.Tests` | Service implementations (user management, email sending…), EF Core mappings and queries, file storage, plugin loader, PDF snapshots | xUnit, SQLite in-memory, NSubstitute, Bogus |
| `tests/Ambio.Web.Tests` | Blazor components: the app shell, then the interactive pages | bUnit, NSubstitute, Bogus |
| `tests/Ambio.E2E.Tests` | Critical user journeys in a real browser | xUnit, Playwright |

Shared settings (target framework, xUnit package, global `using Xunit;`) live in `tests/Directory.Build.props`, and package versions in `Directory.Packages.props`.

## Conventions

- **Layout.** Test folders and namespaces mirror the project under test, so they follow its feature folders ([ADR 0018](adr/0018-feature-folders-in-every-project.md)): `Users/UserServiceTests.cs` tests `Users/UserService.cs`. Helpers shared across features go in `Fixtures/`. End-to-end tests don't mirror a project: they are grouped by journey in `Journeys/`.
- **Names.** `Method_Scenario_Result`, e.g. `RegisterUserAsync_WhenUserExists_Fails`.
- **Member order.** Tests first, then private helpers (and helper types), then the data sets at the bottom of the class.
- **Theories.** More than two rows go in a `[MemberData]` backed by a `TheoryData<T>` property. With one or two rows, keep `[InlineData]`.
- **Test data.** Entities, DTOs and form inputs come from Bogus generators ([ADR 0022](adr/0022-test-data-with-bogus.md)), see [Test data](#test-data).
- **No magic strings.** Expected values are read from the generated data. A named constant is kept only for a value the test relies on: a name whose case or slug is checked, a sort order, a displayed date format. Expected messages come from the production constants (e.g. `UserErrors.AlreadyExists`), never retyped. Data set rows can stay literal.
- **Mocks.** Use NSubstitute, never hand-written fakes, only for what the test doesn't own (an email sender, an external API, the Application services consumed by a component). Services under test are resolved through the production DI extensions rather than built by hand.

## Test data

The Web and Infrastructure test projects build their data with [Bogus](https://github.com/bchavez/Bogus), in `Fixtures/`. Values are random on every run, without a seed.

- **A generator per type.** Each factory exposes `GetGenerator()`, a `Faker<T>` that fills every field with realistic values. Positional DTOs are built with the `WithRecord()` extension.
- **Variants are rule sets.** A factory exposes them as constants that include the default rules, such as `CompanyFactory.IsArchivedRuleSet` (`"default,archived"`) or `CompanyDetailsFactory.RequiredOnlyRuleSet`: `generator.Generate(CompanyFactory.IsArchivedRuleSet)`.
- **Set what the test checks or relies on.** Override a field with `with` on a record, or with a `WithCustom…` extension such as `WithCustomName("Acme Inc.")`, which adds the rule to a copy of the generator. Everything else stays random, and the assertions read it from the generated object.
- **Unique values.** `Name` and `Slug` are unique in the database. Bogus company names always have several words, so a test that needs two companies distinct by construction gives one of them a single-word name.
- **Stories for related entities.** Each test creates its own story, so no data is shared between tests.

## EF Core integration tests

Tests run against a real SQLite engine in memory, so they catch mapping and translation issues that the EF in-memory provider would hide.

- Each test gets its own named in-memory database (`DataSource=file:<guid>?mode=memory&cache=shared`). A keeper `SqliteConnection` stays open for the test's lifetime, so the database survives while contexts open and close their own connections.
- Services are registered with the production extensions (`AddDatabase()`, `AddApplicationServices()`), so tests use the same `IDbContextFactory<ApplicationDbContext>` and Identity setup as the app. See `tests/Ambio.Infrastructure.Tests/Fixtures/SqliteServiceProvider.cs`.
- Create the schema with `Database.EnsureCreated()`.
- `SqliteServiceProvider` has helpers so tests don't open scopes and contexts by hand: `AddAsync(entities)` saves entities on a new context, `CallAsync((ICompanyService service) => …)` calls a service resolved in its own scope, as a request would, and `QueryAsync(context => …)` reads what was saved on a new context. A test opens a context itself only to change an entity it has loaded.
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
- Only **critical journeys** get an E2E test, as one test per journey. The current ones are:
  - `Journeys/SingleAccountJourneyTests`: register, confirm the email, log in, log out, then check that registration is closed;
  - `Journeys/AppShellJourneyTests`: on a phone-sized viewport, open the More sheet, choose the Dark theme, check that it survives a reload, then log out from the sheet. It covers the shell script (`wwwroot/js/shell.js`), which bUnit doesn't run. It starts from `AmbioAppFixture.NewLoggedInPageAsync` with a phone viewport and the light color scheme;
  - `Journeys/CompanyJourneyTests`: create a company (after a link refused by the validation), check its detail page, edit it, then open it again from the list. It covers the statically rendered forms and their redirects, which bUnit doesn't post;
  - `Journeys/OfferJourneyTests`: create an offer while typing a new company name (after a link refused by the validation), find it in the offers list, then in the Offers tab of the company created with it. It covers the form post and the inline company creation.
- A journey that doesn't test the account itself starts with `var page = await _app.NewLoggedInPageAsync();`. The fixture creates the confirmed account (`AmbioAppFixture.UserEmail`, `UserPassword`) on the first call, opens a browser context on the app with the given options (viewport, color scheme), and logs in.
- Steps shared by several journeys are extension methods on Playwright's `IPage` in `Fixtures/PageExtensions.cs`, such as `LogInAsync`, which works at any viewport width.
- Select elements by role or label (`GetByRole`, `GetByLabel`), and add a `data-testid` only when nothing else is stable. Assert with Playwright's `Expect`, which waits for the page instead of failing at once.

## PDF snapshot tests

Every CV template (built-in or bundled plugin) is rendered with a fixed `CvModel` fixture.

- The test checks that the PDF is valid and has the expected page count.
- Pages are rasterized to images and compared with approved images stored next to the test. A differing snapshot fails the test and writes the new image for review.

## Designing templates by hand

`tools/Ambio.CvPlayground` renders a template with sample data in the QuestPDF Companion for live preview. It isn't a test, but it's the fastest way to iterate on a layout before approving new snapshots. See [plugins.md](plugins.md#developing-a-template).
