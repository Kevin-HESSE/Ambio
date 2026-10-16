using System.ComponentModel.DataAnnotations;

using Ambio.Domain.JobApplications;

namespace Ambio.Application.JobApplications.Dtos;

/// <summary>The fields of the new application form, for an application to an offer.</summary>
public record NewOfferApplicationInput : IValidatableObject
{
    [Required(ErrorMessage = "Choose the offer you apply to.")]
    public Guid? JobOfferId { get; set; }

    /// <summary>One of the offer's postings, whose site was used to apply; null when not specified.</summary>
    public Guid? AppliedViaPostingId { get; set; }

    public Channel Channel { get; set; } = Channel.OnlineForm;

    [StringLength(200, ErrorMessage = "The channel detail can't be longer than {1} characters.")]
    public string? ChannelDetail { get; set; }

    /// <summary>Already sent on <see cref="SentOn"/>; otherwise the application is a draft.</summary>
    public bool AlreadySent { get; set; }

    public DateOnly? SentOn { get; set; }

    public string? Notes { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (AlreadySent && SentOn is null)
        {
            yield return new ValidationResult("Enter the day you sent it.", [nameof(SentOn)]);
        }
    }
}
