# UI

The screens of the MVP ([Phase 1](roadmap.md#phase-1--application-tracking-mvp)), how the layout adapts to mobile, and how later phases fit in. Colors, type and spacing come from the [design system](design-system.md). The layout decisions are in [ADR 0016](adr/0016-responsive-navigation-bottom-tabs-and-sidebar.md).

**Mockups**: [`docs/mockups/`](mockups/index.html) holds an HTML page that renders every screen at 390 px (mobile) and 1440 px (desktop), in light and dark.
- It uses the real `--ambio-*` tokens ([`ambio.css`](mockups/ambio.css)), Source Sans 3 and Bootstrap Icons.
- Links inside the screens navigate between mockups.
- To view it locally, serve the folder (`python3 -m http.server -d docs/mockups`) and open `http://localhost:8000`.
- The screens aren't in Figma: the Figma Starter plan allows 20 MCP tool calls a month, which the design system used up. They can be rebuilt in Figma's `Screens` page later from this page.

## Layout

| Width | Layout |
|---|---|
| **< 992 px** (mobile and tablet) | Top **app bar** (logo or back arrow, page title, contextual ⋯ action), content, **bottom tab bar**. A floating "New application" button (FAB) on the dashboard and the applications list. |
| **≥ 992 px** (Bootstrap `lg`) | 240 px **sidebar**, page header (title and primary actions), content at most 1200 px wide. |

- There is a single breakpoint, at 992 px. Tablets use the mobile layout.
- Touch targets are at least 44 px below 992 px. Buttons and inputs are 44 px high on mobile and 40 px on desktop.
- On mobile the page title is in the app bar; the in-page header (`page-header`) only appears on desktop. Detail pages show their own header (title, company, tags) at every width.
- Page titles use `text/h2` (22 px) on mobile and `text/h1` (28 px) on desktop.

## Navigation

| Mobile tab | Desktop sidebar | Route |
|---|---|---|
| Home | Dashboard | `/` |
| Applications | Applications | `/applications` |
| Offers | Offers | `/offers` |
| Companies | Companies | `/companies` |
| More (sheet) | Bottom of the sidebar | — |

- The sidebar groups the four sections under "Tracking". Its bottom holds Settings, the theme selector, and the account (email, log out).
- The **More** tab opens a bottom sheet with the same entries: Settings, Account, Theme, Log out.
- **Later phases add entries, never tabs.** The tab bar stays at five items:

| Phase | Added to the sidebar and the More sheet |
|---|---|
| 3 | Profile, My CVs (a "Profile & CV" group in the sidebar) |
| 4 | Templates |
| 5 | Language (in Settings) |
| 6 | Export / import |

## Patterns

**Lists** (applications, offers, companies)
- Desktop: a table in a card, with an inline filter bar (search, selects, "Show archived").
- Mobile: cards (title, company, `StatusBadge`, tags, date), a search field and a **Filters** button that opens a bottom sheet. The applications list also has horizontally scrolling status chips.
- Archived rows are hidden unless "Show archived" is on. When shown, they are dimmed and carry the `Archived` tag.
- Sorted by date, newest first. "Load more" rather than numbered pages.

**Detail pages**
- A header: breadcrumb (desktop), title, company, tags, then Edit and a ⋯ menu (Archive…).
- Sections are **tabs**, and the active tab is in the URL (`?tab=letters`), so a later phase adds a tab instead of reworking the page.
- Application: two columns on desktop, tabbed content on the left and a side panel with the status, "Change status" and the history. On mobile, the status card sits above the tabs and the history closes the Overview tab.
- Offer and company pages follow the same grid: a side column (postings and applications, or the company profile) next to the main content.

**Forms**
- One column on mobile, two on desktop. Sections with a title (`text/h3`).
- On mobile the actions are pinned to the bottom of the screen; on desktop they close the form, aligned right.
- Optional fields say "(optional)"; the others are required.
- Errors appear under the field in `feedback/danger`, with an icon, and say how to fix the value.

**Overlays**: a **bottom sheet** on mobile and a centered **modal** on desktop, from the same component: status change, filters, the More menu.

**Feedback**
- Toasts at the bottom, above the tab bar on mobile, with an action when one makes sense ("Undo").
- Banners inside the page for a state that lasts (archived item, possible duplicate): neutral background, colored icon (`feedback/info`, `feedback/warning`).
- Empty states explain what the page is for and offer the next action.

**Archiving** ([ADR 0012](adr/0012-archive-instead-of-delete.md)): "Archive" in the ⋯ menu, without confirmation, followed by a toast with **Undo**. An archived item shows a banner with **Restore**, and its status can't change until it's restored.

**Status change**: only the transitions the workflow allows are offered (`Draft → Applied → Interview → Offer / Rejected / Withdrawn`). Moving from Draft to Applied asks for the sending date (`AppliedOn`). An optional comment is stored in the `StatusChange`.

**Theme**: System (default), Light or Dark, in the sidebar, the More sheet and Settings. The choice is stored in the browser (`localStorage`) and applied as `data-bs-theme` on `<html>` before the first paint.

## Screens

| # | Screen | Route | Content | Later phases |
|---|---|---|---|---|
| 1 | Log in | `/Account/Login` | Logo, email, password, "Remember me". Registration link only while no account exists ([ADR 0002](adr/0002-single-user-application.md)). | GitHub login (5) |
| 2 | Dashboard | `/` | Pipeline bar and a counter per status (each opens the filtered list), **Follow up**, recent activity. | Upcoming interviews (2b) |
| 3 | Applications — list | `/applications` | Filters, table or cards. | — |
| 4 | Applications — empty | `/applications` | Empty state with "New application" and "Add an offer". | — |
| 5 | Application — Overview | `/applications/{ShortId}-{slug}` | Offer, posting, channel, sending date, short id, notes; status panel and history. | Interactions and Contacts tabs (2b) |
| 6 | Application — Cover letters | `…?tab=letters` | Letter list, plain-text editor, Copy and Save. | — |
| 7 | Application — CV | `…?tab=cv` | PDF upload (drop zone or file picker), sent CVs with preview and download. | Generate a CV (3) |
| 8 | Application — archived | same | Restore banner, Undo toast. | — |
| 9 | New application | `/applications/new`, `?offer={id}` to start with an offer, `?company={slug}` to start spontaneous with a company (the link of the company page) | "From an offer / Spontaneous" toggle; the Application section keeps its values when the kind changes. Offer search with a "Create a new offer" link, or company and target position. "Already sent" + date, otherwise Draft. The form is an interactive island: the offer is searched by title or company, and the company by name, as the user types (once typing stops for 300 ms, ignoring case and accents). The offer's postings fill "Applied via" once it is chosen; the company search ends with "Create the company “…”", which creates it with the application, unless a result already has the typed name. | — |
| 10 | Change status | modal / sheet | Allowed transitions with their badges, date when sending, comment. | — |
| 11 | Offers — list | `/offers` | Contract, remote policy, postings and applications counts, "Apply" when there is none. "Published" is the day the offer was recorded. | Recruiter (2b) |
| 12 | Offer — detail | `/offers/{id}`, edit at `/offers/{id}/edit` | Company, location and publication date under the title; postings (site, link, day recorded), added in a sheet and removed after a confirmation, without reloading the page (the last one stays); applications (status, sending day and site) with a "New" link to apply; plain-text copy of the ad. The edit form doesn't change the company. | — |
| 13 | New offer | `/offers/new` | Offer (company picked from the existing ones or typed to create it), first posting (site and link, no date), ad text. **Duplicate suggestion** banner after "Create offer" when the company and a similar title match: "Add this posting to this offer" adds the site and link to an existing offer and opens it, "Create a new offer anyway" creates it. The form keeps its values. | Recruiter company (2b) |
| 14 | Companies — list | `/companies` | Kind, industry, location, offers and applications counts. | — |
| 15 | Company — detail | `/companies/{slug}` | Profile, description, Offers and Applications tabs (`?tab=applications`), each with its count. The Applications tab lists the applications to its offers and the spontaneous ones; its empty state opens a spontaneous application to the company. | Notes and Contacts tabs (2b) |
| 16 | More sheet | mobile only | Settings, Account, Theme, Log out. | Profile, My CVs, Templates, Export |
| 17 | Settings | `/settings` | Follow-up delay (days), theme. | Language (5) |
| 18 | Account | `/Account/Manage` | Identity management: section list on desktop, scrolling tabs on mobile. | External logins (5) |

**Follow up** lists the applications still `Applied` after the delay set in Settings (7 days by default, `UserSettings.FollowUpAfterDays`), oldest first. An item leaves the list when its status changes. In Phase 2b, logging a follow-up interaction also resets the delay.

**Identity pages**: the template's Identity pages (password reset, 2FA, email confirmation…) render in the same shell as Account, or, for the pages shown before logging in, in the centered card of Log in. They keep their content and get the design system through the Bootstrap mapping. Passkey pages are removed in Phase 0.

## Components

What the mockups are built from, and the Blazor component each becomes. Stock Bootstrap classes (`btn`, `form-control`, `form-select`, `card`, `table`) are kept wherever they exist.

| UI element | Mockup class | Future component |
|---|---|---|
| App shell | `app`, `sidebar`, `appbar`, `tabbar`, `fab` | `MainLayout.razor`, `Sidebar.razor`, `AppBar.razor` (with `AppBarTitle`, `AppBarActions` and the back arrow `AppBarBack`), `TabBar.razor` |
| Logo | `logo`, `logo-mark` | `Logo.razor` |
| Page header | `page-header` | `PageHeader.razor` |
| Detail header | `detail-head`, `breadcrumb` | `DetailHeader.razor` (title, subtitle, tags, actions) |
| Link to another site | `box-arrow-up-right` icon | `ExternalLink.razor` (new tab, short text) |
| Floating action button | `fab` | `ButtonLink` with `btn-primary btn-fab mobile-only` and an `aria-label` (applications list, then dashboard) |
| Search as you type | — | `Shared/SearchField.razor` (generic: waits 300 ms after the last keystroke, results as buttons, optional empty line and last line), inside an interactive component; wrapped by `Offers/Shared/OfferSearch.razor` and `Companies/Shared/CompanySearch.razor` |
| Status badge | `status-badge status-{status}` | `JobApplications/Shared/StatusBadge.razor` |
| Tag | `tag`, `tag-accent`, `tag-archived` | `Tag.razor` (`TagVariant.Neutral` or `Accent`); `JobApplications/Shared/SpontaneousTag.razor` for spontaneous applications |
| Filter chip | `chip` | `FilterChip.razor` |
| List card (mobile) / table row (desktop) | `app-card` / `table` | `ResponsiveList.razor` (cards below 992 px, table above) and `ListTable.razor` (header, rows, `num` cells), filled by each feature's card and table: `CompanyCard.razor` / `CompanyTable.razor`, `OfferCard.razor` / `OfferTable.razor`, `JobApplicationCard.razor` / `JobApplicationTable.razor`. Each card is a `ListCard.razor` (title, aside such as a date, subtitle, tags; a link with `Href`) |
| Tabs | `tabs`, `tab` | `TabNav.razor` (links carrying `?tab=`) |
| Count | `n`, `card-sub` | `CountBadge.razor` (tab counts, postings of an offer) |
| Status panel and history | `status-panel`, `timeline` | `StatusPanel.razor`, `StatusTimeline.razor` |
| Bottom sheet / modal | `overlay`, `dialog` | `Sheet.razor` (optional title with a close button; `sheet-content` and `sheet-actions` blocks; `CloseAsync()` from an interactive component) |
| Toast | `toast` | `ToastHost.razor` + a scoped `ToastService` |
| Banner | `banner` | `Banner.razor` (icon, title, text, actions; `Neutral` or `Info` icon color) |
| Empty state | `empty` | `EmptyState.razor` |
| Segmented control | `segmented` | `SegmentedControl.razor` (theme, read by `shell.js`; application kind, bound with `@bind-Value` and `Disabled` until the circuit) |
| File drop zone | `dropzone` | `PdfUpload.razor` (`InputFile`) |
| Dashboard | `pipeline-bar`, `stat` | `PipelineCard.razor`, `FollowUpCard.razor`, `ActivityFeed.razor` |

In the app, the container query of the mockups (`@container app (min-width: 992px)`) becomes a media query (`@media (min-width: 992px)`), and the tokens move from `.pf` to `:root` and `[data-bs-theme="dark"]`.

### Global classes

Layout and text patterns repeated across components are global classes. The markup keeps its own class next to them (`offer-tags cluster`), and the scoped CSS only keeps what is specific to the component (padding, borders, alignment).

| Class | File | Effect |
|---|---|---|
| `cluster` | `css/layout.css` | A row that wraps, with centred items: tag rows, action rows |
| `stack` | `css/layout.css` | A column |
| `--gap` | `css/layout.css` | The gap of `cluster` and `stack`, 8 px by default, set with a spacing token (`--gap: var(--ambio-space-16)`). Declared with `@property` and `inherits: false`, so a nested stack keeps its own gap |
| `mobile-only` / `desktop-only` | `css/layout.css` | Hidden from 992 px / below 992 px |
| `list-divided` | `css/lists.css` | A list without bullets or padding, with a border between items |
| `detail-layout`, `detail-aside`, `detail-main` | `css/layout.css` | The grid of a detail page: header, side column, then content below 992 px; content and a 320 px side column above |
| `prose` | `css/typography.css` | Plain text typed or pasted by the user (description, ad copy): line breaks kept, long words wrapped |

`app.css` resets the margins of headings, paragraphs, lists and `figure`/`blockquote`.
