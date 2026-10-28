using System.Linq.Expressions;

using Ambio.Application.Common;
using Ambio.Application.Companies;
using Ambio.Application.Companies.Dtos;
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

    public async Task<JobApplicationListDto> ListAsync(JobApplicationFilter filter, int count, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);

        var query = db.JobApplications.AsNoTracking().Where(a => filter.IncludeArchived || a.ArchivedAt == null);
        if (filter.Kind is { } kind)
        {
            query = kind == ApplicationKind.Spontaneous
                ? query.Where(a => a is SpontaneousApplication)
                : query.Where(a => a is OfferApplication);
        }
        if (filter.CompanySlug is { } companySlug)
        {
            query = query.Where(a => a is OfferApplication
                ? ((OfferApplication)a).JobOffer.Company.Slug == companySlug
                : ((SpontaneousApplication)a).Company.Slug == companySlug);
        }

        IEnumerable<JobApplicationSummaryDto> applications = await NewestFirst(query)
            .ToSummaryDto()
            .ToListAsync(cancellationToken);

        // SQLite compares neither ignoring accents nor word by word: the applications are searched once loaded, then
        // counted by status before the status filter, so each count says what its status would show.
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            applications = applications.Where(a => TextSearch.Matches($"{a.Title} {a.CompanyName}", filter.Search));
        }

        var matching = applications.ToList();
        var counts = matching.CountBy(a => a.Status).ToDictionary();
        if (filter.Status is { } status)
        {
            matching = matching.Where(a => a.Status == status).ToList();
        }

        return new JobApplicationListDto(
            Applications: matching.Take(count).ToList(),
            Total: matching.Count,
            CountsByStatus: Enum.GetValues<ApplicationStatus>().ToDictionary(s => s, s => counts.GetValueOrDefault(s)),
            HasArchived: await db.JobApplications.AnyAsync(a => a.ArchivedAt != null, cancellationToken));
    }

    public async Task<IReadOnlyList<CompanySummaryDto>> ListCompaniesAsync(bool includeArchived = false, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);

        var listed = db.JobApplications.Where(a => includeArchived || a.ArchivedAt == null);

        return await db.Companies
            .AsNoTracking()
            .Where(c => listed.OfType<SpontaneousApplication>().Any(a => a.CompanyId == c.Id)
                || listed.OfType<OfferApplication>().Any(a => a.JobOffer.CompanyId == c.Id))
            .OrderBy(c => c.Name)
            .ToSummaryDto()
            .ToListAsync(cancellationToken);
    }

    public Task<IReadOnlyList<JobApplicationSummaryDto>> ListByOfferAsync(Guid offerId, bool includeArchived = false, CancellationToken cancellationToken = default) =>
        ListWhereAsync(a => a is OfferApplication && ((OfferApplication)a).JobOfferId == offerId, includeArchived, cancellationToken);

    public Task<IReadOnlyList<JobApplicationSummaryDto>> ListByCompanyAsync(Guid companyId, bool includeArchived = false, CancellationToken cancellationToken = default) =>
        ListWhereAsync(JobApplicationQueries.OfCompany(companyId), includeArchived, cancellationToken);

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

        if (application.IsArchived)
        {
            return ServiceResult.Failure(JobApplicationErrors.Archived);
        }

        if (input.To is not { } to || !application.CanChangeStatusTo(to))
        {
            return ServiceResult.Failure(JobApplicationErrors.StatusNotAllowed);
        }

        input.ApplyTo(application, Now);
        await db.SaveChangesAsync(cancellationToken);

        return ServiceResult.Success();
    }

    public async Task<ServiceResult> ArchiveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);

        var application = await db.JobApplications.SingleOrDefaultAsync(a => a.Id == id, cancellationToken);
        if (application is null)
        {
            return ServiceResult.Failure(JobApplicationErrors.NotFound);
        }

        application.Archive(Now);
        await db.SaveChangesAsync(cancellationToken);

        return ServiceResult.Success();
    }

    public async Task<ServiceResult> RestoreAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);

        // One query per kind, as GetAsync: each loads the parents to restore.
        var offerApplication = await db.JobApplications
            .OfType<OfferApplication>()
            .Include(a => a.JobOffer.Company)
            .SingleOrDefaultAsync(a => a.Id == id, cancellationToken);
        var spontaneousApplication = offerApplication is null
            ? await db.JobApplications
                .OfType<SpontaneousApplication>()
                .Include(a => a.Company)
                .SingleOrDefaultAsync(a => a.Id == id, cancellationToken)
            : null;

        JobApplication? application = (JobApplication?)offerApplication ?? spontaneousApplication;
        if (application is null)
        {
            return ServiceResult.Failure(JobApplicationErrors.NotFound);
        }

        if (!application.IsArchived)
        {
            return ServiceResult.Success();
        }

        // Its parents come back alone: their other offers and applications stay archived.
        application.Restore();
        offerApplication?.JobOffer.Restore();
        offerApplication?.JobOffer.Company.Restore();
        spontaneousApplication?.Company.Restore();
        await db.SaveChangesAsync(cancellationToken);

        return ServiceResult.Success();
    }

    private DateTime Now => _timeProvider.GetUtcNow().UtcDateTime;

    /// <summary>The active applications of every kind that match <paramref name="predicate"/>, and the archived ones
    /// too with <paramref name="includeArchived"/>, newest first.</summary>
    private async Task<IReadOnlyList<JobApplicationSummaryDto>> ListWhereAsync(
        Expression<Func<JobApplication, bool>> predicate, bool includeArchived, CancellationToken cancellationToken)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);

        return await NewestFirst(db.JobApplications
                .AsNoTracking()
                .Where(a => includeArchived || a.ArchivedAt == null)
                .Where(predicate))
            .ToSummaryDto()
            .ToListAsync(cancellationToken);
    }

    /// <summary>Newest first: by sending day, or by creation day for a draft, then by creation time.</summary>
    private static IQueryable<JobApplication> NewestFirst(IQueryable<JobApplication> applications) =>
        applications
            .OrderByDescending(a => a.AppliedOn ?? DateOnly.FromDateTime(a.CreatedAt))
            .ThenByDescending(a => a.CreatedAt);
}
