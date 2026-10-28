using Ambio.Application.Users;
using Ambio.Application.Users.Dtos;
using Ambio.Infrastructure.Tests.Fixtures;
using Ambio.Infrastructure.Users;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Ambio.Infrastructure.Tests.Users;

public class AccountStartupTests
{
    private const string UserEmail = "user@example.com";
    private const string UserPassword = "Passw0rd!";
    private const string EmailKey = $"{AccountOptions.Section}:{nameof(AccountOptions.Email)}";
    private const string PasswordKey = $"{AccountOptions.Section}:{nameof(AccountOptions.Password)}";

    [Fact]
    public async Task CreateAccountAtFirstStartAsync_WithoutAccount_CreatesTheConfirmedAccount()
    {
        await using var services = await SqliteServiceProvider.CreateAsync(Settings(UserEmail, UserPassword));

        await CreateAccountAsync(services);

        var user = await services.QueryAsync(context => context.Users.SingleAsync(TestContext.Current.CancellationToken));
        Assert.Equal(UserEmail, user.Email);
        Assert.True(user.EmailConfirmed);
        Assert.True(await services.CallAsync((UserManager<ApplicationUser> manager) => manager.CheckPasswordAsync(user, UserPassword)));
    }

    [Theory]
    [InlineData("other@example.com", "Other0rd!")]
    [InlineData(null, null)]
    public async Task CreateAccountAtFirstStartAsync_WithAccount_KeepsItAndIgnoresTheSettings(string? email, string? password)
    {
        await using var services = await SqliteServiceProvider.CreateAsync(Settings(email, password));
        await services.CallAsync((IUserService service) =>
            service.RegisterUserAsync(new RegisterInput { Email = UserEmail, Password = UserPassword }, TestContext.Current.CancellationToken));

        await CreateAccountAsync(services);

        var user = await services.QueryAsync(context => context.Users.SingleAsync(TestContext.Current.CancellationToken));
        Assert.Equal(UserEmail, user.Email);
    }

    [Fact]
    public async Task CreateAccountAtFirstStartAsync_WithoutSettings_ThrowsWithoutAccount()
    {
        await using var services = await SqliteServiceProvider.CreateAsync();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => CreateAccountAsync(services));

        Assert.Contains(AccountOptions.Section, exception.Message, StringComparison.Ordinal);
        Assert.Equal(0, await CountUsersAsync(services));
    }

    [Theory]
    [MemberData(nameof(RefusedSettings))]
    public async Task CreateAccountAtFirstStartAsync_SettingsRefused_ThrowsWithoutAccount(string? email, string? password)
    {
        await using var services = await SqliteServiceProvider.CreateAsync(Settings(email, password));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => CreateAccountAsync(services));

        Assert.Contains(AccountOptions.Section, exception.Message, StringComparison.Ordinal);
        Assert.Equal(0, await CountUsersAsync(services));
    }

    private static Dictionary<string, string?> Settings(string? email, string? password) => new()
    {
        [EmailKey] = email,
        [PasswordKey] = password,
    };

    private static Task CreateAccountAsync(SqliteServiceProvider services) =>
        services.CallAsync((IServiceProvider provider) => provider.CreateAccountAtFirstStartAsync(TestContext.Current.CancellationToken));

    private static Task<int> CountUsersAsync(SqliteServiceProvider services) =>
        services.QueryAsync(context => context.Users.CountAsync(TestContext.Current.CancellationToken));

    public static TheoryData<string?, string?> RefusedSettings => new()
    {
        { "not-an-email", UserPassword },
        { UserEmail, null },
        { UserEmail, "Ab1!" },
        { UserEmail, "passw0rd!" },
    };
}
