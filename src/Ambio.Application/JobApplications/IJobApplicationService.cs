using Ambio.Application.Common;
using Ambio.Application.Companies.Dtos;
using Ambio.Application.JobApplications.Dtos;

namespace Ambio.Application.JobApplications;

public interface IJobApplicationService
{
    /// <summary>The first <paramref name="count"/> applications that match the filter, newest first: by sending day, or
    /// by creation day for a draft. Only the active ones (not archived) unless the filter includes the archived ones.
    /// The search matches every word in the title and the company, ignoring case and accents
    /// (<see cref="TextSearch.Matches"/>).</summary>
    public Task<JobApplicationListDto> ListAsync(JobApplicationFilter filter, int count, CancellationToken cancellationToken = default);

    /// <summary>The companies the active applications were sent to, to their offers or spontaneously, by name: the
    /// choices of the company filter. With <paramref name="includeArchived"/>, those of the archived applications
    /// too. A company archived since is kept.</summary>
    public Task<IReadOnlyList<CompanySummaryDto>> ListCompaniesAsync(bool includeArchived = false, CancellationToken cancellationToken = default);

    /// <summary>Active applications to the offer, and the archived ones too with <paramref name="includeArchived"/>,
    /// newest first.</summary>
    public Task<IReadOnlyList<JobApplicationSummaryDto>> ListByOfferAsync(Guid offerId, bool includeArchived = false, CancellationToken cancellationToken = default);

    /// <summary>Active applications to the company, to its offers or spontaneous, and the archived ones too with
    /// <paramref name="includeArchived"/>, newest first.</summary>
    public Task<IReadOnlyList<JobApplicationSummaryDto>> ListByCompanyAsync(Guid companyId, bool includeArchived = false, CancellationToken cancellationToken = default);

    /// <summary>The application with this short id, archived or not, with its status history: an
    /// <see cref="OfferApplicationDetailsDto"/> or a <see cref="SpontaneousApplicationDetailsDto"/>, or null when no
    /// application has it.</summary>
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

    /// <summary>Moves the application to the new status, as the status workflow allows, and records the change with its
    /// comment; sending a draft records the day it was sent. Fails with <see cref="JobApplicationErrors.NotFound"/>,
    /// with <see cref="JobApplicationErrors.Archived"/> when the application is archived, or with
    /// <see cref="JobApplicationErrors.StatusNotAllowed"/> when the new status can't follow the current one.</summary>
    public Task<ServiceResult> ChangeStatusAsync(Guid id, StatusChangeInput input, CancellationToken cancellationToken = default);

    /// <summary>Archives the application, or fails with <see cref="JobApplicationErrors.NotFound"/>. An archived
    /// application is left as it is.</summary>
    public Task<ServiceResult> ArchiveAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Restores the application, and its offer and company when archived, but not their other offers and
    /// applications. Fails with <see cref="JobApplicationErrors.NotFound"/>. An active application is left as it
    /// is.</summary>
    public Task<ServiceResult> RestoreAsync(Guid id, CancellationToken cancellationToken = default);
}
