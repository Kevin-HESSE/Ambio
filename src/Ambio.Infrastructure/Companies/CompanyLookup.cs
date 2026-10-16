using Ambio.Domain.Companies;
using Ambio.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace Ambio.Infrastructure.Companies;

/// <summary>Finds the company typed by name in a form, such as the new offer form, or adds it, shared by the services
/// that link a new row to a company.</summary>
internal static class CompanyLookup
{
    /// <summary>The id of the company with this name once trimmed, compared ignoring case, archived or not. Otherwise
    /// a new <see cref="CompanyKind.Employer"/> company with this name and a free slug is added to
    /// <paramref name="db"/>: the caller's <c>SaveChangesAsync</c> saves it with the rest, in a single
    /// transaction.</summary>
    public static async Task<Guid> FindOrAddAsync(ApplicationDbContext db, string name, CancellationToken cancellationToken)
    {
        // The Name column is COLLATE NOCASE: the comparison ignores case, like the unique index. An archived
        // company keeps its name, so it is matched too.
        var trimmed = name.Trim();
        var companyId = await db.Companies
            .Where(c => c.Name == trimmed)
            .Select(c => (Guid?)c.Id)
            .SingleOrDefaultAsync(cancellationToken);

        if (companyId is { } id)
        {
            return id;
        }

        var company = new Company
        {
            Name = trimmed,
            Slug = await CompanySlugs.FreeSlugAsync(db, trimmed, companyId: null, cancellationToken),
            Kind = CompanyKind.Employer,
        };
        db.Companies.Add(company);

        return company.Id;
    }
}
