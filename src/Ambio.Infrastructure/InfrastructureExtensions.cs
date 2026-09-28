using Ambio.Infrastructure.Persistence;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ambio.Infrastructure;

public static class InfrastructureExtensions
{
    private const string DefaultConnection = "DefaultConnection";

    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfigurationManager configurationManager)
    {
        var connectionString = configurationManager.GetConnectionString(DefaultConnection)
                               ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContextFactory<ApplicationDbContext>(options =>
            options.UseSqlite(connectionString));

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
}
