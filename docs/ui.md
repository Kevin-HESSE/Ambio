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
- The **More** tab opens a bottom sheet with the same entries: Settings, Account, Theme, Log out. Settings and Account show only once logged in, and the More tab is active on their pages.
- **Later phases add entries, never tabs.** The tab bar stays at five items:

| Phase | Added to the sidebar and the More sheet |
|---|---|
| 3 | Profile, My CVs (a "Profile & CV" group in the sidebar) |
| 4 | Export |

## Patterns

**Lists** (applications, offers, companies)
- Desktop: a table in a card, with an inline filter bar (search, selects, "Show archived").
- Mobile: cards (title, company, `StatusBadge`, tags, date), a search field and a **Filters** button that opens a bottom sheet. The applications list also has horizontally scrolling status chips.
- Archived rows are hidden unless "Show archived" is on (`?archived=true`). When shown, their title is dimmed (`t-muted`) and they carry the `Archived` tag after their other tags. The applications list has the box at the end of its bar and in its Filters sheet; the offers and companies lists, which have no other filter yet, show it alone above the list at every width. When every row is archived, the list keeps the box and says so, with "Show archived" and the New action.
- Sorted by date, newest first. "Load more" rather than numbered pages.
- The filters live in the query string, sent by static GET forms (`data-enhance`) whose result is loaded by enhanced navigation, so a filtered list can be linked to. The empty fields stay out of the URL, and a select or a checkbox of a `data-auto-submit` form applies as soon as it changes (`wwwroot/js/shell.js`).

**Detail pages**
- A header: breadcrumb (desktop), title, company, tags, then Edit and a ⋯ menu (Archive or Restore). On desktop the menu is a native `popover` anchored under its button; on mobile, a ⋯ button next to the pencil of the app bar opens it in a sheet.
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
- Toasts at the bottom, above the tab bar on mobile and centered on desktop, with an action when one makes sense ("Undo"). They fade out after 8 s, paused while hovered or focused. Most come from the server with the response of a static form; the browser shows its own, such as after a copy, from the same template.
- Banners inside the page for a state that lasts (archived item, possible duplicate): neutral background, colored icon (`feedback/info`, `feedback/warning`).
- Empty states explain what the page is for and offer the next action.

**Archiving** ([ADR 0012](adr/0012-archive-instead-of-delete.md), [ADR 0027](adr/0027-archiving-cascade-and-restore.md)): "Archive" in the ⋯ menu, without confirmation, followed by a toast with **Undo**. An archived item shows a banner with the day it was archived and **Restore**, the `Archived` tag in its header, and Restore in its ⋯ menu. An archived application's status can't change until it's restored: its status card offers "Restore to change status". Archiving a company or an offer archives what depends on it; an archived company or offer lists its archived offers and applications too, dimmed. Archive, Restore and Undo are static forms, without circuit.

