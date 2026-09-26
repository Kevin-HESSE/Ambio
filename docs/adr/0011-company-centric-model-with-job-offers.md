# 0011 — Company-centric model with job offers and an application hierarchy

- Status: Accepted
- Date: 2026-09-26

## Context

The first sketch of the data model put the offer fields (title, URL, source) directly on `JobApplication`, with a bare `Company`. Several real situations don't fit that:

- The same company is targeted several times: one offer this month, another two months later. The research done on the company should be kept across applications.
- The same offer is often published on several sites (LinkedIn, Indeed, the careers page…).
- An offer can be worth saving before deciding to apply.
- Recruitment agencies and IT services companies (ESN) publish offers for a client that is sometimes hidden.
- Some applications are spontaneous: there is no offer at all.
- Many applications go through a website form, with no contact person.

## Decision

- **`Company`** is the anchor. It holds a structured profile, and later dated research notes and contacts. A `Kind` tells employers from recruitment agencies and ESNs.
- **`JobOffer`** is a separate entity. It keeps a plain-text copy of the ad and points to the end client (`CompanyId`), to the recruiter (`RecruiterCompanyId`), or to both, with at least one set.
- **`JobOfferPosting`** records each site where an offer was seen (site, URL, date). One offer can have many postings.
- **`JobApplication`** is an abstract base class with two subtypes, mapped as **table-per-hierarchy** in EF Core:
  - `OfferApplication`: required `JobOfferId`, optional `AppliedViaPostingId`. The company comes from the offer.
  - `SpontaneousApplication`: required `CompanyId` and `TargetPosition`.
- A discriminator column `Kind` and a check constraint keep the subtype columns consistent.
- How an application was sent is a `Channel` (online form, email, referral…). **Contacts are optional.**

Details are in [data-model.md](../data-model.md).

## Consequences

- Company research is written once and shared by every application to that company.
- "Exactly one of offer or company" is expressed by the type system instead of a runtime check. Shared behaviour (status workflow, cover letters, CVs, interactions) lives once, on the base class.
- TPH keeps the application list a single query with no join or `UNION`, at the cost of a few nullable columns guarded by a check constraint.
- The domain stays persistence-ignorant ([ADR 0006](0006-persistence-ignorant-domain-and-dtos.md)): the inheritance is plain C#, and the discriminator lives only in the mapping classes.
- Lists use a flat summary DTO, and detail pages use one DTO per subtype.
- Finding "all applications for a company" must check both the offer's client and recruiter, and the spontaneous applications' company.
