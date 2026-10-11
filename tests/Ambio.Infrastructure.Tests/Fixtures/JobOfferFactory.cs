using Ambio.Domain.Offers;

namespace Ambio.Infrastructure.Tests.Fixtures;

/// <summary>Job offers to save straight through the DbContext, without the service.</summary>
public static class JobOfferFactory
{
    /// <summary>An offer of the company with only the required fields, and no posting.</summary>
    public static JobOffer New(Guid companyId, string title = "Backend Developer") => new()
    {
        CompanyId = companyId,
        Title = title,
        ContractType = ContractType.Permanent,
        RemotePolicy = RemotePolicy.Hybrid,
    };
}
