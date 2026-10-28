using Ambio.Infrastructure;
using Ambio.Infrastructure.Persistence;

using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;

namespace Ambio.E2E.Tests.Fixtures;

/// <summary>
/// Starts the app on an empty, migrated SQLite file, a temporary storage root and a headless Chromium browser. The app
/// creates the account (<see cref="UserEmail"/>, <see cref="UserPassword"/>) at start. Chromium is installed on first
/// run, so no separate install step is needed.
/// </summary>
public sealed class AmbioAppFixture : IAsyncLifetime
{
    public const string UserEmail = "user@example.com";
    public const string UserPassword = "Passw0rd!";

    private readonly string _databasePath = Path.Combine(Path.GetTempPath(), $"ambio-e2e-{Guid.NewGuid()}.db");
    private readonly string _storageRoot = Path.Combine(Path.GetTempPath(), $"ambio-e2e-{Guid.NewGuid()}");
    private AmbioAppFactory _factory = null!;
    private IPlaywright _playwright = null!;

    public IBrowser Browser { get; private set; } = null!;

    public string BaseUrl { get; private set; } = null!;

    public async ValueTask InitializeAsync()
    {
        var connectionString = $"DataSource={_databasePath}";

        // The app reads the account table at start, so the file is migrated first, with the production wiring of
        // the context (Identity's schema version comes from its options).
        var configuration = new ConfigurationManager { ["ConnectionStrings:DefaultConnection"] = connectionString };
        await using (var services = new ServiceCollection().AddLogging().AddDatabase(configuration).BuildServiceProvider())
        await using (var context = await services
                         .GetRequiredService<IDbContextFactory<ApplicationDbContext>>()
                         .CreateDbContextAsync())
        {
            await context.Database.MigrateAsync();
        }

        _factory = new AmbioAppFactory(connectionString, _storageRoot);
        _factory.StartServer();

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

    /// <summary>Opens a page on the app, logged in as <see cref="UserEmail"/>. The <paramref name="options"/>
    /// (viewport, color scheme) are merged with the base URL of the app; the context closes with the browser.</summary>
    public async Task<IPage> NewLoggedInPageAsync(BrowserNewContextOptions? options = null)
    {
        var context = await Browser.NewContextAsync(new BrowserNewContextOptions(options ?? new()) { BaseURL = BaseUrl });
        var page = await context.NewPageAsync();
        await page.LogInAsync(UserEmail, UserPassword);
        return page;
    }

    public async ValueTask DisposeAsync()
    {
        await Browser.DisposeAsync();
        _playwright.Dispose();
        await _factory.DisposeAsync();

        SqliteConnection.ClearAllPools();
        File.Delete(_databasePath);
        if (Directory.Exists(_storageRoot))
        {
            Directory.Delete(_storageRoot, recursive: true);
        }
    }
}
