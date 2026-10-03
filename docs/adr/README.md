# Architecture Decision Records

Each significant decision is recorded as a short ADR: context, decision, consequences. See [ADR 0001](0001-record-architecture-decisions.md).

| # | Decision | Status |
|---|---|---|
| [0001](0001-record-architecture-decisions.md) | Record architecture decisions | Accepted |
| [0002](0002-single-user-application.md) | Single-user application | Accepted |
| [0003](0003-runtime-plugin-loading-at-startup.md) | CV templates as plugin DLLs loaded at startup | Accepted |
| [0004](0004-questpdf-for-cv-generation.md) | QuestPDF for CV generation | Accepted |
| [0005](0005-sqlite-and-filesystem-storage.md) | SQLite for data, file system for documents | Accepted |
| [0006](0006-persistence-ignorant-domain-and-dtos.md) | Persistence-ignorant domain, EF mapping classes, DTOs | Accepted, partly superseded by [0018](0018-feature-folders-in-every-project.md) |
| [0007](0007-cv-snapshots-selection-and-override.md) | Per-application CVs: selection + override, frozen as snapshots | Accepted |
| [0008](0008-docker-hub-multi-arch-images.md) | Multi-arch Docker images on Docker Hub | Accepted |
| [0009](0009-early-deployment-and-incremental-demo.md) | Early deployment and an incremental demo | Accepted |
| [0010](0010-dbcontext-factory-without-repositories.md) | DbContext factory, without repositories or unit of work | Accepted |
| [0011](0011-company-centric-model-with-job-offers.md) | Company-centric model with job offers and an application hierarchy | Accepted |
| [0012](0012-archive-instead-of-delete.md) | Archive instead of delete | Accepted |
| [0013](0013-guid-v7-primary-keys.md) | GUID v7 primary keys | Accepted |
| [0014](0014-short-id-and-slug-urls.md) | Short id and slug in application URLs | Accepted |
| [0015](0015-neutral-visual-identity-with-status-colors.md) | Neutral visual identity with status colors | Accepted |
| [0016](0016-responsive-navigation-bottom-tabs-and-sidebar.md) | Responsive navigation: bottom tabs on mobile, sidebar on desktop | Accepted |
| [0017](0017-account-creation-local-or-github.md) | Account creation: local or GitHub, one account at most | Accepted |
| [0018](0018-feature-folders-in-every-project.md) | Feature folders in every project | Accepted |
| [0019](0019-git-hooks-with-husky-net.md) | Git hooks with Husky.Net: formatting and commit messages | Accepted |

New ADRs copy the structure of an existing one and take the next number. An ADR is never edited once accepted: a new ADR supersedes it.
