using Ambio.Application.Common;
using Ambio.Application.JobApplications.Dtos;

namespace Ambio.Application.JobApplications;

public interface IJobApplicationService
{
    /// <summary>Active applications (not archived), newest first: by sending day, or by creation day for a draft.</summary>
    public Task<IReadOnlyList<JobApplicationSummaryDto>> ListAsync(CancellationToken cancellationToken = default);

    /// <summary>Active applications to the offer, newest first.</summary>
    public Task<IReadOnlyList<JobApplicationSummaryDto>> ListByOfferAsync(Guid offerId, CancellationToken cancellationToken = default);

    /// <summary>Active applications to the company, to its offers or spontaneous, newest first.</summary>
    public Task<IReadOnlyList<JobApplicationSummaryDto>> ListByCompanyAsync(Guid companyId, CancellationToken cancellationToken = default);

    /// <summary>The application with this short id, archived or not: an <see cref="OfferApplicationDetailsDto"/> or a
    /// <see cref="SpontaneousApplicationDetailsDto"/>, or null when no application has it.</summary>
    public Task<JobApplicationDetailsDto?> GetAsync(string shortId, CancellationToken cancellationToken = default);

    /// <summary>Creates an application to the offer, as a draft or already sent, with a free short id, and returns
    /// what its URL is built from. Fails with <see cref="JobApplicationErrors.OfferNotFound"/>, or with
    /// <see cref="JobApplicationErrors.PostingNotFound"/> when the posting isn't one of the offer's.</summary>
    public Task<ServiceResult<JobApplicationLinkDto>> CreateAsync(NewOfferApplicationInput input, CancellationToken cancellationToken = default);

    /// <summary>Creates a spontaneous application, as a draft or already sent, with a free short id, and returns
    /// what its URL is built from. Its company is the one with this name, compared ignoring case and archived or
    /// not, or a new employer created in the same save.</summary>
    public Task<JobApplicationLinkDto> CreateAsync(NewSpontaneousApplicationInput input, CancellationToken cancellationToken = default);

    /// <summary>Replaces the fields of the application to an offer, but not its offer nor its status, and returns what
    /// its URL is built from. The sending day is corrected only once sent. Fails with
    /// <see cref="JobApplicationErrors.NotFound"/>, or with <see cref="JobApplicationErrors.PostingNotFound"/> when the
    /// posting isn't one of the offer's.</summary>
    public Task<ServiceResult<JobApplicationLinkDto>> UpdateAsync(Guid id, OfferApplicationInput input, CancellationToken cancellationToken = default);

    /// <summary>Replaces the fields of the spontaneous application, but not its company nor its status, and returns
    /// what its URL is built from, whose slug follows a new target position. The sending day is corrected only once
    /// sent. Fails with <see cref="JobApplicationErrors.NotFound"/>.</summary>
    public Task<ServiceResult<JobApplicationLinkDto>> UpdateAsync(Guid id, SpontaneousApplicationInput input, CancellationToken cancellationToken = default);
}
