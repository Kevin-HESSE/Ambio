using System.ComponentModel.DataAnnotations;

using Ambio.Application.Common.Attributes;

namespace Ambio.Application.Offers.Dtos;

/// <summary>The fields of the new offer form: the company, the offer and where it was seen (its first posting).</summary>
public record NewJobOfferInput : JobOfferInput
{
    /// <summary>The name of an existing company, compared ignoring case, or of a company to create.</summary>
    [Required(ErrorMessage = "Enter the name of the company.")]
    [StringLength(200, ErrorMessage = "The company name can't be longer than {1} characters.")]
    public string CompanyName { get; set; } = "";

    [Required(ErrorMessage = "Enter the site where you saw the offer.")]
    public string Site { get; set; } = "";

    [Required(ErrorMessage = "Enter the link to the offer.")]
    [HttpUrl]
    public string Url { get; set; } = "";
}
