using Ambio.Infrastructure.Common;
using Ambio.Infrastructure.Tests.Fixtures;

using Microsoft.AspNetCore.DataProtection;

namespace Ambio.Infrastructure.Tests;

public class InfrastructureExtensionsTests : IAsyncLifetime
{
    private const string DefaultRoot = "Data";

    private SqliteServiceProvider _services = null!;

    public async ValueTask InitializeAsync() => _services = await SqliteServiceProvider.CreateAsync();

    public ValueTask DisposeAsync() => _services.DisposeAsync();

    [Fact]
    public async Task AddApplicationServices_DataProtection_StoresTheKeysUnderTheStorageRoot()
    {
        await _services.CallAsync((IDataProtectionProvider provider) =>
            Task.FromResult(provider.CreateProtector(nameof(InfrastructureExtensionsTests)).Protect("a cookie")));

        var keysFolder = Path.Combine(_services.ContentRootPath, DefaultRoot, StorageOptions.KeysFolder);
        Assert.Single(Directory.GetFiles(keysFolder, "key-*.xml"));
    }
}
