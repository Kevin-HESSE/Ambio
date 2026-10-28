using Ambio.Application.Common;
using Ambio.Application.Users;
using Ambio.Application.Users.Dtos;
using Ambio.Infrastructure.Tests.Fixtures;

using Microsoft.EntityFrameworkCore;

namespace Ambio.Infrastructure.Tests.Users;

public class UserServiceTests : IAsyncLifetime
{
    private const string UserEmail = "user@example.com";
    private const string OtherUserEmail = "other@example.com";
    private const string UserPassword = "Passw0rd!";

    private SqliteServiceProvider _services = null!;

    public async ValueTask InitializeAsync() => _services = await SqliteServiceProvider.CreateAsync();

    public ValueTask DisposeAsync() => _services.DisposeAsync();

    [Fact]
    public async Task HasUserAsync_WithoutUser_ReturnsFalse()
    {
        Assert.False(await HasUserAsync());
    }

    [Fact]
    public async Task RegisterUserAsync_FirstUser_CreatesConfirmedUser()
    {
        var result = await RegisterAsync(ValidInput());

        Assert.True(result.TryGetValue(out var userId));
        Assert.True(await HasUserAsync());
        var user = await _services.QueryAsync(context => context.Users.SingleAsync(TestContext.Current.CancellationToken));
        Assert.Equal(userId, user.Id);
        Assert.Equal(UserEmail, user.Email);
        Assert.True(user.EmailConfirmed);
    }

    [Fact]
    public async Task RegisterUserAsync_WhenUserExists_Fails()
    {
        await RegisterAsync(ValidInput());

        var result = await RegisterAsync(ValidInput() with { Email = OtherUserEmail });

        Assert.False(result.IsSuccess);
        Assert.Equal(UserErrors.AlreadyExists, result.GetMessage);
        Assert.Equal(1, await CountUsersAsync());
    }

    [Fact]
    public async Task RegisterUserAsync_ConcurrentCalls_CreateOnlyOneUser()
    {
        var results = await Task.WhenAll(
            RegisterAsync(ValidInput()),
            RegisterAsync(ValidInput() with { Email = OtherUserEmail }));

        Assert.Single(results, r => r.IsSuccess);
        Assert.Equal(1, await CountUsersAsync());
    }

    [Theory]
    [MemberData(nameof(PasswordsRejectedByIdentity))]
    public async Task RegisterUserAsync_PasswordRejectedByIdentity_FailsWithoutUser(string password)
    {
        var result = await RegisterAsync(ValidInput() with { Password = password });

        Assert.False(result.IsSuccess);
        Assert.Equal(0, await CountUsersAsync());
    }

    private static RegisterInput ValidInput() => new()
    {
        Email = UserEmail,
        Password = UserPassword,
    };

    private Task<ServiceResult<string>> RegisterAsync(RegisterInput input) =>
        _services.CallAsync((IUserService service) => service.RegisterUserAsync(input, TestContext.Current.CancellationToken));

    private Task<bool> HasUserAsync() =>
        _services.CallAsync((IUserService service) => service.HasUserAsync(TestContext.Current.CancellationToken));

    private Task<int> CountUsersAsync() =>
        _services.QueryAsync(context => context.Users.CountAsync(TestContext.Current.CancellationToken));

    public static TheoryData<string> PasswordsRejectedByIdentity =>
    [
        "passw0rd!",
        "PASSW0RD!",
        "Password!",
        "Passw0rd",
    ];
}
