using Ambio.Application.Common;
using Ambio.Application.Offers.Dtos;
using Ambio.Domain.Offers;

namespace Ambio.Application.Offers;

public static class JobOfferMappingExtensions
{
    /// <summary>Projection translated to SQL by EF Core, with the company and the postings count.</summary>
    public static IQueryable<JobOfferSummaryDto> ToSummaryDto(this IQueryable<JobOffer> offers) =>
        offers.Select(o => new JobOfferSummaryDto(
            Id: o.Id,
            Title: o.Title,
            CompanyName: o.Company.Name,
            CompanySlug: o.Company.Slug,
            ContractType: o.ContractType,
            RemotePolicy: o.RemotePolicy,
            PostingCount: o.Postings.Count,
            RegisterAt: o.CreatedAt));

    /// <summary>A new offer of the company with the fields of the form and its first posting. The optional text
    /// fields are trimmed, and a blank one is stored as null.</summary>
    public static JobOffer ToEntity(this JobOfferInput input, Guid companyId)
    {
        var offer = new JobOffer
        {
            CompanyId = companyId,
            Title = input.Title,
            ContractType = input.ContractType,
            RemotePolicy = input.RemotePolicy,
            Location = input.Location.NullIfBlank(),
            Salary = input.Salary.NullIfBlank(),
            Description = input.Description.NullIfBlank(),
        };
        offer.AddPosting(input.Site, input.Url);
        return offer;
    }
}
