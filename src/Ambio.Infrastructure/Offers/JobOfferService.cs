using Ambio.Application.Common;
using Ambio.Application.Offers;
using Ambio.Application.Offers.Dtos;
using Ambio.Domain.JobApplications;
using Ambio.Domain.Offers;
using Ambio.Infrastructure.Common;
using Ambio.Infrastructure.Companies;
using Ambio.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace Ambio.Infrastructure.Offers;

internal class JobOfferService : IJobOfferService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
    private readonly ShortIdGenerator _shortIds;
    private readonly TimeProvider _timeProvider;

    public JobOfferService(IDbContextFactory<ApplicationDbContext> dbFactory, ShortIdGenerator shortIds, TimeProvider timeProvider)
    {
        _dbFactory = dbFactory;
        _shortIds = shortIds;
        _timeProvider = timeProvider;
    }

    public async Task<JobOfferListDto> ListAsync(bool includeArchived = false, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);

        var offers = await db.JobOffers
            .AsNoTracking()
            .Where(o => includeArchived || o.ArchivedAt == null)
            .OrderByDescending(o => o.CreatedAt)
            .ToSummaryDto()
            .ToListAsync(cancellationToken);

        return new JobOfferListDto(
            Offers: offers,
            HasArchived: await db.JobOffers.AnyAsync(o => o.ArchivedAt != null, cancellationToken));
    }

    public async Task<IReadOnlyList<JobOfferSummaryDto>> ListByCompanyAsync(Guid companyId, bool includeArchived = false, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);

        return await db.JobOffers
            .AsNoTracking()
            .Where(o => o.CompanyId == companyId && (includeArchived || o.ArchivedAt == null))
            .OrderByDescending(o => o.CreatedAt)
            .ToSummaryDto()
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<JobOfferSummaryDto>> FindSimilarAsync(string companyName, string title, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);

        // The Name column is COLLATE NOCASE, as in CreateAsync. A company has few offers: the titles are compared
        // once loaded.
        var name = companyName.Trim();
        var offers = await db.JobOffers
            .AsNoTracking()
            .Where(o => o.Company.Name == name && o.ArchivedAt == null)
            .OrderByDescending(o => o.CreatedAt)
            .ToSummaryDto()
            .ToListAsync(cancellationToken);

        return offers.Where(o => JobOfferTitles.AreSimilar(o.Title, title)).ToList();
    }

    public async Task<IReadOnlyList<JobOfferSummaryDto>> SearchAsync(string text, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return [];
        }

        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);

        // SQLite compares neither ignoring accents nor word by word: the few offers are matched once loaded.
        var offers = await db.JobOffers
            .AsNoTracking()
            .Where(o => o.ArchivedAt == null)
            .OrderByDescending(o => o.CreatedAt)
            .ToSummaryDto()
            .ToListAsync(cancellationToken);

        return offers.Where(o => TextSearch.Matches($"{o.Title} {o.CompanyName}", text)).ToList();
    }

    public async Task<JobOfferLinkDto> CreateAsync(NewJobOfferInput input, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);

        var companyId = await CompanyLookup.FindOrAddAsync(db, input.CompanyName, cancellationToken);
        var shortId = await _shortIds.FreeAsync(candidate =>
            db.JobOffers.AnyAsync(o => o.ShortId == candidate, cancellationToken));

        // Saved with the new company, if any, in a single transaction.
        var offer = input.ToEntity(companyId, shortId);
        db.JobOffers.Add(offer);
        await db.SaveChangesAsync(cancellationToken);

        // The name typed matches the company's ignoring case only, which the slug ignores too.
        return new JobOfferLinkDto(shortId, JobOfferSlug.From(offer.Title, input.CompanyName));
    }

    public async Task<JobOfferDetailsDto?> GetAsync(string shortId, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);

        return await db.JobOffers
            .AsNoTracking()
            .Where(o => o.ShortId == shortId)
            .ToDetailsDto()
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<ServiceResult<JobOfferLinkDto>> UpdateAsync(Guid id, JobOfferInput input, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);

        var offer = await db.JobOffers
            .Include(o => o.Company)
            .SingleOrDefaultAsync(o => o.Id == id, cancellationToken);
        if (offer is null)
        {
            return ServiceResult<JobOfferLinkDto>.Failure(JobOfferErrors.NotFound);
        }

        input.ApplyTo(offer);
        await db.SaveChangesAsync(cancellationToken);

        return ServiceResult<JobOfferLinkDto>.Success(
            new JobOfferLinkDto(offer.ShortId, JobOfferSlug.From(offer.Title, offer.Company.Name)));
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

    public async Task<ServiceResult> ArchiveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);

        var offer = await db.JobOffers.SingleOrDefaultAsync(o => o.Id == id, cancellationToken);
        if (offer is null)
        {
            return ServiceResult.Failure(JobOfferErrors.NotFound);
        }

        if (offer.IsArchived)
        {
            return ServiceResult.Success();
        }

        // The active applications go with it, on the same day, so a restore brings them back together.
        var now = Now;
        offer.Archive(now);
        foreach (var application in await db.JobApplications.OfType<OfferApplication>().Where(a => a.JobOfferId == id && a.ArchivedAt == null).ToListAsync(cancellationToken))
        {
            application.Archive(now);
        }

        await db.SaveChangesAsync(cancellationToken);

        return ServiceResult.Success();
    }

    public async Task<ServiceResult> RestoreAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);

        var offer = await db.JobOffers
            .Include(o => o.Company)
            .SingleOrDefaultAsync(o => o.Id == id, cancellationToken);
        if (offer is null)
        {
            return ServiceResult.Failure(JobOfferErrors.NotFound);
        }

        if (offer.ArchivedAt is not { } archivedAt)
        {
            return ServiceResult.Success();
        }

        // Only the applications archived with it, on the same day. Its company comes back alone: its other offers and
        // applications stay archived.
        offer.Restore();
        offer.Company.Restore();
        foreach (var application in await db.JobApplications.OfType<OfferApplication>().Where(a => a.JobOfferId == id && a.ArchivedAt == archivedAt).ToListAsync(cancellationToken))
        {
            application.Restore();
        }

        await db.SaveChangesAsync(cancellationToken);

        return ServiceResult.Success();
    }

    private DateTime Now => _timeProvider.GetUtcNow().UtcDateTime;
}
