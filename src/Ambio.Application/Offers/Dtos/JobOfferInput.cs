using System.ComponentModel.DataAnnotations;

using Ambio.Domain.Offers;

namespace Ambio.Application.Offers.Dtos;

/// <summary>The fields of the offer form, to create or edit an offer.</summary>
public record JobOfferInput
{
    [Required(ErrorMessage = "Enter the title of the offer.")]
    [StringLength(200, ErrorMessage = "The title can't be longer than {1} characters.")]
    public string Title { get; set; } = "";

    public ContractType ContractType { get; set; } = ContractType.Permanent;

    public RemotePolicy RemotePolicy { get; set; } = RemotePolicy.Unknown;

    public string? Location { get; set; }

    public string? Salary { get; set; }

    /// <summary>Plain-text copy of the ad.</summary>
    public string? Description { get; set; }
}
