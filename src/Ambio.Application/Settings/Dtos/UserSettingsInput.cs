using System.ComponentModel.DataAnnotations;

using Ambio.Domain.Settings;

namespace Ambio.Application.Settings.Dtos;

/// <summary>The fields of the Settings form. The theme isn't one of them: the browser keeps it.</summary>
public record UserSettingsInput
{
    public const string FollowUpAfterDaysRequired = "Enter a number of days.";

    /// <summary>Formatted by <see cref="RangeAttribute"/>: <c>{1}</c> and <c>{2}</c> are the bounds.</summary>
    public const string FollowUpAfterDaysOutOfRange = "Enter a number of days between {1} and {2}.";

    [Required(ErrorMessage = FollowUpAfterDaysRequired)]
    [Range(UserSettings.MinFollowUpAfterDays, UserSettings.MaxFollowUpAfterDays, ErrorMessage = FollowUpAfterDaysOutOfRange)]
    public int? FollowUpAfterDays { get; set; }
}
