# 0020 — CI checks and protected branches

- Status: Accepted
- Date: 2026-10-03

## Context

The git hooks of [ADR 0019](0019-git-hooks-with-husky-net.md) can be skipped with `--no-verify`, and nothing built or tested the code outside the author's machine. Anything could still be pushed straight to `dev` or `main`.

## Decision

- **GitHub Actions** runs the CI (`.github/workflows/ci.yml`) on pull requests and on pushes to `dev` and `main`.
- **Conventions before tests.** The CI first checks the project conventions, then runs the tests in jobs isolated from that check.
- **The CI's jobs and checks are not fixed by this ADR.** They are declared in `.github/workflows/ci.yml` and may change with the project: adding, changing or removing one needs no new ADR.
- **Protected branches.** A repository ruleset protects `dev` and `main`: changes only arrive through a pull request whose required CI checks pass. Force pushes and deletion are blocked, and nobody bypasses the ruleset.

## Consequences

- Direct pushes to `dev` and `main` are refused, even for the owner.
- A failing check blocks the merge until it is fixed on the branch.
- A commit made with `--no-verify` is still caught by the checks the CI repeats.
- Renaming or adding a job that must block merges also means updating the required checks of the ruleset.
