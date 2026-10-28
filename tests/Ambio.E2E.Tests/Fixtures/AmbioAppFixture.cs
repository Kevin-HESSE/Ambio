
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;

namespace Ambio.E2E.Tests.Fixtures;

/// <summary>
/// Starts the app on a new SQLite file, a temporary storage root and a headless Chromium browser. The app applies the
/// migrations and creates the account (<see cref="UserEmail"/>, <see cref="UserPassword"/>) at start. Chromium is
/// installed on first run, so no separate install step is needed.
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
        // The app applies the migrations to the new file at start.
        _factory = new AmbioAppFactory($"DataSource={_databasePath}", _storageRoot);
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
