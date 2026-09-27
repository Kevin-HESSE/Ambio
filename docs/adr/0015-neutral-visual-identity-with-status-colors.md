# 0015 — Neutral visual identity with status colors

- Status: Accepted
- Date: 2026-09-27

## Context

The app still uses the Blazor template defaults: Helvetica and a Bootstrap blue (`#1B6EC2`). An application tracker is read as a list: dozens of rows, each with a status, a kind, a channel, a contract type. If every attribute gets its own color, the status (the information that matters) gets lost among them. The app must also stay readable in a dark theme and remain a showcase project, so its look must be consistent and documented.

## Decision

- **Neutral base**: cool grays for surfaces, text and borders, plus a single **indigo** accent (`#4F46E5` light, `#818CF8` dark) for primary actions, links and focus.
- **Hues reserved for application statuses**: Draft (gray, dashed), Applied (blue), Interview (amber), Offer (green), Rejected (red), Withdrawn (gray, filled). The accent never marks a status. The feedback colors (success, warning, danger, info) reuse the same hues.
- **Neutral tags** for every other attribute. The icon tells them apart, not the color. The only exception is a spontaneous application, which uses an accent tag.
- **Color is never the only signal**: a badge always has a label (and an icon in the app). All text pairs reach WCAG AA contrast (4.5:1), and status dots reach 3:1, in both themes.
- **Light and dark themes** from the start, with a light and a dark value for every semantic token.
- **Typography**: Source Sans 3 (OFL), self-hosted, with body text at 16 px.
- **Figma is the source of the tokens**. The values are documented in hex in [design-system.md](../design-system.md), and they become `--postulo-*` CSS custom properties that are mapped onto Bootstrap 5.3's variables. Bootstrap stays the component library.

## Consequences

- The status stands out in any list, and the rest of the interface stays calm.
- Stock Bootstrap components follow the identity through variable mapping, with little custom CSS.
- Dark mode comes almost for free with Bootstrap's `data-bs-theme`, as long as components only use tokens and never hard-coded colors.
- A new status or tag must fit the rules: a new status needs a hue distinct from the others and from the accent, and a new attribute is a neutral tag.
- The font files (two weights, woff2) add a few dozen KB to the image, and no request goes to an external CDN.
- On Figma's Starter plan, a collection has one mode only, so light and dark are two variable collections with identical names. They can be merged into one collection with two modes if the plan changes.
