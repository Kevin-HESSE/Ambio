# 0023 — Interactive islands in static pages

- Status: Accepted
- Date: 2026-10-14

## Context

Every page of Ambio is rendered statically on the server: forms are posted and the server answers with a redirect or the page with its errors. The shell's behaviors (sheets, theme) are plain JavaScript in `wwwroot/js/shell.js`. This keeps pages light and works without a live connection.

The offer page ([#46](https://github.com/Kevin-HESSE/Ambio/issues/46)) adds a posting from a sheet. Posted statically, a form with errors reloads the page and the sheet closes, hiding its messages. Making the sheet reopen after the post would need JavaScript to recover state that an interactive component keeps by itself.

Blazor offers Interactive Server rendering, by page or by component. The layout is static, and the app bar slots (`AppBarTitle`, `AppBarActions`, `AppBarBack`) are sections, which don't cross render-mode boundaries: a whole interactive page would lose them.

## Decision

- Pages stay statically rendered by default.
- When a slice needs interactivity, only the **smallest component** that needs it gets `@rendermode InteractiveServer`, set by the static page that renders it: an interactive island.
- The page reads the data and passes it to the island as serializable parameters (ids, DTO records), so nothing is loaded twice by the prerender. The island then keeps its own state up to date through the Application services.
- Its buttons are disabled while `RendererInfo.IsInteractive` is false, so a click before the circuit is up isn't lost.
- Sheets are still opened by `data-sheet-open`. An island closes one from code with `Sheet.CloseAsync()`, which calls `ambio.sheet.close` in `shell.js`.
- Services already create one `DbContext` per operation through `IDbContextFactory` ([ADR 0010](0010-dbcontext-factory-without-repositories.md)), which is safe in a long-lived circuit.

## Consequences

- The components rendered by an island (`Sheet`, `FormField`, `Tag`…) become interactive with it, without change: they only produce markup. The page, the layout and the shell stay static.
- An island can't contain what only works statically: the app bar sections, `HttpContext`, or forms posted with a `FormName`.
- A static page can't pass a `RenderFragment` or an `EventCallback` to an island: the island owns its part of the page, including its counts.
- Each interactive page holds a SignalR circuit while open; the reconnection UI (`ReconnectModal`) shows when it drops.
- Each later slice decides whether it needs an island, such as the duplicate suggestion of a new offer or an Undo toast; lists, filters and edit forms can stay static.
- bUnit tests set the render mode with `SetRendererInfo`, and mock the `ambio.sheet.close` call with `JSInterop`. The end-to-end tests cover the circuit itself.
