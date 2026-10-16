using Ambio.Application.Common;
using Ambio.Application.JobApplications.Dtos;

namespace Ambio.Application.JobApplications;

public interface IJobApplicationService
{
    /// <summary>Active applications (not archived), newest first: by sending day, or by creation day for a draft.</summary>
    public Task<IReadOnlyList<JobApplicationSummaryDto>> ListAsync(CancellationToken cancellationToken = default);

    /// <summary>Active applications to the offer, newest first.</summary>
    public Task<IReadOnlyList<JobApplicationSummaryDto>> ListByOfferAsync(Guid offerId, CancellationToken cancellationToken = default);

    /// <summary>Active applications to the offers of the company, newest first.</summary>
    public Task<IReadOnlyList<JobApplicationSummaryDto>> ListByCompanyAsync(Guid companyId, CancellationToken cancellationToken = default);

    /// <summary>Creates an application to the offer, as a draft or already sent, with a free short id. Fails with
    /// <see cref="JobApplicationErrors.OfferNotFound"/>, or with <see cref="JobApplicationErrors.PostingNotFound"/>
    /// when the posting isn't one of the offer's.</summary>
    public Task<ServiceResult> CreateAsync(NewOfferApplicationInput input, CancellationToken cancellationToken = default);
}
