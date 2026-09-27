# 0007 — Per-application CVs: selection + override, frozen as snapshots

- Status: Accepted
- Date: 2026-09-26

## Context

Each application should keep the exact CV that was sent, even if the profile changes later. A CV is also needed outside any application, to share on social networks. PDF generation arrives only in Phase 3, but a trace of sent CVs is needed from Phase 1.

## Decision

- A single `CvDocument` entity covers every CV, with `Source = Uploaded | Generated`.
- **Uploaded** (from Phase 1): the PDF actually sent is uploaded and attached to the application.
- **Generated** (from Phase 3): the user starts from the profile, **selects** the items to include and can **override** the title and summary for the offer. The result is stored as an immutable JSON snapshot, together with the template id and version, then rendered to PDF.
- `JobApplicationId` is nullable: a CV without an application is a **general CV**.

## Consequences

- Past applications always show what was actually sent.
- A CV can be regenerated from its snapshot with another template.
- The snapshot schema must stay readable across versions (versioned JSON).
