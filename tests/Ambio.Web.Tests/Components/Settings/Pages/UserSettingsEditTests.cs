using Ambio.Application.Settings;
using Ambio.Application.Settings.Dtos;
using Ambio.Domain.Settings;
using Ambio.Web.Components.Layout;
using Ambio.Web.Components.Settings.Pages;

using Bunit;

using Microsoft.Extensions.DependencyInjection;

using NSubstitute;

namespace Ambio.Web.Tests.Components.Settings.Pages;

public class UserSettingsEditTests : BunitContext
{
    private const int SavedDays = 14;
    private const int NewDays = 21;
    private const string DelayField = "#follow-up-after-days";

    private readonly IUserSettingsService _userSettingsService = Substitute.For<IUserSettingsService>();
    private readonly ToastService _toasts = new();

    public UserSettingsEditTests()
    {
        Services.AddSingleton(_userSettingsService);
        Services.AddSingleton(_toasts);
        _userSettingsService.GetAsync(Arg.Any<CancellationToken>()).Returns(new UserSettingsDto(SavedDays));
    }

    [Fact]
    public void Render_WithSettings_PrefillsTheDelay()
    {
        var cut = Render<UserSettingsEdit>();

        Assert.Equal(SavedDays.ToString(), cut.Find(DelayField).GetAttribute("value"));
    }

    [Fact]
    public void Submit_WithDelay_SavesAndShowsToast()
    {
        var cut = Render<UserSettingsEdit>();

        cut.Find(DelayField).Change(NewDays.ToString());
        cut.Find("form").Submit();

        _userSettingsService.Received(1).UpdateAsync(
            Arg.Is<UserSettingsInput>(input => input.FollowUpAfterDays == NewDays),
            Arg.Any<CancellationToken>());
        Assert.Equal("Settings saved", _toasts.Current?.Message);
    }

    [Theory]
    [MemberData(nameof(InvalidDelays))]
    public void Submit_WithInvalidDelay_ShowsErrorWithoutSaving(string days, string error)
    {
        var cut = Render<UserSettingsEdit>();

        cut.Find(DelayField).Change(days);
        cut.Find("form").Submit();

        Assert.Equal(error, cut.Find(".validation-message").TextContent);
        _userSettingsService.DidNotReceive().UpdateAsync(Arg.Any<UserSettingsInput>(), Arg.Any<CancellationToken>());
        Assert.Null(_toasts.Current);
    }

    private static string OutOfRange => string.Format(
        UserSettingsInput.FollowUpAfterDaysOutOfRange,
        nameof(UserSettingsInput.FollowUpAfterDays),
        UserSettings.MinFollowUpAfterDays,
        UserSettings.MaxFollowUpAfterDays);

    public static TheoryData<string, string> InvalidDelays =>
        new()
        {
            { "", UserSettingsInput.FollowUpAfterDaysRequired },
            { "0", OutOfRange },
            { "31", OutOfRange },
        };
}
