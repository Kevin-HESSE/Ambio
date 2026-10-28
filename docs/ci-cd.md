# CI/CD

The CI runs on GitHub Actions. There is no CD: the Docker image is built locally with `docker compose` (see [deployment.md](deployment.md)).

## CI

Workflow: `.github/workflows/ci.yml`, run on pull requests into `main`, on pushes to `main` (which keeps the badge up to date), and on demand (`workflow_dispatch`). A new push to the same branch or pull request cancels the run in progress.

Until the proof of concept is finished, the CI doesn't run for `dev`: work on `dev` goes faster and GitHub Actions usage stays low. This departs from [ADR 0020](adr/0020-ci-and-protected-branches.md), which describes the CI on `dev` and `main`. Before opening a pull request into `dev`, run the checks locally:

```bash
dotnet format --verify-no-changes
dotnet test
```

```
Conventions ──┬──> Tests
              └──> End-to-end tests
```

The tests only start once the conventions are checked, and run in isolated jobs, in parallel.

| Job | Steps |
|---|---|
| `Conventions` | On a pull request, the commit message of every commit of the pull request, checked by the `commit-message-linter` task of the [git hooks](git-hooks.md). Merge commits, created by GitHub, are skipped. Then `dotnet format --verify-no-changes` on the whole solution. Both checks always report, even when the first one fails. |
| `Tests` | Release build of the solution, then every test but the end-to-end tests (`dotnet test -- --filter-not-trait "Category=E2E"`). |
| `End-to-end tests` | Release build of `tests/Ambio.E2E.Tests`, installs Chromium with its system dependencies (`playwright.ps1 install --with-deps chromium`), then runs the end-to-end tests. |

The `Conventions` job repeats the checks of the git hooks, so a commit made with `--no-verify` is still caught.

### Shared setup

Every job uses the composite action `.github/actions/setup`:

- installs the .NET 10 SDK (`10.0.x`, since `global.json` pins no SDK);
- restores the NuGet package cache (`~/.nuget/packages`), keyed on the hash of the `.csproj`, `Directory.Packages.props` and `Directory.Build.props` files;
- runs `dotnet restore`.

### Environment

| Variable | Value | Why |
|---|---|---|
| `HUSKY` | `0` | Skips the install of the git hooks on restore ([ADR 0019](adr/0019-git-hooks-with-husky-net.md)). The `Conventions` job installs them itself with `dotnet husky install` before checking the commit messages, since `husky run` needs them. |
| `DOTNET_NOLOGO`, `DOTNET_CLI_TELEMETRY_OPTOUT` | `true` | Quieter logs, no telemetry. |

### Protected branches

Two repository rulesets protect the branches ([ADR 0020](adr/0020-ci-and-protected-branches.md)), with no bypass:

| Ruleset | Branches | Rules |
|---|---|---|
| Protected branches | `dev`, `main` | Changes only arrive through a pull request, with no approval required. Force pushes and deletion are blocked. |
| Required CI checks | `main` | The `Conventions`, `Tests` and `End-to-end tests` checks must pass. The branch doesn't have to be up to date with its base. |

Renaming or adding a job that must block merges also means updating the required checks of the "Required CI checks" ruleset (Settings → Rules → Rulesets).

### Badge

The README badge shows the status of the default branch.

## CD

None in the proof of concept ([ADR 0029](adr/0029-proof-of-concept-ends-after-phase-2.md)): the image is built locally from the repository. The `Dockerfile` cross-compiles for the target architecture, so a CD workflow could publish a multi-arch image with buildx without emulating the build.
