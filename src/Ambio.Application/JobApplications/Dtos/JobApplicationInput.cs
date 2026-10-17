using System.ComponentModel.DataAnnotations;

using Ambio.Domain.JobApplications;

namespace Ambio.Application.JobApplications.Dtos;

/// <summary>The fields of the application forms shared by every kind, to create or edit an application: how and when
/// it was sent.</summary>
public abstract record JobApplicationInput : IValidatableObject
{
    public const string SentOnRequired = "Enter the day you sent it.";

    public Channel Channel { get; set; } = Channel.OnlineForm;

    [StringLength(200, ErrorMessage = "The channel detail can't be longer than {1} characters.")]
    public string? ChannelDetail { get; set; }

    /// <summary>Already sent on <see cref="SentOn"/>; otherwise the application is a draft. The new application form
    /// sets it with a checkbox, the edit form from the application: a draft is sent by changing its status.</summary>
    public bool AlreadySent { get; set; }

    public DateOnly? SentOn { get; set; }

    public string? Notes { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (AlreadySent && SentOn is null)
        {
            yield return new ValidationResult(SentOnRequired, [nameof(SentOn)]);
        }
    }
}
