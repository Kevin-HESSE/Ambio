# CI/CD

The CI runs on GitHub Actions. The CD, which builds and publishes the Docker image, comes in [Phase 2](roadmap.md#phase-2--docker-deployment--demo) (see [deployment.md](deployment.md)).

## CI

Workflow: `.github/workflows/ci.yml`, run on every pull request, on every push to `dev` and `main`, and on demand (`workflow_dispatch`). A new push to the same branch or pull request cancels the run in progress.

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

### Badge

The README badge shows the status of the default branch.

## CD

_Coming in Phase 2_: multi-arch image (`linux/amd64`, `linux/arm64`) built with buildx and pushed to Docker Hub on version tags ([ADR 0008](adr/0008-docker-hub-multi-arch-images.md)).
