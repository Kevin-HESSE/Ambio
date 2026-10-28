using Ambio.Application.Common;
using Ambio.Application.Users;
using Ambio.Application.Users.Dtos;
using Ambio.Infrastructure.Common;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Ambio.Infrastructure.Users;

internal class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserStore<ApplicationUser> _userStore;
    private readonly SemaphoreContainer _semaphoreContainer;

    public UserService(
        UserManager<ApplicationUser> userManager,
        IUserStore<ApplicationUser> userStore,
        SemaphoreContainer semaphoreContainer)
    {
        _userManager = userManager;
        _userStore = userStore;
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
            // The email comes from whoever deploys the instance, and Ambio sends no email: it is confirmed at once.
            await emailStore.SetEmailConfirmedAsync(user, true, cancellationToken);
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
