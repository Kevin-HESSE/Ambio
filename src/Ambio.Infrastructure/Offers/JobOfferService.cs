using Ambio.Application.Offers;
using Ambio.Application.Offers.Dtos;
using Ambio.Domain.Companies;
using Ambio.Infrastructure.Companies;
using Ambio.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace Ambio.Infrastructure.Offers;

internal class JobOfferService : IJobOfferService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

    public JobOfferService(IDbContextFactory<ApplicationDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<IReadOnlyList<JobOfferSummaryDto>> ListAsync(CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);

        return await db.JobOffers
            .AsNoTracking()
            .Where(o => o.ArchivedAt == null)
            .OrderByDescending(o => o.CreatedAt)
            .ToSummaryDto()
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<JobOfferSummaryDto>> ListByCompanyAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);

        return await db.JobOffers
            .AsNoTracking()
            .Where(o => o.CompanyId == companyId && o.ArchivedAt == null)
            .OrderByDescending(o => o.CreatedAt)
            .ToSummaryDto()
            .ToListAsync(cancellationToken);
    }

    public async Task<Guid> CreateAsync(JobOfferInput input, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);

        // The Name column is COLLATE NOCASE: the comparison ignores case, like the unique index. An archived
        // company keeps its name, so it is matched too.
        var name = input.CompanyName.Trim();
        var companyId = await db.Companies
            .Where(c => c.Name == name)
            .Select(c => (Guid?)c.Id)
            .SingleOrDefaultAsync(cancellationToken);

        if (companyId is null)
        {
            var company = new Company
            {
                Name = name,
                Slug = await CompanySlugs.FreeSlugAsync(db, name, companyId: null, cancellationToken),
                Kind = CompanyKind.Employer,
            };
            db.Companies.Add(company);
            companyId = company.Id;
        }

        // Saved with the new company, if any, in a single transaction.
        var offer = input.ToEntity(companyId.Value);
        db.JobOffers.Add(offer);
        await db.SaveChangesAsync(cancellationToken);

        return offer.Id;
    }
}
