# 0026 — Status workflow and history

- Status: Accepted
- Date: 2026-10-21

## Context

An application moves through `Draft → Applied → Interview → Offer / Rejected / Withdrawn` ([data-model.md](../data-model.md#application-hierarchy)). Until now its status only changed once, when a draft was created as already sent (`MarkAsSent`), and nothing kept track of it. Following an application needs every change to be kept, with the day it happened and a note. The order the statuses come in had to be settled too: can an application be rejected without an interview, can an offer be declined, can a rejected application come back?

## Decision

- **An application only moves forward.** The domain holds the transition table (`StatusWorkflow`) and refuses any other change:

  | From | Allowed next statuses |
  |---|---|
  | `Draft` | `Applied`, `Withdrawn` |
  | `Applied` | `Interview`, `Offer`, `Rejected`, `Withdrawn` |
  | `Interview` | `Offer`, `Rejected`, `Withdrawn` |
  | `Offer` | `Withdrawn` (declined) |
  | `Rejected`, `Withdrawn` | none |

- **`Applied` is only reached from `Draft`**, and requires the day the application was sent (`AppliedOn`).
- **Every change is a `StatusChange` row**: `From`, `To`, `ChangedAt` and an optional `Comment`. The history is append-only and deleted with its application.
- **Every application is born `Draft`.** One created as already sent records `Draft → Applied` as its first change: a single method, `JobApplication.ChangeStatus`, changes the status, and the history explains every status but the first.
- **`ChangedAt` is when the change was recorded** (UTC), while `AppliedOn` is the day the user says the application was sent. The two can differ when an application is logged after it was sent.

## Consequences

- The status shown always matches the last change of the history, and the dashboard can build its recent activity and its follow-up delay from the history.
- An application created as already sent shows a Draft entry and an Applied entry on the same day.
- A wrong status can't be undone: only a transition the table allows corrects it. An undo, or a way back from `Rejected` and `Withdrawn`, would need a new decision.
- Applications recorded before the history existed have none; no backfill was written, since the app wasn't released.
