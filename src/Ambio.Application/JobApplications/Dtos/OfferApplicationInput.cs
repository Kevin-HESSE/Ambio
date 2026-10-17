namespace Ambio.Application.JobApplications.Dtos;

/// <summary>The fields of the application form, to create or edit an application to an offer.</summary>
public record OfferApplicationInput : JobApplicationInput
{
    /// <summary>One of the offer's postings, whose site was used to apply; null when not specified.</summary>
    public Guid? AppliedViaPostingId { get; set; }
}
