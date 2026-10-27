using Ambio.Infrastructure.Tests.Fixtures;

using Microsoft.EntityFrameworkCore;

namespace Ambio.Infrastructure.Tests.Settings;

public class UserSettingsConfigurationTests : IAsyncLifetime
{
    private SqliteServiceProvider _services = null!;

    public async ValueTask InitializeAsync() => _services = await SqliteServiceProvider.CreateAsync();

    public ValueTask DisposeAsync() => _services.DisposeAsync();

    [Fact]
    public async Task SaveChangesAsync_Settings_SavesEveryField()
    {
        var settings = UserSettingsFactory.GetGenerator().Generate();

        await _services.AddAsync(settings);

        var saved = await _services.QueryAsync(context =>
            context.UserSettings.SingleAsync(TestContext.Current.CancellationToken));
        Assert.Equal((settings.Id, settings.FollowUpAfterDays), (saved.Id, saved.FollowUpAfterDays));
    }
}
