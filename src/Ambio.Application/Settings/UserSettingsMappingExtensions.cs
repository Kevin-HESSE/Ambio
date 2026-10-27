using Ambio.Application.Settings.Dtos;
using Ambio.Domain.Settings;

namespace Ambio.Application.Settings;

public static class UserSettingsMappingExtensions
{
    /// <summary>Projection translated to SQL by EF Core.</summary>
    public static IQueryable<UserSettingsDto> ToDto(this IQueryable<UserSettings> settings) =>
        settings.Select(s => new UserSettingsDto(
            FollowUpAfterDays: s.FollowUpAfterDays));

    /// <summary>Copies the fields of the form onto the settings. The input must be valid.</summary>
    public static void ApplyTo(this UserSettingsInput input, UserSettings settings)
    {
        settings.FollowUpAfterDays = input.FollowUpAfterDays!.Value;
    }

    /// <summary>The form, filled with the current settings.</summary>
    public static UserSettingsInput ToInput(this UserSettingsDto settings) => new()
    {
        FollowUpAfterDays = settings.FollowUpAfterDays,
    };
}