**Status change**: only the transitions the workflow allows are offered ([data-model.md](data-model.md#application-hierarchy)), as radio cards with their badge and what they mean ("Sent to the employer", "You dropped it"…). Moving from Draft to Applied asks for the sending date (`AppliedOn`). An optional comment is stored in the `StatusChange`. A final status (Rejected, Withdrawn) has no "Change status" button. Once saved, the sheet closes and the page is refreshed: status, sending date and history.

**Theme**: System (default), Light or Dark, in the sidebar, the More sheet and Settings. The choice is stored in the browser (`localStorage`) and applied as `data-bs-theme` on `<html>` before the first paint.

## Screens

| # | Screen | Route | Content | Later phases |
|---|---|---|---|---|
| 1 | Log in | `/Account/Login` | The logo above a centered card: email, password with "Forgot your password?" on the right of its label, "Remember me", Log in. Under the card, "Ambio has a single account. Registration is closed." The account is created at first start ([ADR 0028](adr/0028-account-created-at-first-start.md)). | — |
| 2 | Dashboard | `/` | The day under the title and "New application" (the FAB on mobile). **Pipeline**: the count of active applications, a bar with a segment per status that has any, as wide as its count, and a counter per status, which opens the list filtered on it (`JobApplicationRoutes.Filtered`). **Follow up**, with its count and the days each application has waited. **Recent activity**: the 5 latest events of the active applications, newest first: "was created as a draft", "was sent", "moved to …", each title linking to its application; one created as already sent only shows its sending. Without any active application, an empty state with "New application" and "Add an offer". | Upcoming interviews (2b) |
| 3 | Applications — list | `/applications`, filtered with `?q=…&status=…&company={slug}&kind=offer\|spontaneous&archived=true` | Search on the title and the company (every word, ignoring case and accents), status, company, kind and "Show archived"; table or cards, 25 at first and 25 more with each "Load more" (`?shown=50`). Desktop: inline bar and "Clear filters". Mobile: search, a Filters sheet (company, kind, "Show archived", with their count on its button) and status chips counting what each would show with the other criteria. A filter without result keeps the filters and offers to clear them. The dashboard counters link to `?status=…` (`JobApplicationRoutes.Filtered`). | — |
| 4 | Applications — empty | `/applications` | Empty state with "New application" and "Add an offer". | — |
| 5 | Application — Overview | `/applications/{ShortId}-{slug}`, edit at `/applications/{ShortId}-{slug}/edit` | Company and location (the offer's, or the company's for a spontaneous application) under the title; the offer's contract, remote policy and salary as tags, or the Spontaneous tag. Offer, posting, channel, sending date, short id, creation date, notes; status panel (status, "since" the last change or the creation, "Change status") and history (changes newest first, with their comment, closed by the creation as Draft). Any other slug, or none, redirects to the current URL. Opened from the applications list, the offer page and the Applications tab of the company page. Edit (header, app bar, Notes card) changes the posting used (offer) or the target position (spontaneous, which changes the slug), the channel, the sending date once sent, and the notes; neither the offer, the company nor the status. | Interactions and Contacts tabs (2b) |
| 6 | Application — Cover letters | `…?tab=letters` | The Overview and Cover letters tabs carry the count of letters. Letter list (title, day last edited) in the order written, above the editor on mobile and on its left on desktop, with "New"; the editor (title, text, count of characters and day saved) opens on the first letter, or on a new one when there is none, and nothing is created before Save. Copy copies the text as typed and shows a toast; the trash button deletes the letter after a confirmation. Outside the Overview tab, the history only shows on desktop. | — |
| 7 | Application — CV | `…?tab=cv` | The CV tab carries the count of CVs. A drop zone: a PDF dropped on it or picked with "Choose a file" is sent at once, "PDF only, 5 MB max" with the configured limit; a file over the limit or that isn't a PDF shows its error under the zone. Then the sent CVs, newest first (name, size, day uploaded), each with Preview, which opens the PDF in a new tab, and Download, under its uploaded name. No list while there is none. | Generate a CV (3) |
| 8 | Application — archived | same | Right after "Archive": the toast with Undo. Banner with the day and Restore, `Archived` tag, Restore in the ⋯ menu, "Restore to change status" in the status card. | — |
| 9 | New application | `/applications/new`, `?offer={ShortId}` to start with an offer, `?company={slug}` to start spontaneous with a company (the link of the company page) | "From an offer / Spontaneous" toggle; the Application section keeps its values when the kind changes. Offer search with a "Create a new offer" link, or company and target position. "Already sent" + date, otherwise Draft. The form is an interactive island: the offer is searched by title or company, and the company by name, as the user types (once typing stops for 300 ms, ignoring case and accents). The offer's postings fill "Applied via" once it is chosen; the company search ends with "Create the company “…”", which creates it with the application, unless a result already has the typed name. "Create application" opens the new application's page. | — |
| 10 | Change status | modal / sheet | Current status, allowed transitions as radio cards with their badges and hints, "Sent on" when sending a draft, optional comment. An interactive island in the status panel: errors stay in the sheet, and saving refreshes the static page. | — |
| 11 | Offers — list | `/offers`, `?archived=true` with the archived ones | Contract, remote policy, postings and applications counts, "Apply" when there is none. "Published" is the day the offer was recorded. "Show archived" above the list. | Recruiter (2b) |
| 12 | Offer — detail | `/offers/{ShortId}-{slug}`, edit at `/offers/{ShortId}-{slug}/edit`; an old slug redirects to the current one | Company, location and publication date under the title; postings (site, link, day recorded), added in a sheet and removed after a confirmation, without reloading the page (the last one stays); applications (status, sending day and site) with a "New" link to apply; plain-text copy of the ad. The edit form doesn't change the company. | — |
| 13 | New offer | `/offers/new` | Offer (company picked from the existing ones or typed to create it), first posting (site and link, no date), ad text. **Duplicate suggestion** banner after "Create offer" when the company and a similar title match: "Add this posting to this offer" adds the site and link to an existing offer and opens it, "Create a new offer anyway" creates it. The form keeps its values. | Recruiter company (2b) |
| 14 | Companies — list | `/companies`, `?archived=true` with the archived ones | Kind, industry, location, offers and applications counts. "Show archived" above the list. | — |
| 15 | Company — detail | `/companies/{slug}` | Profile, description, Offers and Applications tabs (`?tab=applications`), each with its count. The Applications tab lists the applications to its offers and the spontaneous ones; its empty state opens a spontaneous application to the company. | Notes and Contacts tabs (2b) |
| 16 | More sheet | mobile only | Settings, Account, Theme, Log out. | Profile, My CVs, Export |
| 17 | Settings | `/settings` | Two cards. **Follow-ups**: the follow-up delay, from 1 to 30 days (7 until first saved), and "Save settings", a static form that stays on the page with a toast; a value out of bounds shows its error under the field. **Appearance**: the theme selector, applied at once and kept by the browser, outside the form. No back arrow on mobile, as for Account. | — |
| 18 | Account | `/Account/Manage` | Identity management in the app shell, under the header "Account": the sections (Profile, Email, Password, Two-factor authentication, Personal data) as scrolling tabs on mobile and a list on the left on desktop, and the page of the section in a card. No back arrow on mobile. | — |

**Follow up** lists the applications still `Applied` after the delay set in Settings (7 days by default, `UserSettings.FollowUpAfterDays`), oldest first. The delay counts from the sending day (`AppliedOn`): an application sent 7 days ago or more shows, whatever the day it was recorded. An item leaves the list when its status changes. In Phase 2b, logging a follow-up interaction also resets the delay.

**Identity pages**: the template's Identity pages (password reset, 2FA, email confirmation…) render in the same shell as Account (`ManageLayout`, the pages of `Account/Pages/Manage`), or, for the pages shown before logging in, in the centered card of Log in (`AuthLayout`, every other page of `Account/Pages`). They keep their content, floating labels included, without the template's grid, and get the design system through the Bootstrap mapping. Passkey pages are removed in Phase 0.

## Components

What the mockups are built from, and the Blazor component each becomes. Stock Bootstrap classes (`btn`, `form-control`, `form-select`, `card`, `table`) are kept wherever they exist.

| UI element | Mockup class | Future component |
|---|---|---|
| App shell | `app`, `sidebar`, `appbar`, `tabbar`, `fab` | `MainLayout.razor`, `Sidebar.razor`, `AppBar.razor` (with `AppBarTitle`, `AppBarActions` and the back arrow `AppBarBack`), `TabBar.razor` |
| Logo | `logo`, `logo-mark` | `Logo.razor` |
| Page header | `page-header` | `PageHeader.razor` |
| Detail header | `detail-head`, `breadcrumb` | `DetailHeader.razor` (title, subtitle, tags, actions, and the items of the ⋯ menu as `Menu`, rendered in an `ActionMenu.razor` popover on desktop and in a sheet opened by `DetailMenuButton.razor` in the app bar on mobile) |
| Link to another site | `box-arrow-up-right` icon | `ExternalLink.razor` (new tab, short text) |
| Floating action button | `fab` | `ButtonLink` with `btn-primary btn-fab mobile-only` and an `aria-label` (applications list and dashboard) |
| Search as you type | — | `Shared/SearchField.razor` (generic: waits 300 ms after the last keystroke, results as buttons, optional empty line and last line), inside an interactive component; wrapped by `Offers/Shared/OfferSearch.razor` and `Companies/Shared/CompanySearch.razor` |
| Status badge | `status-badge status-{status}` | `JobApplications/Shared/StatusBadge.razor` |
| Tag | `tag`, `tag-accent`, `tag-archived` | `Tag.razor` (`TagVariant.Neutral`, `Accent` or `Archived`); `JobApplications/Shared/SpontaneousTag.razor` for spontaneous applications, `ArchivedTag.razor` for archived rows and items |
| Show archived | `check` | `ShowArchived.razor` (the checkbox, `?archived=true`), alone in `ArchivedFilter.razor` for the offers and companies lists |
| Archiving | `banner`, `toast` | `ArchiveForms.razor` (the hidden Archive and Restore forms of a detail page, and its Undo toast), `ArchiveMenuItem.razor` (Archive or Restore in the ⋯ menu), `ArchivedBanner.razor` |
| Filter chip | `chip` | `FilterChip.razor` (a link with an optional icon, its label and count, `aria-current` when active; a `status-*` class tints the icon) |
| List card (mobile) / table row (desktop) | `app-card` / `table` | `ResponsiveList.razor` (cards below 992 px, table above) and `ListTable.razor` (header, rows, `num` cells), filled by each feature's card and table: `CompanyCard.razor` / `CompanyTable.razor`, `OfferCard.razor` / `OfferTable.razor`, `JobApplicationCard.razor` / `JobApplicationTable.razor`. Each card is a `ListCard.razor` (title, aside such as a date, subtitle, tags; a link with `Href`). An optional `Footer` closes both, such as `ListFooter.razor` ("Showing 25 of 61" and "Load more") |
| Tabs | `tabs`, `tab` | `TabNav.razor` (links carrying `?tab=`, or one route per section; with `Sections`, a list of sections from 992 px, as on the Account pages) |
| Log-in card | `auth` | `Account/Shared/AuthLayout.razor` (the logo, the page in a card, and a line under it given with a `SectionContent` named `AuthLayout.FootSection`) |
| Account | `manage` | `Account/Shared/ManageLayout.razor` (page header, sections, the page in a card) and `ManageNavMenu.razor` (the sections, a `TabNav` with `Sections`) |
| Count | `n`, `card-sub` | `CountBadge.razor` (tab counts, postings of an offer) |
| Status panel and history | `status-panel`, `timeline` | `JobApplications/Shared/StatusPanel.razor` (badge, "since", and the action as child content), `ChangeStatusSheet.razor` (interactive island: the "Change status" button and its sheet), `StatusTimeline.razor` (the History card) |
| Bottom sheet / modal | `overlay`, `dialog` | `Sheet.razor` (optional title with a close button; `sheet-content` and `sheet-actions` blocks; `CloseAsync()` from an interactive component) |
| Toast | `toast` | `Layout/ToastHost.razor` in the layout, filled by the request-scoped `ToastService` (`Show(new Toast(message, icon, action))`); the action sends a form of the page by its id. Its `<template id="toast-template">` is the toast shown by the browser (`wwwroot/js/shell.js`) |
| Cover letters | `letters`, `letter-list`, `editor` | `CoverLetters/Shared/CoverLetterEditor.razor` (interactive island, keyed by the application: the list, the editor and the delete sheet), whose Copy button is a `data-copy` button handled by `shell.js` |
| Banner | `banner` | `Banner.razor` (icon, title, text, actions; `Neutral` or `Info` icon color) |
| Empty state | `empty` | `EmptyState.razor` |
| Segmented control | `segmented` | `SegmentedControl.razor` (theme, read by `shell.js`; application kind, bound with `@bind-Value` and `Disabled` until the circuit) |
| File drop zone | `dropzone` | `CvDocuments/Shared/PdfUpload.razor` (interactive island: a transparent `InputFile` laid over the zone, so a drop needs no script; it refreshes the page once the CV is stored) |
| Sent CVs | `list`, `list-item` | `CvDocuments/Shared/CvDocumentList.razor` (static; Preview and Download are links with `data-enhance-nav="false"`) |
| Dashboard | `pipeline-bar`, `stat` | `Dashboard/Shared/PipelineCard.razor` (bar and counters), `FollowUpCard.razor` (a `ListCard` per application, the days waited as its aside), `ActivityFeed.razor`, laid out by `Pages/Home.razor` |

In the app, the container query of the mockups (`@container app (min-width: 992px)`) becomes a media query (`@media (min-width: 992px)`), and the tokens move from `.pf` to `:root` and `[data-bs-theme="dark"]`.

### Global classes

Layout and text patterns repeated across components are global classes. The markup keeps its own class next to them (`offer-tags cluster`), and the scoped CSS only keeps what is specific to the component (padding, borders, alignment).

| Class | File | Effect |
|---|---|---|
| `cluster` | `css/layout.css` | A row that wraps, with centred items: tag rows, action rows |
| `stack` | `css/layout.css` | A column |
| `--gap` | `css/layout.css` | The gap of `cluster` and `stack`, 8 px by default, set with a spacing token (`--gap: var(--ambio-space-16)`). Declared with `@property` and `inherits: false`, so a nested stack keeps its own gap |
| `mobile-only` / `desktop-only` | `css/layout.css` | Hidden from 992 px / below 992 px |
| `tabs`, `tab` | `css/navigation.css` | Tabs: a row of links that scrolls, the active one underlined |
| `tabs-sections` | `css/navigation.css` | Section tabs (`TabNav` with `Sections`): tabs below 992 px, a list of `nav-item`s above |
| `nav-item` | `css/navigation.css` | An item of a navigation list from 992 px: the sidebar, the section tabs |
| `list-divided` | `css/lists.css` | A list without bullets or padding, with a border between items |
| `list-title` | `css/lists.css` | The title of a list table row, linking to its page: primary text, accent and underlined on hover, dimmed with `t-muted` when archived |
| `menu-list`, `menu-item`, `menu-item-danger` | `css/menus.css` | A list of actions, links or buttons, in the More sheet and the ⋯ menu |
| `detail-layout`, `detail-aside`, `detail-main` | `css/layout.css` | The grid of a detail page: header, side column, then content below 992 px; content and a 320 px side column above |
| `status-draft`, `status-applied`… | `css/statuses.css` | The colors of a status (`--status-bg`, `--status-fg`, `--status-border`, `--status-solid`), read by `StatusBadge`, the status `FilterChip`s and the dots of the history |
| `prose` | `css/typography.css` | Plain text typed or pasted by the user (description, ad copy): line breaks kept, long words wrapped |

`app.css` resets the margins of headings, paragraphs, lists and `figure`/`blockquote`.
