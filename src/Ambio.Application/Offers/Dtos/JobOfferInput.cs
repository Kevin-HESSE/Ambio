using System.ComponentModel.DataAnnotations;

using Ambio.Application.Common.Attributes;
using Ambio.Domain.Offers;

namespace Ambio.Application.Offers.Dtos;

/// <summary>The fields of the new offer form: the offer, where it was seen (its first posting) and the ad text.</summary>
public record JobOfferInput
{
    /// <summary>The name of an existing company, compared ignoring case, or of a company to create.</summary>
    [Required(ErrorMessage = "Enter the name of the company.")]
    [StringLength(200, ErrorMessage = "The company name can't be longer than {1} characters.")]
    public string CompanyName { get; set; } = "";

    [Required(ErrorMessage = "Enter the title of the offer.")]
    [StringLength(200, ErrorMessage = "The title can't be longer than {1} characters.")]
    public string Title { get; set; } = "";

    public ContractType ContractType { get; set; } = ContractType.Permanent;

    public RemotePolicy RemotePolicy { get; set; } = RemotePolicy.Unknown;

    public string? Location { get; set; }

    public string? Salary { get; set; }

    [Required(ErrorMessage = "Enter the site where you saw the offer.")]
    public string Site { get; set; } = "";

    [Required(ErrorMessage = "Enter the link to the offer.")]
    [HttpUrl]
    public string Url { get; set; } = "";

    /// <summary>Plain-text copy of the ad.</summary>
    public string? Description { get; set; }
}
