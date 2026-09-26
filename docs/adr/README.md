# Architecture Decision Records

Each significant decision is recorded as a short ADR: context, decision, consequences. See [ADR 0001](0001-record-architecture-decisions.md).

| # | Decision | Status |
|---|---|---|
| [0001](0001-record-architecture-decisions.md) | Record architecture decisions | Accepted |
| [0002](0002-single-user-application.md) | Single-user application | Accepted |
| [0003](0003-runtime-plugin-loading-at-startup.md) | CV templates as plugin DLLs loaded at startup | Accepted |
| [0004](0004-questpdf-for-cv-generation.md) | QuestPDF for CV generation | Accepted |
| [0005](0005-sqlite-and-filesystem-storage.md) | SQLite for data, file system for documents | Accepted |
| [0006](0006-persistence-ignorant-domain-and-dtos.md) | Persistence-ignorant domain, EF mapping classes, DTOs | Accepted |
| [0007](0007-cv-snapshots-selection-and-override.md) | Per-application CVs: selection + override, frozen as snapshots | Accepted |
| [0008](0008-docker-hub-multi-arch-images.md) | Multi-arch Docker images on Docker Hub | Accepted |
| [0009](0009-early-deployment-and-incremental-demo.md) | Early deployment and an incremental demo | Accepted |
| [0010](0010-dbcontext-factory-without-repositories.md) | DbContext factory, without repositories or unit of work | Accepted |

New ADRs copy the structure of an existing one and take the next number. An ADR is never edited once accepted: a new ADR supersedes it.
