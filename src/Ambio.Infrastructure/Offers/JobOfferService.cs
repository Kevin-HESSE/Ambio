using Ambio.Application.Common;
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

    public async Task<Guid> CreateAsync(NewJobOfferInput input, CancellationToken cancellationToken = default)
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

    public async Task<JobOfferDetailsDto?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);

        return await db.JobOffers
            .AsNoTracking()
            .Where(o => o.Id == id)
            .ToDetailsDto()
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<ServiceResult> UpdateAsync(Guid id, JobOfferInput input, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);

        var offer = await db.JobOffers.SingleOrDefaultAsync(o => o.Id == id, cancellationToken);
        if (offer is null)
        {
            return ServiceResult.Failure(JobOfferErrors.NotFound);
        }

        input.ApplyTo(offer);
        await db.SaveChangesAsync(cancellationToken);

        return ServiceResult.Success();
    }

    public async Task<ServiceResult> AddPostingAsync(Guid offerId, JobOfferPostingInput input, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);

        var offer = await db.JobOffers
            .Include(o => o.Postings)
            .SingleOrDefaultAsync(o => o.Id == offerId, cancellationToken);
        if (offer is null)
        {
            return ServiceResult.Failure(JobOfferErrors.NotFound);
        }

        if (offer.HasPostingWithUrl(input.Url))
        {
            return ServiceResult.Failure(JobOfferErrors.PostingAlreadyExists);
        }

        offer.AddPosting(input.Site, input.Url);
        await db.SaveChangesAsync(cancellationToken);

        return ServiceResult.Success();
    }

    public async Task<ServiceResult> RemovePostingAsync(Guid offerId, Guid postingId, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);

        var offer = await db.JobOffers
            .Include(o => o.Postings)
            .SingleOrDefaultAsync(o => o.Id == offerId, cancellationToken);
        if (offer is null)
        {
            return ServiceResult.Failure(JobOfferErrors.NotFound);
        }

        if (offer.Postings.All(p => p.Id != postingId))
        {
            return ServiceResult.Failure(JobOfferErrors.PostingNotFound);
        }

        if (!offer.CanRemovePosting)
        {
            return ServiceResult.Failure(JobOfferErrors.LastPosting);
        }

        // The posting leaves the offer's collection: EF Core deletes the orphan, as the relationship cascades.
        offer.RemovePosting(postingId);
        await db.SaveChangesAsync(cancellationToken);

        return ServiceResult.Success();
    }
}
