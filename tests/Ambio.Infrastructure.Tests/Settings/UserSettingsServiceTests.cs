using Ambio.Application.Settings;
using Ambio.Application.Settings.Dtos;
using Ambio.Domain.Settings;
using Ambio.Infrastructure.Tests.Fixtures;

using Microsoft.EntityFrameworkCore;

namespace Ambio.Infrastructure.Tests.Settings;

public class UserSettingsServiceTests : IAsyncLifetime
{
    private SqliteServiceProvider _services = null!;

    public async ValueTask InitializeAsync() => _services = await SqliteServiceProvider.CreateAsync();

    public ValueTask DisposeAsync() => _services.DisposeAsync();

    [Fact]
    public async Task GetAsync_WithoutSettings_ReturnsTheDefaults()
    {
        var settings = await _services.CallAsync((IUserSettingsService service) =>
            service.GetAsync(TestContext.Current.CancellationToken));

        Assert.Equal(new UserSettingsDto(UserSettings.DefaultFollowUpAfterDays), settings);
    }

    [Fact]
    public async Task GetAsync_WithSettings_ReturnsThem()
    {
        var saved = UserSettingsFactory.GetGenerator().Generate();
        await _services.AddAsync(saved);

        var settings = await _services.CallAsync((IUserSettingsService service) =>
            service.GetAsync(TestContext.Current.CancellationToken));

        Assert.Equal(new UserSettingsDto(saved.FollowUpAfterDays), settings);
    }

    [Fact]
    public async Task UpdateAsync_WithoutSettings_SavesASingleRow()
    {
        var input = UserSettingsInputFactory.GetGenerator().Generate();

        await _services.CallAsync((IUserSettingsService service) =>
            service.UpdateAsync(input, TestContext.Current.CancellationToken));

        var saved = await _services.QueryAsync(context =>
            context.UserSettings.SingleAsync(TestContext.Current.CancellationToken));
        Assert.Equal(input.FollowUpAfterDays, saved.FollowUpAfterDays);
    }

    [Fact]
    public async Task UpdateAsync_WithSettings_UpdatesTheSameRow()
    {
        var existing = UserSettingsFactory.GetGenerator().Generate();
        await _services.AddAsync(existing);
        var input = UserSettingsInputFactory.GetGenerator().Generate();

        await _services.CallAsync((IUserSettingsService service) =>
            service.UpdateAsync(input, TestContext.Current.CancellationToken));

        var saved = await _services.QueryAsync(context =>
            context.UserSettings.SingleAsync(TestContext.Current.CancellationToken));
        Assert.Equal((existing.Id, input.FollowUpAfterDays), (saved.Id, saved.FollowUpAfterDays));
    }
}
