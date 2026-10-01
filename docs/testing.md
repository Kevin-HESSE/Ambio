# Testing

> Status: test projects are created in [Phase 0](roadmap.md#phase-0--foundations).

```bash
dotnet test
```

## Test projects

| Project | Scope | Tools |
|---|---|---|
| `tests/Ambio.Domain.Tests` | Entities and domain rules (status workflow, invariants) | xUnit |
| `tests/Ambio.Application.Tests` | Services and DTO mapping extensions | xUnit |
| `tests/Ambio.Infrastructure.Tests` | EF Core mappings and queries, file storage, plugin loader, PDF snapshots | xUnit, SQLite in-memory |
| `tests/Ambio.Web.Tests` | Blazor components | bUnit |

## EF Core integration tests

Tests run against a real SQLite engine in memory, so they catch mapping and translation issues that the EF in-memory provider would hide.

- Each test gets its own named in-memory database (`DataSource=file:<guid>?mode=memory&cache=shared`). A keeper `SqliteConnection` stays open for the test's lifetime, so the database survives while contexts open and close their own connections.
- Services are registered with the production extensions (`AddDatabase()`, `AddApplicationServices()`), so tests use the same `IDbContextFactory<ApplicationDbContext>` and Identity setup as the app. See `tests/Ambio.Infrastructure.Tests/Fixtures/SqliteServiceProvider.cs`.
- Create the schema with `Database.EnsureCreated()`.
- Test mapping classes (keys, required fields, relationships, cascade rules) and every LINQ projection to DTOs.

## Component tests

bUnit renders components with fake Application services, and asserts on markup and interactions (filters, forms, validation messages).

## PDF snapshot tests

Every CV template (built-in or bundled plugin) is rendered with a fixed `CvModel` fixture.

- The test checks that the PDF is valid and has the expected page count.
- Pages are rasterized to images and compared with approved images stored next to the test. A differing snapshot fails the test and writes the new image for review.

## Designing templates by hand

`tools/Ambio.CvPlayground` renders a template with sample data in the QuestPDF Companion for live preview. It isn't a test, but it's the fastest way to iterate on a layout before approving new snapshots. See [plugins.md](plugins.md#developing-a-template).
