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
            ShortId: o.ShortId,
            Title: o.Title,
            CompanyName: o.Company.Name,
            CompanySlug: o.Company.Slug,
            ContractType: o.ContractType,
            RemotePolicy: o.RemotePolicy,
            PostingCount: o.Postings.Count,
            RegisterAt: o.CreatedAt));

    /// <summary>Projection translated to SQL by EF Core, with the company and the postings, from the first seen.</summary>
    public static IQueryable<JobOfferDetailsDto> ToDetailsDto(this IQueryable<JobOffer> offers) =>
        offers.Select(o => new JobOfferDetailsDto(
            Id: o.Id,
            ShortId: o.ShortId,
            Title: o.Title,
            CompanyName: o.Company.Name,
            CompanySlug: o.Company.Slug,
            ContractType: o.ContractType,
            RemotePolicy: o.RemotePolicy,
            Location: o.Location,
            Salary: o.Salary,
            Description: o.Description,
            RegisterAt: o.CreatedAt,
            Postings: o.Postings
                .OrderBy(p => p.CreatedAt)
                .Select(p => new JobOfferPostingDto(
                    Id: p.Id,
                    Site: p.Site,
                    Url: p.Url,
                    SeenAt: p.CreatedAt))
                .ToList()));

    /// <summary>A new offer of the company with the fields of the form, see <see cref="ApplyTo"/>, and its first
    /// posting.</summary>
    public static JobOffer ToEntity(this NewJobOfferInput input, Guid companyId, string shortId)
    {
        var offer = new JobOffer
        {
            ShortId = shortId,
            CompanyId = companyId,
            Title = input.Title,
            ContractType = input.ContractType,
            RemotePolicy = input.RemotePolicy,
        };
        input.ApplyTo(offer);
        offer.AddPosting(input.Site, input.Url);
        return offer;
    }

    /// <summary>The site and the link of the new offer form, to add them as a posting of an existing offer.</summary>
    public static JobOfferPostingInput ToPostingInput(this NewJobOfferInput input) => new()
    {
        Site = input.Site,
        Url = input.Url,
    };

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

    /// <summary>The edit form, filled with the current values of the offer.</summary>
    public static JobOfferInput ToInput(this JobOfferDetailsDto offer) => new()
    {
        Title = offer.Title,
        ContractType = offer.ContractType,
        RemotePolicy = offer.RemotePolicy,
        Location = offer.Location,
        Salary = offer.Salary,
        Description = offer.Description,
    };
}
