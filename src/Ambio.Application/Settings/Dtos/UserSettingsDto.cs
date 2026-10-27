namespace Ambio.Application.Settings.Dtos;

/// <summary>The preferences of the account: an application still Applied after <paramref name="FollowUpAfterDays"/>
/// days needs a follow-up.</summary>
public record UserSettingsDto(int FollowUpAfterDays);
