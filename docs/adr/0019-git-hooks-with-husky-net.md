# 0019 — Git hooks with Husky.Net

- Status: Accepted
- Date: 2026-10-03

## Context

`.editorconfig`, `EnforceCodeStyleInBuild` and `TreatWarningsAsErrors` catch style issues, but only at build time. Merges and reverts of a whole branch should only happen through pull requests, where they stay visible.

## Decision

- **Husky.Net** runs git hooks stored in the repository. It is a local dotnet tool (`.config/dotnet-tools.json`), and the hooks live in `.husky/`.
- **Automatic install.** A target in `Directory.Build.targets` runs `dotnet tool restore` and `dotnet husky install` during restore, so the hooks are set up by the first build of a clone. A stamp file (`.husky/_/install.stamp`) keeps it from running again until the tool manifest changes. `HUSKY=0` skips it, for the CI and for builds without `.git` (Docker).
- **The hooks' checks are not fixed by this ADR.** They are declared in `.husky/task-runner.json` (scripts in `.husky/scripts/`) and may change with the project: adding, changing or removing one needs no new ADR.
- **Hooks check, they don't fix.** A hook refuses the commit and tells what is wrong; it never changes or stages files behind the author's back.
- **`commit-msg` enforces the commit convention**, and refuses local merges and reverts of a whole branch, which only go through pull requests.
- **The CI repeats the checks it can**, so a commit made with `--no-verify` is still caught.

## Consequences

- Each hook adds its checks' time to every commit.
- `git commit --no-verify` skips the hooks; only the checks the CI repeats still catch the commit.
- Merges between branches are only done on GitHub, never locally.
- The Phase 2 Dockerfile sets `HUSKY=0`, since its build context has no `.git`.
