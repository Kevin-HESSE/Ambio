using System.Text;
using System.Text.Encodings.Web;

using Ambio.Application.Common;
using Ambio.Application.Users;
using Ambio.Application.Users.Dtos;
using Ambio.Infrastructure.Common;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Ambio.Infrastructure.Users;

internal class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserStore<ApplicationUser> _userStore;
    private readonly IEmailSender<ApplicationUser> _emailSender;
    private readonly ILogger<UserService> _logger;
    private readonly SemaphoreContainer _semaphoreContainer;

    public UserService(
        IEmailSender<ApplicationUser> emailSender,
        UserManager<ApplicationUser> userManager,
        IUserStore<ApplicationUser> userStore,
        SemaphoreContainer semaphoreContainer,
        ILogger<UserService> logger)
    {
        _userManager = userManager;
        _userStore = userStore;
        _emailSender = emailSender;
        _logger = logger;
        _semaphoreContainer = semaphoreContainer;
    }

    public async Task<bool> HasUserAsync(CancellationToken cancellationToken = default) => await _userManager.Users.AnyAsync(cancellationToken);

    public async Task<ServiceResult<string>> RegisterUserAsync(RegisterInput input, CancellationToken cancellationToken = default)
    {
        await _semaphoreContainer.Semaphore.WaitAsync(cancellationToken);

        try
        {
            if (await HasUserAsync(cancellationToken))
            {
                return ServiceResult<string>.Failure(UserErrors.AlreadyExists);
            }

            var user = CreateUser();
            await _userStore.SetUserNameAsync(user, input.Email, cancellationToken);
            var emailStore = GetEmailStore();
            await emailStore.SetEmailAsync(user, input.Email, cancellationToken);
            var result = await _userManager.CreateAsync(user, input.Password);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return ServiceResult<string>.Failure(errors);
            }

            return ServiceResult<string>.Success(user.Id);
        }
        finally
        {
            _semaphoreContainer.Semaphore.Release();
        }
    }

    public async Task<ServiceResult> GenerateConfirmEmailAsync(string userId, string callbackUrl)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return ServiceResult.Failure(UserErrors.NotFound);
        }

        try
        {
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var queryParams = new Dictionary<string, string?>
            {
                ["userId"] = userId,
                ["code"] = code,
            };

            var confirmationLink = QueryHelpers.AddQueryString(callbackUrl, queryParams);

            await _emailSender.SendConfirmationLinkAsync(user, user.Email!, HtmlEncoder.Default.Encode(confirmationLink));
            return ServiceResult.Success();
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to send email confirmation link.");
            return ServiceResult.Failure(UserErrors.EmailConfirmationFailed);
        }
    }

    private static ApplicationUser CreateUser()
    {
        try
        {
            return Activator.CreateInstance<ApplicationUser>();
        }
        catch
        {
            throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
                                                $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor.");
        }
    }

    private IUserEmailStore<ApplicationUser> GetEmailStore()
    {
        if (!_userManager.SupportsUserEmail)
        {
            throw new NotSupportedException("The default UI requires a user store with email support.");
        }
        return (IUserEmailStore<ApplicationUser>)_userStore;
    }
}
