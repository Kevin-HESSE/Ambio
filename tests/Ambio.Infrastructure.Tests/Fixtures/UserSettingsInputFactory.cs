using Ambio.Application.Settings.Dtos;
using Ambio.Domain.Settings;

using Bogus;

namespace Ambio.Infrastructure.Tests.Fixtures;

/// <summary>Settings forms, as sent to <c>IUserSettingsService</c>.</summary>
public static class UserSettingsInputFactory
{
    /// <summary>Forms with a delay other than the default, so a test tells them from the defaults.</summary>
    public static Faker<UserSettingsInput> GetGenerator() =>
        new Faker<UserSettingsInput>()
            .RuleFor(s => s.FollowUpAfterDays, faker => faker.PickRandom(Enumerable
                .Range(UserSettings.MinFollowUpAfterDays, UserSettings.MaxFollowUpAfterDays)
                .Where(days => days != UserSettings.DefaultFollowUpAfterDays)));
}
