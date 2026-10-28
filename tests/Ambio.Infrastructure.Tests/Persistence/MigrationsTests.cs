using Ambio.Infrastructure.Tests.Fixtures;

using Microsoft.EntityFrameworkCore;

namespace Ambio.Infrastructure.Tests.Persistence;

public class MigrationsTests : IAsyncLifetime
{
    private SqliteServiceProvider _services = null!;

    public async ValueTask InitializeAsync() => _services = await SqliteServiceProvider.CreateAsync();

    public ValueTask DisposeAsync() => _services.DisposeAsync();

    [Fact]
    public async Task HasPendingModelChanges_WithTheCurrentMigrations_IsFalse()
    {
        // The app migrates its database at start, and a model change without its migration would stop it.
        var hasPendingChanges = await _services.QueryAsync(context => Task.FromResult(context.Database.HasPendingModelChanges()));

        Assert.False(hasPendingChanges);
    }
}
