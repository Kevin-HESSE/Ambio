using Ambio.Infrastructure.Common;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Ambio.E2E.Tests.Fixtures;

/// <summary>Runs Ambio.Web on a real Kestrel server, on a free port, against the given SQLite database and storage
/// root.</summary>
public sealed class AmbioAppFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString;
    private readonly string _storageRoot;

    public AmbioAppFactory(string connectionString, string storageRoot)
    {
        _connectionString = connectionString;
        _storageRoot = storageRoot;
        UseKestrel(0);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:DefaultConnection", _connectionString);
        builder.UseSetting($"{StorageOptions.Section}:Root", _storageRoot);
    }
}
