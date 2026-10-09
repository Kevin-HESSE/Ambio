using Ambio.Application.Companies;
using Ambio.Application.Users;
using Ambio.Infrastructure.Common;
using Ambio.Infrastructure.Companies;
using Ambio.Infrastructure.Persistence;
using Ambio.Infrastructure.Users;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

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
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ICompanyService, CompanyService>();

        return services;
    }
}
