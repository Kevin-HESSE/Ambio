using Ambio.Application.Common;
using Ambio.Application.Offers.Dtos;

namespace Ambio.Application.Offers;

public interface IJobOfferService
{
    /// <summary>Active offers (not archived), newest first.</summary>
    public Task<IReadOnlyList<JobOfferSummaryDto>> ListAsync(CancellationToken cancellationToken = default);

    /// <summary>Active offers of the company, newest first.</summary>
    public Task<IReadOnlyList<JobOfferSummaryDto>> ListByCompanyAsync(Guid companyId, CancellationToken cancellationToken = default);

    /// <summary>Active offers of the company with this name, ignoring case, archived or not, whose title is similar
    /// (<see cref="Ambio.Domain.Offers.JobOfferTitles.AreSimilar"/>), newest first: an offer about to be created may
    /// already exist. Empty when no company has this name.</summary>
    public Task<IReadOnlyList<JobOfferSummaryDto>> FindSimilarAsync(string companyName, string title, CancellationToken cancellationToken = default);

    /// <summary>Active offers whose title and company name contain every word of <paramref name="text"/>, ignoring
    /// case and accents (<see cref="TextSearch.Matches"/>), newest first. Empty for a text without any word.</summary>
    public Task<IReadOnlyList<JobOfferSummaryDto>> SearchAsync(string text, CancellationToken cancellationToken = default);

    /// <summary>Creates the offer with its first posting and returns what its URL is built from. The offer goes to the
    /// company with this name ignoring case, archived or not; when there is none, the company is created as an
    /// employer.</summary>
    public Task<JobOfferLinkDto> CreateAsync(NewJobOfferInput input, CancellationToken cancellationToken = default);

    /// <summary>The offer with its postings, archived or not, or null when no offer has this short id.</summary>
    public Task<JobOfferDetailsDto?> GetAsync(string shortId, CancellationToken cancellationToken = default);

    /// <summary>Replaces the fields of the offer, but not its company nor its postings, and returns what its URL is
    /// now built from (a new title gives it a new slug), or fails with <see cref="JobOfferErrors.NotFound"/>.</summary>
    public Task<ServiceResult<JobOfferLinkDto>> UpdateAsync(Guid id, JobOfferInput input, CancellationToken cancellationToken = default);

    /// <summary>Records another site where the offer was seen, or fails with <see cref="JobOfferErrors.NotFound"/>,
    /// or with <see cref="JobOfferErrors.PostingAlreadyExists"/> when the offer already has this link.</summary>
    public Task<ServiceResult> AddPostingAsync(Guid offerId, JobOfferPostingInput input, CancellationToken cancellationToken = default);

    /// <summary>Removes a posting of the offer, or fails with <see cref="JobOfferErrors.NotFound"/>,
    /// <see cref="JobOfferErrors.PostingNotFound"/>, or <see cref="JobOfferErrors.LastPosting"/> when it is the
    /// offer's only posting.</summary>
    public Task<ServiceResult> RemovePostingAsync(Guid offerId, Guid postingId, CancellationToken cancellationToken = default);
}
