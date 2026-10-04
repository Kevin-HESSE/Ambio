using Ambio.Application.Common;
using Ambio.Application.Users;
using Ambio.Application.Users.Dtos;
using Ambio.Infrastructure.Tests.Fixtures;
using Ambio.Infrastructure.Users;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Ambio.Infrastructure.Tests.Service;

public class UserServiceTests : IAsyncLifetime
{
    private const string UserEmail = "user@example.com";
    private const string OtherUserEmail = "other@example.com";
    private const string UserPassword = "Passw0rd!";
    private const string ConfirmEmailUrl = "https://localhost/Account/ConfirmEmail";

    private SqliteServiceProvider _services = null!;

    public async ValueTask InitializeAsync() => _services = await SqliteServiceProvider.CreateAsync();

    public ValueTask DisposeAsync() => _services.DisposeAsync();

    [Fact]
    public async Task HasUserAsync_WithoutUser_ReturnsFalse()
    {
        Assert.False(await HasUserAsync());
    }

    [Fact]
    public async Task RegisterUserAsync_FirstUser_CreatesUser()
    {
        var result = await RegisterAsync(ValidInput());

        Assert.True(result.TryGetValue(out var userId));
        Assert.True(await HasUserAsync());

        await using var context = await _services.CreateDbContextAsync();
        var user = await context.Users.SingleAsync(TestContext.Current.CancellationToken);
        Assert.Equal(userId, user.Id);
        Assert.Equal(UserEmail, user.Email);
        Assert.False(user.EmailConfirmed);
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
        var result = await RegisterAsync(ValidInput() with { Password = password, ConfirmPassword = password });

        Assert.False(result.IsSuccess);
        Assert.Equal(0, await CountUsersAsync());
    }

    [Fact]
    public async Task GenerateConfirmEmailAsync_UnknownUser_Fails()
    {
        var result = await GenerateConfirmEmailAsync(Guid.NewGuid().ToString());

        Assert.False(result.IsSuccess);
        Assert.Equal(UserErrors.NotFound, result.GetMessage);
        await _services.EmailSender.DidNotReceiveWithAnyArgs().SendConfirmationLinkAsync(default!, default!, default!);
    }

    [Fact]
    public async Task GenerateConfirmEmailAsync_KnownUser_SendsConfirmationLink()
    {
        (await RegisterAsync(ValidInput())).TryGetValue(out var userId);

        var result = await GenerateConfirmEmailAsync(userId!);

        Assert.True(result.IsSuccess);
        await _services.EmailSender.Received(1).SendConfirmationLinkAsync(
            Arg.Is<ApplicationUser>(user => user.Id == userId),
            UserEmail,
            Arg.Is<string>(link =>
                link.StartsWith($"{ConfirmEmailUrl}?")
                && link.Contains($"userId={userId}")
                && link.Contains("code=")));
    }

    [Fact]
    public async Task GenerateConfirmEmailAsync_SenderThrows_Fails()
    {
        (await RegisterAsync(ValidInput())).TryGetValue(out var userId);
        _services.EmailSender
            .SendConfirmationLinkAsync(default!, default!, default!)
            .ThrowsAsyncForAnyArgs(new InvalidOperationException());

        var result = await GenerateConfirmEmailAsync(userId!);

        Assert.False(result.IsSuccess);
        Assert.Equal(UserErrors.EmailConfirmationFailed, result.GetMessage);
    }

    private static RegisterInput ValidInput() => new()
    {
        Email = UserEmail,
        Password = UserPassword,
        ConfirmPassword = UserPassword,
    };

    private async Task<ServiceResult<string>> RegisterAsync(RegisterInput input)
    {
        await using var scope = _services.CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
        return await userService.RegisterUserAsync(input, TestContext.Current.CancellationToken);
    }

    private async Task<ServiceResult> GenerateConfirmEmailAsync(string userId)
    {
        await using var scope = _services.CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
        return await userService.GenerateConfirmEmailAsync(userId, ConfirmEmailUrl);
    }

    private async Task<bool> HasUserAsync()
    {
        await using var scope = _services.CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
        return await userService.HasUserAsync(TestContext.Current.CancellationToken);
    }

    private async Task<int> CountUsersAsync()
    {
        await using var context = await _services.CreateDbContextAsync();
        return await context.Users.CountAsync(TestContext.Current.CancellationToken);
    }

    public static TheoryData<string> PasswordsRejectedByIdentity =>
    [
        "passw0rd!",
        "PASSW0RD!",
        "Password!",
        "Passw0rd",
    ];
}
