using Ambio.Domain.Settings;

namespace Ambio.Domain.Tests.Settings;

public class UserSettingsTests
{
    [Fact]
    public void New_GeneratesVersion7Id()
    {
        Assert.Equal(7, new UserSettings().Id.Version);
    }

    [Fact]
    public void New_FollowsUpAfterTheDefaultDelay()
    {
        Assert.Equal(UserSettings.DefaultFollowUpAfterDays, new UserSettings().FollowUpAfterDays);
    }

    [Theory]
    [InlineData(UserSettings.MinFollowUpAfterDays)]
    [InlineData(UserSettings.MaxFollowUpAfterDays)]
    public void FollowUpAfterDays_AtTheBounds_IsKept(int days)
    {
        var settings = new UserSettings { FollowUpAfterDays = days };

        Assert.Equal(days, settings.FollowUpAfterDays);
    }

    [Theory]
    [MemberData(nameof(OutOfBoundsDelays))]
    public void FollowUpAfterDays_OutOfBounds_ThrowsArgumentOutOfRangeException(int days)
    {
        var settings = new UserSettings();

        Assert.Throws<ArgumentOutOfRangeException>(() => settings.FollowUpAfterDays = days);
    }

    public static TheoryData<int> OutOfBoundsDelays =>
    [
        0,
        -1,
        31,
    ];
}
