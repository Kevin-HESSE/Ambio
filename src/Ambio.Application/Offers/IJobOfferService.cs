using Ambio.Application.Common;
using Ambio.Application.Offers.Dtos;

namespace Ambio.Application.Offers;

public interface IJobOfferService
{
    /// <summary>Active offers (not archived), newest first.</summary>
    public Task<IReadOnlyList<JobOfferSummaryDto>> ListAsync(CancellationToken cancellationToken = default);

    /// <summary>Active offers of the company, newest first.</summary>
    public Task<IReadOnlyList<JobOfferSummaryDto>> ListByCompanyAsync(Guid companyId, CancellationToken cancellationToken = default);

    /// <summary>Creates the offer with its first posting and returns its id. The offer goes to the company with
    /// this name ignoring case, archived or not; when there is none, the company is created as an employer.</summary>
    public Task<Guid> CreateAsync(NewJobOfferInput input, CancellationToken cancellationToken = default);

    /// <summary>The offer with its postings, archived or not, or null when no offer has this id.</summary>
    public Task<JobOfferDetailsDto?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Replaces the fields of the offer, but not its company nor its postings, or fails with
    /// <see cref="JobOfferErrors.NotFound"/>.</summary>
    public Task<ServiceResult> UpdateAsync(Guid id, JobOfferInput input, CancellationToken cancellationToken = default);

    /// <summary>Records another site where the offer was seen, or fails with <see cref="JobOfferErrors.NotFound"/>,
    /// or with <see cref="JobOfferErrors.PostingAlreadyExists"/> when the offer already has this link.</summary>
    public Task<ServiceResult> AddPostingAsync(Guid offerId, JobOfferPostingInput input, CancellationToken cancellationToken = default);

    /// <summary>Removes a posting of the offer, or fails with <see cref="JobOfferErrors.NotFound"/>,
    /// <see cref="JobOfferErrors.PostingNotFound"/>, or <see cref="JobOfferErrors.LastPosting"/> when it is the
    /// offer's only posting.</summary>
    public Task<ServiceResult> RemovePostingAsync(Guid offerId, Guid postingId, CancellationToken cancellationToken = default);
}
