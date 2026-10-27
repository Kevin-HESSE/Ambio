# Proof of concept findings

Ambio is a proof of concept for an official version built in separate repositories: an API, a SPA frontend, maybe a desktop app ([ADR 0024](adr/0024-proof-of-concept.md)). This file records what it teaches.

- Write down what real use shows: a need confirmed, a feature never used, something missing or awkward.
- Every slice ends with an entry in the [slice log](#slice-log).
- A finding is a fact or an observation, not a decision: the official version decides.

## Basis for the official version

These ADRs are worth reading again when the official version is designed. They are a basis, not decisions to copy.

| ADR | Topic | Note |
|---|---|---|
| [0002](adr/0002-single-user-application.md) | Single user | To reconsider if the API serves several people or devices |
| [0005](adr/0005-sqlite-and-filesystem-storage.md) | SQLite and files on disk | |
| [0006](adr/0006-persistence-ignorant-domain-and-dtos.md) | Persistence-ignorant domain, DTOs | The DTOs are a starting point for the API contracts |
| [0011](adr/0011-company-centric-model-with-job-offers.md) | Company-centric model, application hierarchy | |
| [0012](adr/0012-archive-instead-of-delete.md) | Archive instead of delete | |
| [0027](adr/0027-archiving-cascade-and-restore.md) | Archiving cascade and restore | Undo is a restore |
| [0013](adr/0013-guid-v7-primary-keys.md) | GUID v7 keys | Lets the official version import the export without remapping ids |
| [0014](adr/0014-short-id-and-slug-urls.md) | Short id and slug URLs | |
| [0015](adr/0015-neutral-visual-identity-with-status-colors.md) | Visual identity and status colors | |
| [0017](adr/0017-account-creation-local-or-github.md) | One account, created locally | The GitHub part is dropped ([ADR 0024](adr/0024-proof-of-concept.md)) |
| [0021](adr/0021-slug-urls-for-companies.md) | Slug URLs for companies | |
| [0025](adr/0025-short-id-and-slug-urls-for-offers.md) | Short id and slug URLs for offers | Same scheme as applications |

## Specific to this proof of concept

These choices follow from Blazor Server and won't apply as they are to an API and a SPA.

- [ADR 0010](adr/0010-dbcontext-factory-without-repositories.md): a `DbContext` factory with one context per operation, as Blazor Server circuits need. An API usually has one scoped context per request.
- [ADR 0016](adr/0016-responsive-navigation-bottom-tabs-and-sidebar.md) and [ADR 0023](adr/0023-interactive-islands-in-static-pages.md): the navigation and the interactive islands are built with Blazor components. The navigation pattern itself can be reused.
- The Identity pages scaffolded from the template: a SPA and a desktop app need another way to authenticate.
- The external-login plumbing left by the template stays dormant, since GitHub login is out of the proof of concept.

## Open questions for the official version

- What replaces the CV template plugins, if anything.
- One way to authenticate for both the SPA and the desktop app.
- Whether the UI is translated.

## Slice log

Newest first. Each entry follows this format:

```markdown
### <date> — <slice> (#<issue>)

- Kept: what proved useful
- Not needed: what was built and turned out useless
- Change in the official version: what to do differently
```

### 2026-10-27 — Home dashboard (#37)

- Kept: the counters are links to the filtered applications list (`?status=…`), so the dashboard needs no filter of its own and no island: the whole page is static.
- Kept: Follow up counts from the sending day typed by the user (`AppliedOn`), not from when the sending was recorded: an application logged late shows at once. The rule sits in the domain (`UserSettings.LastSendingDayToFollowUp`), and the query stays in SQL.
- Kept: the follow-ups and the events reuse the application summary and its projection, so the dashboard adds no new mapping of the two kinds.
- Not needed: nothing so far. Follow up has no limit: real use will tell whether a long search, with many applications left unanswered, needs one, or a way to dismiss an item (the follow-up interaction of Phase 2b).
- Change in the official version: "today" is the server's UTC day, while `AppliedOn` is a day of the user: an API should take the user's time zone. The activity is merged in memory from the status changes and the creations, and a creation as already sent is told apart by its timestamps: an API could store an explicit event log, or the initial status, and page it.

### 2026-10-27 — Settings (#52)

- Kept: the settings are a single row created by the first save, and the defaults come from the domain (`UserSettings.DefaultFollowUpAfterDays`) while there is none, so a fresh database needs no seed.
- Kept: one page saves to two places, the database for the follow-up delay and the browser for the theme. Two cards keep them apart: only the delay has a Save button, and the theme says it is "Saved in this browser", applied at once like in the sidebar and the More sheet.
- Kept: the form is static and enhanced, and stays on the page with a toast: there is no other page to go back to.
- Not needed: nothing so far. The delay is only read by the dashboard (#37), which will tell whether the bounds (1 to 30 days) fit real use.
- Change in the official version: the single row isn't enforced by the schema. An API serving several people or devices would key the settings by user, and could store the theme with them so it follows the user across devices.

### 2026-10-27 — CV upload (#39)

- Kept: the file is named after the id of its row (`documents/<yyyy>/<id>.pdf`, GUID v7), so it is written before the row is saved, and removed if the row fails. The stored path is a key that always uses `/`, so a backup of `/data` moves between systems.
- Kept: the PDF is checked on its content (`%PDF-`), not its name or the type sent by the browser, and within the limit as it is read: the size the browser gives only saves sending a file sure to be refused.
- Kept: preview and download are two plain GET endpoints outside Blazor, behind the login, linked with `data-enhance-nav="false"`; the preview is the browser's own viewer in a new tab, without any script.
- Kept: the drop zone is a transparent `InputFile` laid over it: a drop reaches the input without JavaScript.
- Not needed: nothing so far. There is no delete on purpose, as the tab is the record of what was sent: real use will tell whether a wrong upload needs one.
- Change in the official version: `InputFile` streams the file over the SignalR circuit, and the service reads it whole in memory, within the limit, before checking it. An API would take a `multipart/form-data` upload and stream it to the storage. Android Chrome has no built-in PDF viewer, so the preview downloads the file there: a SPA could render it with PDF.js.

### 2026-10-24 — Cover letters (#38)

- Kept: the letters and their editor are one island keyed by the application. Each save or deletion refreshes the static page, so the count of the tab follows, and the island keeps the letter shown and a text being written. Without `@key`, a refresh starts a new island: the note of #30 that the island "receives its new parameters" only holds with a key, which the change status sheet didn't need.
- Kept: a cover letter is its own aggregate, pointing to its application, rather than a collection of the application: editing one doesn't load the others.
- Kept: Copy runs in a click handler of `shell.js`, which reads the text as typed and shows a toast from the template of `ToastHost`. Safari and iOS refuse a clipboard write that comes after a round trip to the server, as a Blazor JS interop call would.
- Not needed: nothing so far.
- Change in the official version: the Clipboard API needs a secure context. Served over plain HTTP on a local network (Docker), the browser falls back to `execCommand("copy")`, which is deprecated: the official version should be served over HTTPS. Switching letters or tabs drops a text not saved without warning: a SPA could keep a draft per letter or ask before leaving.

### 2026-10-23 — Archiving instead of deleting (#31)

- Kept: archive and restore are hidden static forms sent by enhanced navigation, which buttons anywhere on the page reach through their `form` attribute (⋯ menu, banner, Undo). No circuit is needed, so the buttons work at once. The toast comes from a request-scoped `ToastService`, filled by the page's handler and rendered by the layout in the same response.
- Kept: the ⋯ menu is a native `popover` anchored with CSS anchor positioning on desktop, without script, and a sheet on mobile.
- Kept: the cascade links its rows by their common `ArchivedAt` ([ADR 0027](adr/0027-archiving-cascade-and-restore.md)), so a restore brings back exactly what was archived with the row, without any extra table.
- Not needed: nothing so far.
- Change in the official version: an API needs archive and restore endpoints that run the cascade in a single transaction, and return what they changed so the SPA can offer Undo. A company typed by name in a form is still matched when it is archived (`CompanyLookup`), and the new offer or application goes to an archived company; the official version should decide whether that restores it. A Blazor static page loses an open `popover` on enhanced navigation, as it did the `<dialog>` (#51): `shell.js` hides it.

### 2026-10-22 — Applications list: filters and search (#51)

- Kept: the filters in the query string, sent by static GET forms and loaded by enhanced navigation. A filtered list is a link, which the dashboard counters will use, and no island is needed. Enhanced navigation keeps the scroll position when only the query string changes.
- Kept: the status counts follow the other criteria, so each chip says what it would show.
- Not needed: nothing so far.
- Change in the official version: the search runs in memory once the active applications are loaded, as for offers and companies, because SQLite compares neither ignoring accents nor word by word; "Load more" reloads the whole list with a larger `shown`. An API needs a searchable column (folded text, a collation or full-text search) and a cursor to page.
- Change in the official version: a static form sent from inside an open `<dialog>` must close it first. Enhanced navigation drops the `open` attribute without calling `close()`, and the page stays inert behind the dialog. A SPA closes its sheet in its own state.

### 2026-10-21 — Status change and history (#30)

- Kept: the transition table in the domain (`StatusWorkflow`, [ADR 0026](adr/0026-status-workflow-and-history.md)), with a single method that changes the status and records it. Creating an application as already sent goes through it too, so `MarkAsSent` is gone and the history is complete.
- Kept: the details DTO carries the allowed next statuses, so the UI never repeats the workflow.
- Kept: the change status sheet is an island inside a static card, and refreshes the static page once saved (`NavigationManager.Refresh()`). The page re-renders its status, sending day and history, and the island receives its new parameters, without reloading anything twice.
- Not needed: nothing so far. `StatusChangeDto.From` isn't shown yet; the dashboard's recent activity (#37) may use it.
- Change in the official version: `ChangedAt` is the time the change was recorded, not a date the user picks; an API could accept a past date for a change logged late. A mistaken status can't be undone here: the official version should decide between an undo and a way back.

### 2026-10-21 — Offers: readable URLs (#65)

- Kept: the `{ShortId}-{slug}` URL of applications (ADR 0014) fits offers as is (ADR 0025). The generator, its setting and the redirect to the canonical slug were reused; only the key helper moved to a shared place.
- Kept: links that carry an offer to another page use its short id (`?offer=482913`), so no GUID is left in the URLs.
- Not needed: nothing so far.
- Change in the official version: an offer and its applications get the same slug, built from the same title and company. A renamed offer changes the URL of every application to it; an API that returns the canonical key of each resource handles this without redirects.

### 2026-10-17 — Application detail and edit (#50)

- Kept: the `{ShortId}-{slug}` URL with a redirect to the current slug (ADR 0014). Links stay readable, and a renamed target position doesn't break them. The slug isn't stored: the DTOs compute it from the title and the company.
- Kept: one details DTO per kind, read with one query per kind. The pages switch on the type of the DTO.
- Kept: the create inputs derive from the edit inputs (`NewOfferApplicationInput : OfferApplicationInput`), as for offers, so both forms share their fields.
- Not needed: nothing so far. Until the status change (#30), the status card only shows the badge.
- Change in the official version: the redirect to the canonical URL is a 302 from Blazor's static rendering; an API returns the canonical key and lets the SPA replace the URL. A static Blazor form bound to one of two input types needs two `[SupplyParameterFromForm]` parameters with the same name and their own form name: a SPA form doesn't have this constraint.
