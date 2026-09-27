# 0003 — CV templates as plugin DLLs loaded at startup

- Status: Accepted
- Date: 2026-09-26

## Context

Users must be able to add or change CV templates without rebuilding the Docker image. Options considered: templates compiled into the app, declarative templates (JSON/YAML), and .NET assemblies loaded at runtime.

## Decision

- Templates are .NET assemblies implementing `ICvTemplate` from `Ambio.Plugins.CV.Abstractions`.
- They are placed in `/plugins/<Name>/` (a Docker volume) and discovered **once at startup**.
- Each plugin has its own `AssemblyLoadContext`. The contract assembly and QuestPDF are shared with the host.
- No hot reload: adding or updating a plugin requires a restart.
- Plugin projects follow the naming convention `Ambio.Plugins.<Domain>.<Purpose>`.

## Consequences

- Full QuestPDF power for template authors, with no rebuild of the image.
- Restart-only loading avoids the pitfalls of unloading (locked files, memory leaks from collectible contexts).
- Plugins run in-process and are trusted code. This is acceptable for a single-user, self-hosted app.
- The contract is a public API: breaking changes require a major version bump.
