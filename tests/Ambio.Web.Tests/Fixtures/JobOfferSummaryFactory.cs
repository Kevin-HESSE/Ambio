using Ambio.Application.Common;
using Ambio.Application.Offers.Dtos;
using Ambio.Domain.Offers;

namespace Ambio.Web.Tests.Fixtures;

/// <summary>Rows of the offers lists, as returned by the mocked <c>IJobOfferService</c>.</summary>
public static class JobOfferSummaryFactory
{
    /// <summary>An offer of the company, whose slug is the one of its name. Only what a test checks needs to be
    /// given.</summary>
    public static JobOfferSummaryDto New(
        string title,
        string companyName = "Kelvio",
        ContractType contractType = ContractType.Permanent,
        RemotePolicy remotePolicy = RemotePolicy.Hybrid,
        int postingCount = 1,
        DateTime? registerAt = null) => new(
            Id: Guid.CreateVersion7(),
            Title: title,
            CompanyName: companyName,
            CompanySlug: Slug.From(companyName, "company"),
            ContractType: contractType,
            RemotePolicy: remotePolicy,
            PostingCount: postingCount,
            RegisterAt: registerAt ?? new DateTime(2026, 8, 28, 9, 0, 0, DateTimeKind.Utc));
}
