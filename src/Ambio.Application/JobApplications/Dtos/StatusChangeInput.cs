using System.ComponentModel.DataAnnotations;

using Ambio.Domain.JobApplications;

namespace Ambio.Application.JobApplications.Dtos;

/// <summary>The fields of the change status form: the new status, the day a draft was sent, and a comment.</summary>
public record StatusChangeInput : IValidatableObject
{
    [Required(ErrorMessage = "Choose the new status.")]
    public ApplicationStatus? To { get; set; }

    /// <summary>The day the draft was sent: only a draft moves to <see cref="ApplicationStatus.Applied"/>, and only
    /// then is it used.</summary>
    public DateOnly? SentOn { get; set; }

    public string? Comment { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (To == ApplicationStatus.Applied && SentOn is null)
        {
            yield return new ValidationResult(JobApplicationInput.SentOnRequired, [nameof(SentOn)]);
        }
    }
}
