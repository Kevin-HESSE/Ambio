using Ambio.Infrastructure.Persistence;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using NSubstitute;

namespace Ambio.Infrastructure.Tests.Fixtures;

/// <summary>
/// Production service wiring (<see cref="InfrastructureExtensions"/>) over a private SQLite in-memory database.
/// The keeper connection stays open for the fixture's lifetime, so the database survives between contexts.
/// The documents are stored under a temporary <see cref="ContentRootPath"/>, deleted with the fixture.
/// </summary>
public sealed class SqliteServiceProvider : IAsyncDisposable
{
    private readonly SqliteConnection _keeperConnection;
    private readonly ServiceProvider _provider;

    public TimeProvider TimeProvider { get; } = Substitute.For<TimeProvider>();

    /// <summary>The content root of the app, a new temporary folder: the default storage root <c>Data</c> is under it.</summary>
    public string ContentRootPath { get; } = Path.Combine(Path.GetTempPath(), $"ambio-tests-{Guid.NewGuid()}");

    private SqliteServiceProvider(string connectionString, IReadOnlyDictionary<string, string?>? settings)
    {
        _keeperConnection = new SqliteConnection(connectionString);
        _keeperConnection.Open();

        var configuration = new ConfigurationManager();
        configuration["ConnectionStrings:DefaultConnection"] = connectionString;
        foreach (var (key, value) in settings ?? new Dictionary<string, string?>())
        {
            configuration[key] = value;
        }

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging();
        services.AddDataProtection();
        services.AddSingleton(TimeProvider);
        var environment = Substitute.For<IHostEnvironment>();
        environment.ContentRootPath.Returns(ContentRootPath);
        services.AddSingleton(environment);
        services.AddDatabase(configuration);
        services.AddApplicationServices();

        _provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
    }

    /// <summary>A provider over a new empty database. <paramref name="settings"/> override the app configuration,
    /// such as <c>Ambio:ShortId:Alphabet</c>.</summary>
    public static async Task<SqliteServiceProvider> CreateAsync(IReadOnlyDictionary<string, string?>? settings = null)
    {
        var provider = new SqliteServiceProvider($"DataSource=file:{Guid.NewGuid()}?mode=memory&cache=shared", settings);

        await using var context = await provider.CreateDbContextAsync();
        await context.Database.EnsureCreatedAsync();

        return provider;
    }

    public AsyncServiceScope CreateScope() => _provider.CreateAsyncScope();

    public Task<ApplicationDbContext> CreateDbContextAsync() =>
        _provider.GetRequiredService<IDbContextFactory<ApplicationDbContext>>().CreateDbContextAsync();

    /// <summary>Saves the entities straight through a new context, without any service.</summary>
    public async Task AddAsync<TEntity>(params TEntity[] entities) where TEntity : class
    {
        await using var context = await CreateDbContextAsync();
        context.Set<TEntity>().AddRange(entities);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>Calls a service resolved in its own scope, as a request of the app would.</summary>
    public async Task<TResult> CallAsync<TService, TResult>(Func<TService, Task<TResult>> call) where TService : notnull
    {
        await using var scope = CreateScope();
        return await call(scope.ServiceProvider.GetRequiredService<TService>());
    }

    /// <summary>Calls a service that returns nothing, resolved in its own scope, as a request of the app would.</summary>
    public async Task CallAsync<TService>(Func<TService, Task> call) where TService : notnull
    {
        await using var scope = CreateScope();
        await call(scope.ServiceProvider.GetRequiredService<TService>());
    }

    /// <summary>Reads what was saved, on a new context.</summary>
    public async Task<TResult> QueryAsync<TResult>(Func<ApplicationDbContext, Task<TResult>> query)
    {
        await using var context = await CreateDbContextAsync();
        return await query(context);
    }

    public async ValueTask DisposeAsync()
    {
        await _provider.DisposeAsync();
        await _keeperConnection.DisposeAsync();

        if (Directory.Exists(ContentRootPath))
        {
            Directory.Delete(ContentRootPath, recursive: true);
        }
    }
}
