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
}
