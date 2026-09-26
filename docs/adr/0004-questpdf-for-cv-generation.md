# 0004 — QuestPDF for CV generation

- Status: Accepted
- Date: 2026-09-26

## Context

CVs must be generated as PDFs from structured data, with layouts that third parties can write in C#.

## Decision

Use [QuestPDF](https://www.questpdf.com/): a fluent C# API, a layout engine that handles pagination, and the QuestPDF Companion for live previews during development.

## Consequences

- Templates are plain C#, which makes plugins natural (see ADR 0003).
- Postulo is a public, MIT-licensed open-source project, which qualifies for the QuestPDF **Community License**. The license is set in code with `QuestPDF.Settings.License = LicenseType.Community`.
- The Docker image must include fonts for Linux rendering.
- A `tools/Postulo.CvPlayground` console app is used to design templates with the Companion.
