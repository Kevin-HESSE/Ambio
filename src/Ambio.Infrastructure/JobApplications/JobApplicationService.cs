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
    private readonly TimeProvider _timeProvider;

    public JobApplicationService(IDbContextFactory<ApplicationDbContext> dbFactory, ShortIdGenerator shortIds, TimeProvider timeProvider)
    {
        _dbFactory = dbFactory;
        _shortIds = shortIds;
        _timeProvider = timeProvider;
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

    public async Task<JobApplicationDetailsDto?> GetAsync(string shortId, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);

        // One query per kind: a single projection can't build either DTO.
        return await db.JobApplications
                .OfType<OfferApplication>()
                .AsNoTracking()
                .Where(a => a.ShortId == shortId)
                .ToDetailsDto()
                .SingleOrDefaultAsync(cancellationToken) as JobApplicationDetailsDto
            ?? await db.JobApplications
                .OfType<SpontaneousApplication>()
                .AsNoTracking()
                .Where(a => a.ShortId == shortId)
                .ToDetailsDto()
                .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<ServiceResult<JobApplicationLinkDto>> CreateAsync(NewOfferApplicationInput input, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);

        var offer = await db.JobOffers
            .Include(o => o.Company)
            .Include(o => o.Postings)
            .SingleOrDefaultAsync(o => o.Id == input.JobOfferId, cancellationToken);
        if (offer is null)
        {
            return ServiceResult<JobApplicationLinkDto>.Failure(JobApplicationErrors.OfferNotFound);
        }

        var posting = input.AppliedViaPostingId is { } postingId
            ? offer.Postings.SingleOrDefault(p => p.Id == postingId)
            : null;
        if (input.AppliedViaPostingId is not null && posting is null)
        {
            return ServiceResult<JobApplicationLinkDto>.Failure(JobApplicationErrors.PostingNotFound);
        }

        var shortId = await _shortIds.FreeAsync(candidate =>
            db.JobApplications.AnyAsync(a => a.ShortId == candidate, cancellationToken));

        db.JobApplications.Add(input.ToEntity(offer.Id, shortId, posting, Now));
        await db.SaveChangesAsync(cancellationToken);

        return ServiceResult<JobApplicationLinkDto>.Success(
            new JobApplicationLinkDto(shortId, JobApplicationSlug.From(offer.Title, offer.Company.Name)));
    }

    public async Task<JobApplicationLinkDto> CreateAsync(NewSpontaneousApplicationInput input, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);

        var companyId = await CompanyLookup.FindOrAddAsync(db, input.CompanyName, cancellationToken);
        var shortId = await _shortIds.FreeAsync(candidate =>
            db.JobApplications.AnyAsync(a => a.ShortId == candidate, cancellationToken));

        // Saved with the new company, if any, in a single transaction.
        var application = input.ToEntity(companyId, shortId, Now);
        db.JobApplications.Add(application);
        await db.SaveChangesAsync(cancellationToken);

        // The name typed matches the company's ignoring case only, which the slug ignores too.
        return new JobApplicationLinkDto(shortId, JobApplicationSlug.From(application.TargetPosition, input.CompanyName));
    }

    public async Task<ServiceResult<JobApplicationLinkDto>> UpdateAsync(Guid id, OfferApplicationInput input, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);

        var application = await db.JobApplications
            .OfType<OfferApplication>()
            .Include(a => a.JobOffer.Company)
            .Include(a => a.JobOffer.Postings)
            .SingleOrDefaultAsync(a => a.Id == id, cancellationToken);
        if (application is null)
        {
            return ServiceResult<JobApplicationLinkDto>.Failure(JobApplicationErrors.NotFound);
        }

        var offer = application.JobOffer;
        var posting = input.AppliedViaPostingId is { } postingId
            ? offer.Postings.SingleOrDefault(p => p.Id == postingId)
            : null;
        if (input.AppliedViaPostingId is not null && posting is null)
        {
            return ServiceResult<JobApplicationLinkDto>.Failure(JobApplicationErrors.PostingNotFound);
        }

        input.ApplyTo(application, posting);
        await db.SaveChangesAsync(cancellationToken);

        return ServiceResult<JobApplicationLinkDto>.Success(
            new JobApplicationLinkDto(application.ShortId, JobApplicationSlug.From(offer.Title, offer.Company.Name)));
    }

    public async Task<ServiceResult<JobApplicationLinkDto>> UpdateAsync(Guid id, SpontaneousApplicationInput input, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);

        var application = await db.JobApplications
            .OfType<SpontaneousApplication>()
            .Include(a => a.Company)
            .SingleOrDefaultAsync(a => a.Id == id, cancellationToken);
        if (application is null)
        {
            return ServiceResult<JobApplicationLinkDto>.Failure(JobApplicationErrors.NotFound);
        }

        input.ApplyTo(application);
        await db.SaveChangesAsync(cancellationToken);

        return ServiceResult<JobApplicationLinkDto>.Success(
            new JobApplicationLinkDto(application.ShortId, JobApplicationSlug.From(application.TargetPosition, application.Company.Name)));
    }

    public async Task<ServiceResult> ChangeStatusAsync(Guid id, StatusChangeInput input, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);

        // The history isn't loaded: a change is only appended to it.
        var application = await db.JobApplications.SingleOrDefaultAsync(a => a.Id == id, cancellationToken);
        if (application is null)
        {
            return ServiceResult.Failure(JobApplicationErrors.NotFound);
        }

        if (input.To is not { } to || !application.CanChangeStatusTo(to))
        {
            return ServiceResult.Failure(JobApplicationErrors.StatusNotAllowed);
        }

        input.ApplyTo(application, Now);
        await db.SaveChangesAsync(cancellationToken);

        return ServiceResult.Success();
    }

    private DateTime Now => _timeProvider.GetUtcNow().UtcDateTime;

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
