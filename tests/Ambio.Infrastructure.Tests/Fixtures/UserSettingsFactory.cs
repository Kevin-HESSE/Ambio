using Ambio.Domain.Settings;

using Bogus;

namespace Ambio.Infrastructure.Tests.Fixtures;

/// <summary>Settings to save straight through the DbContext, without the service.</summary>
public static class UserSettingsFactory
{
    /// <summary>Settings with a delay other than the default, so a test tells them from the defaults.</summary>
    public static Faker<UserSettings> GetGenerator() =>
        new Faker<UserSettings>()
            .RuleFor(s => s.FollowUpAfterDays, faker => faker.PickRandom(Enumerable
                .Range(UserSettings.MinFollowUpAfterDays, UserSettings.MaxFollowUpAfterDays)
                .Where(days => days != UserSettings.DefaultFollowUpAfterDays)));
}
