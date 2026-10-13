using System.ComponentModel.DataAnnotations;

using Ambio.Application.Common.Attributes;

namespace Ambio.Application.Offers.Dtos;

/// <summary>The fields of the add posting form: another site where the offer was seen.</summary>
public record JobOfferPostingInput
{
    [Required(ErrorMessage = "Enter the site where you saw the offer.")]
    public string Site { get; set; } = "";

    [Required(ErrorMessage = "Enter the link to the offer.")]
    [HttpUrl]
    public string Url { get; set; } = "";
}
