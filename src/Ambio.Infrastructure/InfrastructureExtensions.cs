using Ambio.Application.Common;
using Ambio.Application.Companies;
using Ambio.Application.CoverLetters;
using Ambio.Application.CvDocuments;
using Ambio.Application.Dashboard;
using Ambio.Application.JobApplications;
using Ambio.Application.Offers;
using Ambio.Application.Settings;
using Ambio.Application.Users;
using Ambio.Infrastructure.Common;
using Ambio.Infrastructure.Companies;
using Ambio.Infrastructure.CoverLetters;
using Ambio.Infrastructure.CvDocuments;
using Ambio.Infrastructure.Dashboard;
using Ambio.Infrastructure.JobApplications;
using Ambio.Infrastructure.Offers;
using Ambio.Infrastructure.Persistence;
using Ambio.Infrastructure.Settings;
using Ambio.Infrastructure.Users;

using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Ambio.Infrastructure;

public static class InfrastructureExtensions
{
    private const string DefaultConnection = "DefaultConnection";

    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfigurationManager configurationManager)
    {
        var connectionString = configurationManager.GetConnectionString(DefaultConnection)
                               ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.TryAddSingleton(TimeProvider.System);
        services.AddSingleton<TimestampInterceptor>();

        services.AddDbContextFactory<ApplicationDbContext>((provider, options) =>
        {
            options.UseSqlite(connectionString);
            options.AddInterceptors(provider.GetRequiredService<TimestampInterceptor>());
        });

        services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.SignIn.RequireConfirmedAccount = true;
                options.Stores.SchemaVersion = IdentitySchemaVersions.Version3;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

        return services;
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddSingleton<SemaphoreContainer>();
        services.AddOptions<ShortIdOptions>()
            .BindConfiguration(ShortIdOptions.Section)
            .Validate(options => options.IsValid(), $"{ShortIdOptions.Section}: the alphabet must be lowercase, without '-', and the size positive.")
            .ValidateOnStart();
        services.AddSingleton<ShortIdGenerator>();
        services.AddOptions<StorageOptions>()
            .BindConfiguration(StorageOptions.Section)
            .Validate(options => options.IsValid(), $"{StorageOptions.Section}: the root must be set, and the upload size limit positive.")
            .ValidateOnStart();
        services.AddSingleton<IDocumentStorage, FileSystemDocumentStorage>();
        // The keys of the login cookies and 2FA tokens live with the data, so they survive a new container.
        services.AddDataProtection().SetApplicationName("Ambio");
        services.AddOptions<KeyManagementOptions>()
            .Configure<IOptions<StorageOptions>, IHostEnvironment, ILoggerFactory>((options, storage, environment, loggerFactory) =>
                options.XmlRepository = new FileSystemXmlRepository(
                    new DirectoryInfo(Path.Combine(storage.Value.GetFullRoot(environment.ContentRootPath), StorageOptions.KeysFolder)),
                    loggerFactory));
        services.AddOptions<AccountOptions>().BindConfiguration(AccountOptions.Section);
        services.AddScoped<AccountStartup>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ICompanyService, CompanyService>();
        services.AddScoped<IJobOfferService, JobOfferService>();
        services.AddScoped<IJobApplicationService, JobApplicationService>();
        services.AddScoped<ICoverLetterService, CoverLetterService>();
        services.AddScoped<ICvDocumentService, CvDocumentService>();
        services.AddScoped<IUserSettingsService, UserSettingsService>();
        services.AddScoped<IDashboardService, DashboardService>();

        return services;
    }

    /// <summary>Applies the migrations the database hasn't got yet, creating it when it doesn't exist. Called before
    /// the app accepts requests, and before the account is created.</summary>
    public static async Task MigrateDatabaseAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var db = await services.GetRequiredService<IDbContextFactory<ApplicationDbContext>>()
            .CreateDbContextAsync(cancellationToken);
        await db.Database.MigrateAsync(cancellationToken);
    }

    /// <summary>Creates the single account from the <c>Ambio:Account</c> settings while none exists, and throws when
    /// they are missing or refused (ADR 0028). Called before the app accepts requests.</summary>
    public static async Task CreateAccountAtFirstStartAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<AccountStartup>().CreateAccountIfNoneAsync(cancellationToken);
    }
}
