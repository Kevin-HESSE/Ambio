using System.ComponentModel.DataAnnotations;

using Ambio.Application.Settings.Dtos;
using Ambio.Domain.Settings;

namespace Ambio.Application.Tests.Settings.Dtos;

public class UserSettingsInputValidationTests
{
    [Fact]
    public void Empty_FailsOnFollowUpAfterDays()
    {
        var result = Assert.Single(Validate(new UserSettingsInput()));

        Assert.Equal([nameof(UserSettingsInput.FollowUpAfterDays)], result.MemberNames);
        Assert.Equal(UserSettingsInput.FollowUpAfterDaysRequired, result.ErrorMessage);
    }

    [Theory]
    [InlineData(UserSettings.MinFollowUpAfterDays - 1)]
    [InlineData(UserSettings.MaxFollowUpAfterDays + 1)]
    public void OutOfBounds_FailsOnFollowUpAfterDays(int days)
    {
        var result = Assert.Single(Validate(new UserSettingsInput { FollowUpAfterDays = days }));

        Assert.Equal([nameof(UserSettingsInput.FollowUpAfterDays)], result.MemberNames);
    }

    [Theory]
    [InlineData(UserSettings.MinFollowUpAfterDays)]
    [InlineData(UserSettings.MaxFollowUpAfterDays)]
    public void WithinBounds_HasNoErrors(int days)
    {
        Assert.Empty(Validate(new UserSettingsInput { FollowUpAfterDays = days }));
    }

    private static List<ValidationResult> Validate(UserSettingsInput input)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(input, new ValidationContext(input), results, validateAllProperties: true);
        return results;
    }
}
