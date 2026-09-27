# 0016 — Responsive navigation: bottom tabs on mobile, sidebar on desktop

- Status: Accepted
- Date: 2026-09-27

## Context

Postulo must work on a phone: an application is often logged or updated right after reading an ad or taking a call. The template layout collapses its sidebar into a hamburger menu below 641 px, which hides the four sections behind an extra tap. The app will also grow: contacts and interactions (Phase 2b), profile and CVs (Phase 3), templates (Phase 4), language (Phase 5), export (Phase 6). The navigation and the page structure must take these without a redesign.

## Decision

- **One breakpoint, at 992 px** (Bootstrap `lg`). Below it, phones and tablets share the mobile layout.
- **Mobile**: a top app bar and a **bottom tab bar** with five tabs: Home, Applications, Offers, Companies, **More**. More opens a bottom sheet (Settings, Account, Theme, Log out). A floating "New application" button sits on the dashboard and the applications list.
- **Desktop**: a 240 px sidebar with the same four sections, and Settings, theme and account at its bottom.
- **Later phases add entries** to the More sheet and the sidebar. The tab bar never gets a sixth tab.
- **Lists** are tables on desktop and cards on mobile, from the same data.
- **Detail pages use tabs** stored in the URL (`?tab=`), so later phases add tabs (interactions, contacts, notes) rather than reworking the pages. The application page has a side panel for the status and its history on desktop, stacked on mobile.
- **Overlays** are one component: a bottom sheet on mobile, a centered modal on desktop.
- The screens are specified in [ui.md](../ui.md), with HTML mockups in `docs/mockups/`.

## Consequences

- The four sections are one tap away on a phone, within thumb reach.
- The layout needs a single media query; there is no tablet-specific design to maintain.
- The More sheet absorbs every future entry, but secondary pages are two taps away on mobile.
- Each list needs two renderings (table and cards), which the list components encapsulate.
- The app bar, tab bar and FAB take about 120 px of height on a phone: pages keep a compact header and put secondary information in tabs.
