using System.ComponentModel.DataAnnotations;

namespace Ambio.Application.JobApplications.Dtos;

/// <summary>The fields of the new application form, for an application to an offer: the offer comes first.</summary>
public record NewOfferApplicationInput : OfferApplicationInput
{
    [Required(ErrorMessage = "Choose the offer you apply to.")]
    public Guid? JobOfferId { get; set; }
}
