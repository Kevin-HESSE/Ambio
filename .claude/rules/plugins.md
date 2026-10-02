---
paths: src/Ambio.Plugins.*/**
---

# Plugin rules

- Plugin projects are named after their purpose: `Ambio.Plugins.<Domain>.<Purpose>` (e.g. `Ambio.Plugins.CV.Abstractions`, `Ambio.Plugins.CV.Default`), never a generic `Ambio.Plugins.Abstractions`. The namespace matches the project name.
- A plugin references only `Ambio.Plugins.CV.Abstractions` and QuestPDF, with `<Private>false</Private>` / `ExcludeAssets="runtime"` so they aren't copied to the output.
- Templates only receive a `CvModel`: no database, no file system access.
- `Ambio.Plugins.CV.Abstractions` is a public API under semantic versioning: a breaking change requires a major version bump ([ADR 0003](../../docs/adr/0003-runtime-plugin-loading-at-startup.md)).
- Every template gets a PDF snapshot test.

See [docs/plugins.md](../../docs/plugins.md).
