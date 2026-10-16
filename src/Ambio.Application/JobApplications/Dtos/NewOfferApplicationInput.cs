using System.ComponentModel.DataAnnotations;

namespace Ambio.Application.JobApplications.Dtos;

/// <summary>The fields of the new application form, for an application to an offer.</summary>
public record NewOfferApplicationInput : NewJobApplicationInput
{
    [Required(ErrorMessage = "Choose the offer you apply to.")]
    public Guid? JobOfferId { get; set; }

    /// <summary>One of the offer's postings, whose site was used to apply; null when not specified.</summary>
    public Guid? AppliedViaPostingId { get; set; }
}
