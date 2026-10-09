# Design system

Ambio's visual identity: neutral grays with a single indigo accent. The only other hues mark application statuses. The reasons are in [ADR 0015](adr/0015-neutral-visual-identity-with-status-colors.md).

> Status: the tokens exist in Figma and are applied to the app in `src/Ambio.Web/wwwroot/app.css` (CSS variables mapped onto Bootstrap 5.3). The theme follows the system unless Light or Dark is chosen with the theme selector of the app shell (sidebar and More sheet), stored in the browser under `ambio-theme`.

**Figma**: [Ambio](https://www.figma.com/design/3ys0FLvxXEynaZmFtUxAda/Ambio) has two pages:
- `Foundations`: primitives, semantic colors, statuses, tags, typography, spacing and radius.
- `Components`: `StatusBadge`, `Tag`, and an application-list preview in light and dark.

The screens are specified in [ui.md](ui.md) and mocked up in HTML in [`mockups/`](mockups/index.html), with the same tokens. A third Figma page, `Screens`, is planned for them.

## Principles

- **Neutral by default.** Surfaces, text and borders are cool grays. Indigo is kept for actions: primary buttons, links and focus.
- **Hue carries meaning.** The six application statuses are the only colored elements. Every other attribute (channel, contract type, company kind…) is a neutral tag, so a list of applications doesn't turn into a rainbow.
- **Color is never the only signal.** A status badge always shows its label and, in the app, an icon. Draft and Withdrawn share the gray hue and are told apart by their style (dashed and empty, or filled).
- **Accessible contrast.** Every text color reaches at least **4.5:1** against its background, and every status dot at least **3:1** (WCAG 2.2 AA), in both themes.
- **Light and dark.** Every semantic token has a light and a dark value.

## Colors

### Primitives

Raw values. Designs never use them directly: they only feed the semantic tokens.

| Step | gray | indigo | blue | amber | green | red |
|---|---|---|---|---|---|---|
| 50 | `#F8FAFC` | `#EEF2FF` | `#EFF6FF` | `#FFFBEB` | `#ECFDF5` | `#FEF2F2` |
| 100 | `#F1F5F9` | | | | | |
| 200 | `#E2E8F0` | `#C7D2FE` | `#BFDBFE` | `#FDE68A` | `#A7F3D0` | `#FECACA` |
| 300 | `#CBD5E1` | `#A5B4FC` | `#93C5FD` | `#FCD34D` | `#6EE7B7` | `#FCA5A5` |
| 400 | `#94A3B8` | `#818CF8` | | | | |
| 500 | `#64748B` | `#6366F1` | `#3B82F6` | `#F59E0B` | `#10B981` | `#EF4444` |
| 600 | `#475569` | `#4F46E5` | `#2563EB` | `#D97706` | `#059669` | `#DC2626` |
| 700 | `#334155` | `#4338CA` | `#1D4ED8` | | | `#B91C1C` |
| 800 | `#1E293B` | `#3730A3` | `#1E40AF` | `#92400E` | `#065F46` | `#991B1B` |
| 900 | `#0F172A` | | | | | |
| 950 | `#020617` | `#1E1B4B` | `#172554` | `#451A03` | `#022C22` | `#450A0A` |

White is `#FFFFFF`.

### Semantic tokens

| Token | Usage | Light | Dark |
|---|---|---|---|
| `bg/canvas` | Page background | `#F8FAFC` | `#020617` |
| `bg/surface` | Cards, tables, inputs | `#FFFFFF` | `#0F172A` |
| `bg/subtle` | Hover, table header | `#F1F5F9` | `#1E293B` |
| `border/default` | Dividers, card borders | `#E2E8F0` | `#1E293B` |
| `border/strong` | Inputs, emphasized borders | `#CBD5E1` | `#334155` |
| `text/primary` | Body text, titles | `#0F172A` | `#F8FAFC` |
| `text/secondary` | Supporting text | `#475569` | `#CBD5E1` |
| `text/muted` | Metadata, archived rows | `#64748B` | `#94A3B8` |
| `accent/default` | Primary button | `#4F46E5` | `#818CF8` |
| `accent/hover` | Primary button hover | `#4338CA` | `#A5B4FC` |
| `accent/on` | Text on the accent | `#FFFFFF` | `#020617` |
| `accent/subtle` | Selected row, accent background | `#EEF2FF` | `#1E1B4B` |
| `accent/text` | Links | `#4338CA` | `#A5B4FC` |
| `focus/ring` | Focus outline | `#6366F1` | `#818CF8` |
| `feedback/success` | Success alerts and toasts | `#059669` | `#10B981` |
| `feedback/warning` | Warnings | `#D97706` | `#F59E0B` |
| `feedback/danger` | Errors, destructive actions | `#DC2626` | `#EF4444` |
| `feedback/info` | Information | `#2563EB` | `#3B82F6` |

In dark mode the accent gets lighter, and the text on it turns dark (`accent/on`) to keep its contrast.

### Application statuses

Each status of `ApplicationStatus` ([data model](data-model.md#application-hierarchy)) has four tokens:
- `bg`: badge background;
- `fg`: label;
- `border`: badge border;
- `solid`: the dot in the badge, and dashboard charts.

| Status | Label | Label (FR) | Hue | Light `bg` / `fg` / `border` / `solid` | Dark `bg` / `fg` / `border` / `solid` | Icon |
|---|---|---|---|---|---|---|
| `Draft` | Draft | Brouillon | gray, **dashed** border | `#FFFFFF` / `#475569` / `#CBD5E1` / `#64748B` | `#0F172A` / `#CBD5E1` / `#475569` / `#64748B` | `bi-pencil` |
| `Applied` | Applied | Envoyée | blue | `#EFF6FF` / `#1D4ED8` / `#BFDBFE` / `#2563EB` | `#172554` / `#93C5FD` / `#1E40AF` / `#3B82F6` | `bi-send` |
| `Interview` | Interview | Entretien | amber | `#FFFBEB` / `#92400E` / `#FDE68A` / `#D97706` | `#451A03` / `#FCD34D` / `#92400E` / `#F59E0B` | `bi-people` |
| `Offer` | Offer received | Offre reçue | green | `#ECFDF5` / `#065F46` / `#A7F3D0` / `#059669` | `#022C22` / `#6EE7B7` / `#065F46` / `#10B981` | `bi-trophy` |
| `Rejected` | Rejected | Refusée | red | `#FEF2F2` / `#B91C1C` / `#FECACA` / `#DC2626` | `#450A0A` / `#FCA5A5` / `#991B1B` / `#EF4444` | `bi-x-circle` |
| `Withdrawn` | Withdrawn | Abandonnée | gray, filled | `#F1F5F9` / `#475569` / `#E2E8F0` / `#475569` | `#1E293B` / `#CBD5E1` / `#334155` / `#94A3B8` | `bi-slash-circle` |

- The hues follow the workflow `Draft → Applied → Interview → Offer / Rejected / Withdrawn`:
  - gray for what isn't sent yet or was dropped;
  - blue for "waiting";
  - amber for "in progress, prepare";
  - green for success;
  - red for a refusal.
- **Draft** vs **Withdrawn**: both are gray. Draft is empty with a dashed border ("not sent yet"). Withdrawn is filled ("closed by me").
- The **indigo accent is never used for a status**, so a badge can't be mistaken for a button or a link.
- The `feedback/*` tokens share these hues (success = Offer, warning = Interview, danger = Rejected, info = Applied), so each color means the same thing everywhere in the app.
- The UI and the mockups use the English labels. The French labels come with localization ([Phase 5](roadmap.md#phase-5--identity--i18n)). The Figma components still show the French labels and will be switched to English.
- Icons come from [Bootstrap Icons](https://icons.getbootstrap.com/) (MIT), self-hosted in `wwwroot/lib/bootstrap-icons/` as an icon font (`<i class="bi bi-send"></i>`). Figma shows a dot instead of the icon.

### Tags

Every attribute that isn't a status is a **neutral tag**. The icon distinguishes the attributes, not the color.

| Tag | Used for | Light `bg` / `fg` / `border` | Dark `bg` / `fg` / `border` |
|---|---|---|---|
| `tag/neutral` | `Offer` kind, `Channel`, `Company.Kind`, `ContractType`, `RemotePolicy`, `Interaction.Kind` | `#F1F5F9` / `#475569` / `#E2E8F0` | `#1E293B` / `#CBD5E1` / `#334155` |
| `tag/accent` | `Spontaneous` applications only | `#EEF2FF` / `#4338CA` / `#C7D2FE` | `#1E1B4B` / `#A5B4FC` / `#3730A3` |
| `tag/archived` | Archived rows ([ADR 0012](adr/0012-archive-instead-of-delete.md)) | `#FFFFFF` / `#64748B` / `#CBD5E1` | `#0F172A` / `#94A3B8` / `#334155` |

- A spontaneous application is the only tag in the accent color, because it changes how the row reads: there is no offer behind it.
- An archived row is dimmed: its title uses `text/muted`, and it carries the `Archived` tag.
- On the dashboard, an upcoming interaction (interview, call) reuses the `Interview` status tokens.

## Typography

**[Source Sans 3](https://fonts.google.com/specimen/Source+Sans+3)**, under the SIL Open Font License.
- It's self-hosted in `wwwroot/fonts/source-sans-3/` (one variable woff2 file covering weights 200 to 900, with its license), so the app has no dependency on Google Fonts, which matters for self-hosted Docker installs.
- Fallback stack: `"Source Sans 3", system-ui, -apple-system, "Segoe UI", Roboto, sans-serif`.

| Style | Size / line height (px) | Weight | Usage |
|---|---|---|---|
| `text/display` | 32 / 40 | SemiBold 600 | Dashboard title |
| `text/h1` | 28 / 36 | SemiBold 600 | Page title |
| `text/h2` | 22 / 30 | SemiBold 600 | Section title |
| `text/h3` | 18 / 26 | SemiBold 600 | Card title |
| `text/body` | 16 / 24 | Regular 400 | Body text |
| `text/body-strong` | 16 / 24 | SemiBold 600 | List titles, table headers |
| `text/small` | 14 / 20 | Regular 400 | Metadata, help text |
| `text/badge` | 13 / 16 | SemiBold 600 | Badges and tags |

- In the app, each style is a CSS class in `wwwroot/css/typography.css`: `t-h1`, `t-h3`, `t-body-strong`, `t-small`, `t-badge`. Components use these classes instead of repeating the size, line height and weight. `text/body` is Bootstrap's default and has no class. The other styles get their class with their first use.
- Source Sans reads smaller than most sans-serifs, so body text stays at 16 px (Bootstrap's `1rem`).
- Tabular figures (`font-variant-numeric: tabular-nums`) are used for dates and counts in tables.
- The `ShortId` uses the system monospace stack (`ui-monospace, SFMono-Regular, Menlo, monospace`).

## Spacing, radius and shadows

| Spacing | Value |
|---|---|
| `space/4` | 4 px |
| `space/8` | 8 px |
| `space/12` | 12 px |
| `space/16` | 16 px |
| `space/24` | 24 px |
| `space/32` | 32 px |
| `space/48` | 48 px |

| Radius | Value | Usage |
|---|---|---|
| `radius/sm` | 4 px | Small elements |
| `radius/md` | 6 px | Inputs and buttons (Bootstrap default) |
| `radius/lg` | 8 px | Cards, tables |
| `radius/full` | 9999 px | Badges and tags |

Shadows are drawn in `#0F172A`:
- `shadow/sm` (cards): `0 1px 2px` at 6% and `0 1px 3px` at 8%.
- `shadow/md` (dropdowns, modals): `0 4px 6px -1px` at 8% and `0 10px 15px -3px` at 10%.

The **scrim** (`--ambio-scrim`) dims the page behind a sheet, a modal or the reconnect dialog: `#0F172A` at 45% in light, `#020617` at 70% in dark.

## Components

| Component | Figma | App (Phase 1) |
|---|---|---|
| `StatusBadge` | One variant per status (`Status=Draft…Withdrawn`), dot + label | `StatusBadge.razor`: takes an `ApplicationStatus`, renders the icon + localized label |
| `Tag` | `Style=Neutral \| Accent \| Archived`, `Label` text property | `Tag.razor`: style + label + optional icon |

The layout components (app shell, lists, tabs, sheets, toasts…) are listed in [ui.md](ui.md#components).

## Logo

A mark and a wordmark:
- the **mark**, "Orbit", is a path that goes around a point and ends on a dot, drawn in `accent/on` in a rounded square filled with `accent/default`. *Ambio* is Latin for "I go around, I canvass", which is what a job search does. It is 28 px in the app bar and the sidebar, 40 px on the log-in page;
- the **wordmark** "Ambio" is set in Source Sans 3 SemiBold, in `text/primary`, next to the mark.

The mark is drawn on a 32 × 32 grid: a square with 9-unit corners, a 270° arc of radius 7.5 and stroke 3 with round caps, from (10.7, 21.3) clockwise to (21.3, 10.7), a dot of radius 3 at the end of the arc and a dot of radius 2.2 in the center.

- In the app, `Logo.razor` draws it as inline SVG colored by the tokens, so it follows the theme like any other element.
- The mark alone is the favicon (`wwwroot/favicon.svg`): `#4F46E5` with a white shape in light, `#818CF8` with a `#020617` shape in dark. A favicon can't read the page's CSS variables, so it holds the hex values and follows the system color scheme, not the theme chosen in the app. `favicon.png` (32 × 32, light) is the fallback for browsers without SVG favicons.
- The mark isn't in Figma yet.

## From tokens to code

Each token becomes a CSS custom property prefixed with `--ambio-`. The `/` in the name becomes `-`, for example `status/interview/bg` → `var(--ambio-status-interview-bg)`. This is the code syntax set on each Figma variable, so Dev Mode shows the CSS name.

- The light values are declared on `:root`, and the dark values under `[data-bs-theme="dark"]`, the attribute Bootstrap 5.3 uses for its color modes.
- The theme follows `prefers-color-scheme` by default, and the user can force Light or Dark ([ui.md](ui.md#patterns)).
- Bootstrap's variables are mapped onto the tokens, so the stock components (buttons, forms, tables, cards) follow the identity without custom classes:

| Bootstrap variable | Token |
|---|---|
| `--bs-body-bg` | `bg/canvas` |
| `--bs-body-color` | `text/primary` |
| `--bs-secondary-color` | `text/secondary` |
| `--bs-tertiary-color` | `text/muted` |
| `--bs-tertiary-bg` | `bg/subtle` |
| `--bs-border-color`, `--bs-border-color-translucent` | `border/default` |
| `--bs-primary`, `--bs-primary-rgb` | `accent/default` |
| `--bs-link-color`, `--bs-link-hover-color` (and their `-rgb`) | `accent/text`, `accent/hover` |
| `--bs-focus-ring-color` | `focus/ring` |
| `--bs-success`, `--bs-warning`, `--bs-danger`, `--bs-info` (and their `-rgb`) | `feedback/*` |
| `--bs-{color}-text-emphasis`, `-bg-subtle`, `-border-subtle` (alerts) | `fg`, `bg`, `border` of the status with the same hue: success = `Offer`, warning = `Interview`, danger = `Rejected`, info = `Applied` |
| `--bs-card-bg`, `--bs-table-bg` | `bg/surface` |
| `--bs-body-font-family`, `--bs-font-monospace` | Source Sans 3 stack, monospace stack |
| `--bs-border-radius`, `-sm`, `-lg`, `-pill` | `radius/md`, `radius/sm`, `radius/lg`, `radius/full` |
| `--bs-box-shadow-sm`, `--bs-box-shadow` | `shadow/sm`, `shadow/md` |

- Bootstrap reads some colors as `r, g, b` triplets (`--bs-primary-rgb`, link and contextual colors). A triplet can't be derived from a hex variable, so `app.css` repeats their light and dark values as literals.
- Some components are compiled with fixed colors. `app.css` sets their component variables:
  - `.btn-primary`: the `accent/*` tokens;
  - `.btn-danger`: outlined, as in the mockups (`feedback/danger` text, `bg/surface`, `border/strong`);
  - inputs, selects and checkboxes: `bg/surface` and `border/strong`; their focus: `focus/ring`;
  - checked checkboxes: `accent/default`.

## Figma structure

| Collection | Variables | Notes |
|---|---|---|
| `Primitives` | 51 colors | Hidden from pickers (no scope) |
| `Color Light` | 51 semantic colors, aliased to primitives | Scoped to fills, text or strokes |
| `Color Dark` | Same names, dark aliases | |
| `Spacing` | 7 | Gap and padding |
| `Radius` | 4 | Corner radius |

The Figma file is on the **Starter** plan, where a collection can hold only one mode. Light and dark are therefore two collections with identical variable names, instead of two modes of one collection. The components are bound to `Color Light`, and the dark preview rebinds them to `Color Dark`. With a paid plan, the two collections can be merged into one collection with `Light` and `Dark` modes, without renaming any variable.
