using System.Linq.Expressions;

using Ambio.Application.Common;
using Ambio.Application.JobApplications;
using Ambio.Application.JobApplications.Dtos;
using Ambio.Domain.JobApplications;
using Ambio.Infrastructure.Common;
using Ambio.Infrastructure.Companies;
using Ambio.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace Ambio.Infrastructure.JobApplications;

internal class JobApplicationService : IJobApplicationService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
    private readonly ShortIdGenerator _shortIds;

    public JobApplicationService(IDbContextFactory<ApplicationDbContext> dbFactory, ShortIdGenerator shortIds)
    {
        _dbFactory = dbFactory;
        _shortIds = shortIds;
    }

    public Task<IReadOnlyList<JobApplicationSummaryDto>> ListAsync(CancellationToken cancellationToken = default) =>
        ListWhereAsync(_ => true, cancellationToken);

    public Task<IReadOnlyList<JobApplicationSummaryDto>> ListByOfferAsync(Guid offerId, CancellationToken cancellationToken = default) =>
        ListWhereAsync(a => a is OfferApplication && ((OfferApplication)a).JobOfferId == offerId, cancellationToken);

    public Task<IReadOnlyList<JobApplicationSummaryDto>> ListByCompanyAsync(Guid companyId, CancellationToken cancellationToken = default) =>
        ListWhereAsync(
            a => a is OfferApplication
                ? ((OfferApplication)a).JobOffer.CompanyId == companyId
                : ((SpontaneousApplication)a).CompanyId == companyId,
            cancellationToken);

    public async Task<ServiceResult> CreateAsync(NewOfferApplicationInput input, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);

        var offer = await db.JobOffers
            .Include(o => o.Postings)
            .SingleOrDefaultAsync(o => o.Id == input.JobOfferId, cancellationToken);
        if (offer is null)
        {
            return ServiceResult.Failure(JobApplicationErrors.OfferNotFound);
        }

        var posting = input.AppliedViaPostingId is { } postingId
            ? offer.Postings.SingleOrDefault(p => p.Id == postingId)
            : null;
        if (input.AppliedViaPostingId is not null && posting is null)
        {
            return ServiceResult.Failure(JobApplicationErrors.PostingNotFound);
        }

        var shortId = await _shortIds.FreeAsync(candidate =>
            db.JobApplications.AnyAsync(a => a.ShortId == candidate, cancellationToken));

        db.JobApplications.Add(input.ToEntity(offer.Id, shortId, posting));
        await db.SaveChangesAsync(cancellationToken);

        return ServiceResult.Success();
    }

    public async Task CreateAsync(NewSpontaneousApplicationInput input, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);

        var companyId = await CompanyLookup.FindOrAddAsync(db, input.CompanyName, cancellationToken);
        var shortId = await _shortIds.FreeAsync(candidate =>
            db.JobApplications.AnyAsync(a => a.ShortId == candidate, cancellationToken));

        // Saved with the new company, if any, in a single transaction.
        db.JobApplications.Add(input.ToEntity(companyId, shortId));
        await db.SaveChangesAsync(cancellationToken);
    }

    /// <summary>The active applications of every kind that match <paramref name="predicate"/>, newest first: by
    /// sending day, or by creation day for a draft, then by creation time.</summary>
    private async Task<IReadOnlyList<JobApplicationSummaryDto>> ListWhereAsync(
        Expression<Func<JobApplication, bool>> predicate, CancellationToken cancellationToken)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);

        return await db.JobApplications
            .AsNoTracking()
            .Where(a => a.ArchivedAt == null)
            .Where(predicate)
            .OrderByDescending(a => a.AppliedOn ?? DateOnly.FromDateTime(a.CreatedAt))
            .ThenByDescending(a => a.CreatedAt)
            .ToSummaryDto()
            .ToListAsync(cancellationToken);
    }
}
