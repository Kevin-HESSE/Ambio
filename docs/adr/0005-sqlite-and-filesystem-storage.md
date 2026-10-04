# 0005 — SQLite for data, file system for documents

- Status: Accepted
- Date: 2026-09-26

## Context

The app is single-user and self-hosted, often on a NAS. It stores structured data (applications, cover letters, profile) and binary files (uploaded and generated CV PDFs, an optional photo).

## Decision

- Structured data, including cover letters (plain text), lives in **SQLite**.
- Files are stored on the **file system** under `/data`, behind an `IDocumentStorage` interface. The database stores only metadata and the relative path.
- All state lives in a single `/data` volume.

## Consequences

- No database server to run. Backup is a copy of `/data`.
- The database stays small and fast.
- Files and database rows can drift apart (e.g. a manual deletion). The storage service handles missing files gracefully.
- `IDocumentStorage` allows switching to object storage later if needed.
