using Ambio.Application.Common;
using Ambio.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace Ambio.Infrastructure.Companies;

/// <summary>The slugs of the company URLs (ADR 0021), shared by the services that create or rename a company.</summary>
internal static class CompanySlugs
{
    // The slug of a company whose name keeps no letter or digit.
    private const string Fallback = "company";

    // Slugs that a page route would catch first, such as /companies/new.
    private static readonly string[] Reserved = ["new"];

    /// <summary>The slug of the name, followed by -2, -3… while it is reserved or held by another company,
    /// archived or not. The company itself is left out, so a rename keeps a slug it already holds.
    /// The unique index on Slug is the final guard.</summary>
    public static async Task<string> FreeSlugAsync(
        ApplicationDbContext db, string name, Guid? companyId, CancellationToken cancellationToken)
    {
        var slug = Slug.From(name, Fallback);
        var candidate = slug;
        var suffix = 1;

        while (await IsTakenAsync(candidate))
        {
            suffix++;
            candidate = $"{slug}-{suffix}";
        }

        return candidate;

        // Taken: reserved for a route, or held by another company, archived or not.
        async Task<bool> IsTakenAsync(string candidate) =>
            Reserved.Contains(candidate)
            || await db.Companies.AnyAsync(c => c.Id != companyId && c.Slug == candidate, cancellationToken);
    }
}
