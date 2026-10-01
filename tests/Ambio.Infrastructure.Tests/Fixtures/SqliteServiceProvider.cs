using Ambio.Infrastructure.Persistence;

using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NSubstitute;

namespace Ambio.Infrastructure.Tests.Fixtures;

/// <summary>
/// Production service wiring (<see cref="InfrastructureExtensions"/>) over a private SQLite in-memory database.
/// The keeper connection stays open for the fixture's lifetime, so the database survives between contexts.
/// </summary>
public sealed class SqliteServiceProvider : IAsyncDisposable
{
    private readonly SqliteConnection _keeperConnection;
    private readonly ServiceProvider _provider;

    public IEmailSender<ApplicationUser> EmailSender { get; } = Substitute.For<IEmailSender<ApplicationUser>>();

    private SqliteServiceProvider(string connectionString)
    {
        _keeperConnection = new SqliteConnection(connectionString);
        _keeperConnection.Open();

        var configuration = new ConfigurationManager();
        configuration["ConnectionStrings:DefaultConnection"] = connectionString;

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDataProtection();
        services.AddDatabase(configuration);
        services.AddApplicationServices();
        services.AddSingleton<IEmailSender<ApplicationUser>>(EmailSender);

        _provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
    }

    public static async Task<SqliteServiceProvider> CreateAsync()
    {
        var provider = new SqliteServiceProvider($"DataSource=file:{Guid.NewGuid()}?mode=memory&cache=shared");

        await using var context = await provider.CreateDbContextAsync();
        await context.Database.EnsureCreatedAsync();

        return provider;
    }

    public AsyncServiceScope CreateScope() => _provider.CreateAsyncScope();

    public Task<ApplicationDbContext> CreateDbContextAsync() =>
        _provider.GetRequiredService<IDbContextFactory<ApplicationDbContext>>().CreateDbContextAsync();

    public async ValueTask DisposeAsync()
    {
        await _provider.DisposeAsync();
        await _keeperConnection.DisposeAsync();
    }
}
