# 0027 — Archiving cascade and restore

- Status: Accepted
- Date: 2026-10-23

## Context

[ADR 0012](0012-archive-instead-of-delete.md) archives rows instead of deleting them, but leaves open what happens to the rows that depend on an archived one. A company has offers and applications, an offer has applications: once the company is archived, its applications would stay in the lists, under a company that no longer shows. Restoring raises the opposite question: what comes back with the row, and what an application brings back when its offer or its company is archived. ADR 0012 also needs an archived application to keep its status.

## Decision

- **Archiving cascades down, on the same day.** Archiving a company archives its active offers and its active applications, to its offers or spontaneous. Archiving an offer archives its active applications. Every row archived by the cascade gets the same `ArchivedAt`.
- **A row keeps the day it was first archived.** `Archive` doesn't change the `ArchivedAt` of an archived row, so a cascade passing over it, or a second archive, leaves it as it was.
- **Restoring brings back what was archived with the row.** Restoring a company or an offer restores the rows below it whose `ArchivedAt` equals its own. A row archived before keeps its own day and stays archived.
- **Restoring a row restores its archived parents, alone.** Restoring an application restores its offer and its company; restoring an offer restores its company. Their other offers and applications stay archived.
- **An archived application can't change its status.** The domain refuses it until the application is restored. Its other fields can still be edited.
- **Archiving and restoring are idempotent.** Archiving an archived row, or restoring an active one, succeeds without changing anything.

## Consequences

- An archived parent shows the children archived with it, dimmed, so the user sees what a restore brings back.
- The Undo of an archive is a restore: it brings back exactly what the archive hid, as long as nothing was archived again in between.
- Two archives made in the same instant by different actions would be restored together. With one user and timestamps to the tick, this isn't expected.
- The same day serves as a link between the rows of a cascade, without any table to record it. A row archived alone, then caught by its parent's cascade, keeps its own day, so the parent's restore leaves it archived.
