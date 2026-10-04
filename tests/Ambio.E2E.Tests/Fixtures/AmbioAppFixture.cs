using Ambio.Infrastructure.Persistence;

using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;

namespace Ambio.E2E.Tests.Fixtures;

/// <summary>
/// Starts the app on an empty, migrated SQLite file and a headless Chromium browser.
/// Chromium is installed on first run, so no separate install step is needed.
/// </summary>
public sealed class AmbioAppFixture : IAsyncLifetime
{
    private readonly string _databasePath = Path.Combine(Path.GetTempPath(), $"ambio-e2e-{Guid.NewGuid()}.db");
    private AmbioAppFactory _factory = null!;
    private IPlaywright _playwright = null!;

    public IBrowser Browser { get; private set; } = null!;

    public string BaseUrl { get; private set; } = null!;

    public async ValueTask InitializeAsync()
    {
        _factory = new AmbioAppFactory($"DataSource={_databasePath}");
        _factory.StartServer();

        await using (var context = await _factory.Services
                         .GetRequiredService<IDbContextFactory<ApplicationDbContext>>()
                         .CreateDbContextAsync())
        {
            await context.Database.MigrateAsync();
        }

        BaseUrl = _factory.Services.GetRequiredService<IServer>()
            .Features.GetRequiredFeature<IServerAddressesFeature>()
            .Addresses.First();

        var exitCode = Microsoft.Playwright.Program.Main(["install", "chromium"]);
        if (exitCode != 0)
        {
            throw new InvalidOperationException($"Playwright failed to install Chromium (exit code {exitCode}).");
        }

        _playwright = await Playwright.CreateAsync();
        Browser = await _playwright.Chromium.LaunchAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await Browser.DisposeAsync();
        _playwright.Dispose();
        await _factory.DisposeAsync();

        SqliteConnection.ClearAllPools();
        File.Delete(_databasePath);
    }
}
