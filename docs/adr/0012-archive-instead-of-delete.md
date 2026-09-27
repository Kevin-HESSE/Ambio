# 0012 — Archive instead of delete

- Status: Accepted
- Date: 2026-09-26

## Context

Companies, contacts, offers and applications reference each other. Deleting a company would either orphan or destroy the history of its applications, which is the very record Ambio is meant to keep. Yet the lists must stay focused on what is current.

## Decision

- `Company`, `Contact`, `JobOffer` and `JobApplication` have an `ArchivedAt` timestamp. Archiving hides a row from the default lists. Restoring it clears the timestamp.
- A hard delete is only allowed when nothing references the row: foreign keys between these entities use `Restrict`.
- Rows owned by a parent (company notes, offer postings, status changes, cover letters, interactions) are deleted in cascade with it.
- CV files are deleted by the service before their `CvDocument` rows.
- Archived rows are filtered out explicitly in the services. There is no EF global query filter, so an archived company still shows up in the history of its old applications.

## Consequences

- No accidental loss of history. Archiving is reversible.
- The services must remember to filter archived rows in their list queries. Integration tests cover it.
- The UI needs an "Archived" filter and a restore action.
