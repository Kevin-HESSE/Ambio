using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Ambio.E2E.Tests.Fixtures;

/// <summary>Runs Ambio.Web on a real Kestrel server, on a free port, against the given SQLite database.</summary>
public sealed class AmbioAppFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString;

    public AmbioAppFactory(string connectionString)
    {
        _connectionString = connectionString;
        UseKestrel(0);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:DefaultConnection", _connectionString);
    }
}
