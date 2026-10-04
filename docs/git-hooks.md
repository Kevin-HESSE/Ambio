# Git hooks

Git hooks (Husky.Net, [ADR 0019](adr/0019-git-hooks-with-husky-net.md)) are installed by the first restore. They only report problems: they never change or stage files.

`HUSKY=0` skips both the install and the hooks. Only use it in the CI and in builds without `.git` (Docker).

## List of hooks

Hooks are declared in `.husky/task-runner.json`.

### pre-commit

- Refuses staged `.cs` files that `dotnet format` would change.

### commit-msg

- Refuses a message that breaks the commit convention of `CLAUDE.md` (Git section). A body is accepted after a blank line.
- Refuses a `Merge …` message and the revert of a merge: they only go through pull requests. Reverting a single commit (`Revert "<subject>"`) is accepted.
