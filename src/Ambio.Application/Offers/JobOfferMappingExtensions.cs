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

    /// <summary>A new offer of the company with the fields of the form, see <see cref="ApplyTo"/>, and its first
    /// posting.</summary>
    public static JobOffer ToEntity(this NewJobOfferInput input, Guid companyId)
    {
        var offer = new JobOffer
        {
            CompanyId = companyId,
            Title = input.Title,
            ContractType = input.ContractType,
            RemotePolicy = input.RemotePolicy,
        };
        input.ApplyTo(offer);
        offer.AddPosting(input.Site, input.Url);
        return offer;
    }

    /// <summary>Copies the fields of the form onto the offer, but not its company nor its postings. The optional
    /// text fields are trimmed, and a blank one is stored as null.</summary>
    public static void ApplyTo(this JobOfferInput input, JobOffer offer)
    {
        offer.Title = input.Title;
        offer.ContractType = input.ContractType;
        offer.RemotePolicy = input.RemotePolicy;
        offer.Location = input.Location.NullIfBlank();
        offer.Salary = input.Salary.NullIfBlank();
        offer.Description = input.Description.NullIfBlank();
    }
}
