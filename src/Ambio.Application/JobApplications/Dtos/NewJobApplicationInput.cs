using System.ComponentModel.DataAnnotations;

using Ambio.Domain.JobApplications;

namespace Ambio.Application.JobApplications.Dtos;

/// <summary>The fields of the new application form shared by every kind: how and when it was sent.</summary>
public abstract record NewJobApplicationInput : IValidatableObject
{
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
