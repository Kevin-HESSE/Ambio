# UI

The screens of the MVP ([Phase 1](roadmap.md#phase-1--application-tracking-mvp)), how the layout adapts to mobile, and how later phases fit in. Colors, type and spacing come from the [design system](design-system.md). The layout decisions are in [ADR 0016](adr/0016-responsive-navigation-bottom-tabs-and-sidebar.md).

**Mockups**: [`docs/mockups/`](mockups/index.html) holds an HTML page that renders every screen at 390 px (mobile) and 1440 px (desktop), in light and dark.
- It uses the real `--postulo-*` tokens ([`postulo.css`](mockups/postulo.css)), Source Sans 3 and Bootstrap Icons.
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
| 1 | Log in | `/Account/Login` | Logo, email, password, "Remember me". No registration link ([ADR 0002](adr/0002-single-user-application.md)). | GitHub login (5) |
| 2 | Dashboard | `/` | Pipeline bar and a counter per status (each opens the filtered list), **Follow up**, recent activity. | Upcoming interviews (2b) |
| 3 | Applications — list | `/applications` | Filters, table or cards. | — |
| 4 | Applications — empty | `/applications` | Empty state with "New application" and "Add an offer". | — |
| 5 | Application — Overview | `/applications/{ShortId}-{slug}` | Offer, posting, channel, sending date, short id, notes; status panel and history. | Interactions and Contacts tabs (2b) |
| 6 | Application — Cover letters | `…?tab=letters` | Letter list, plain-text editor, Copy and Save. | — |
| 7 | Application — CV | `…?tab=cv` | PDF upload (drop zone or file picker), sent CVs with preview and download. | Generate a CV (3) |
| 8 | Application — archived | same | Restore banner, Undo toast. | — |
| 9 | New application | `/applications/new` | "From an offer / Spontaneous" toggle. Offer search with inline creation, or company and target position. "Already sent" + date, otherwise Draft. | — |
| 10 | Change status | modal / sheet | Allowed transitions with their badges, date when sending, comment. | — |
| 11 | Offers — list | `/offers` | Contract, remote policy, postings and applications counts, "Apply" when there is none. | Recruiter (2b) |
| 12 | Offer — detail | `/offers/{id}` | Postings (site, link, seen on), applications, plain-text copy of the ad. | — |
| 13 | New offer | `/offers/new` | Offer, first posting, ad text. **Duplicate suggestion** banner when the company and a similar title match. | Recruiter company (2b) |
| 14 | Companies — list | `/companies` | Kind, industry, location, offers and applications counts. | — |
| 15 | Company — detail | `/companies/{id}` | Profile, description, Offers and Applications tabs. | Notes and Contacts tabs (2b) |
| 16 | More sheet | mobile only | Settings, Account, Theme, Log out. | Profile, My CVs, Templates, Export |
| 17 | Settings | `/settings` | Follow-up delay (days), theme. | Language (5) |
| 18 | Account | `/Account/Manage` | Identity management: section list on desktop, scrolling tabs on mobile. | External logins (5) |

**Follow up** lists the applications still `Applied` after the delay set in Settings (7 days by default, `UserSettings.FollowUpAfterDays`), oldest first. An item leaves the list when its status changes. In Phase 2b, logging a follow-up interaction also resets the delay.

**Identity pages**: the template's Identity pages (password reset, 2FA, email confirmation…) render in the same shell as Account, or, for the pages shown before logging in, in the centered card of Log in. They keep their content and get the design system through the Bootstrap mapping. Passkey pages are removed in Phase 0.

## Components

What the mockups are built from, and the Blazor component each becomes. Stock Bootstrap classes (`btn`, `form-control`, `form-select`, `card`, `table`) are kept wherever they exist.

| UI element | Mockup class | Future component |
|---|---|---|
| App shell | `app`, `sidebar`, `appbar`, `tabbar`, `fab` | `MainLayout.razor`, `Sidebar.razor`, `AppBar.razor`, `TabBar.razor` |
| Logo | `logo`, `logo-mark` | `Logo.razor` |
| Page header | `page-header`, `breadcrumb` | `PageHeader.razor` |
| Status badge | `status-badge status-{status}` | `StatusBadge.razor` |
| Tag | `tag`, `tag-accent`, `tag-archived` | `Tag.razor` |
| Filter chip | `chip` | `FilterChip.razor` |
| List card (mobile) / table row (desktop) | `app-card` / `table` | `ApplicationCard.razor`, `ApplicationTable.razor` (same for offers and companies) |
| Tabs | `tabs`, `tab` | `TabNav.razor` (links carrying `?tab=`) |
| Status panel and history | `status-panel`, `timeline` | `StatusPanel.razor`, `StatusTimeline.razor` |
| Bottom sheet / modal | `overlay`, `dialog` | `Sheet.razor` |
| Toast | `toast` | `ToastHost.razor` + a scoped `ToastService` |
| Banner | `banner` | `Banner.razor` |
| Empty state | `empty` | `EmptyState.razor` |
| Segmented control | `segmented` | `SegmentedControl.razor` (theme, application kind) |
| File drop zone | `dropzone` | `PdfUpload.razor` (`InputFile`) |
| Dashboard | `pipeline-bar`, `stat` | `PipelineCard.razor`, `FollowUpCard.razor`, `ActivityFeed.razor` |

In the app, the container query of the mockups (`@container app (min-width: 992px)`) becomes a media query (`@media (min-width: 992px)`), and the tokens move from `.pf` to `:root` and `[data-bs-theme="dark"]`.
