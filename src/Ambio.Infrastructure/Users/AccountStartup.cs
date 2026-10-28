using System.ComponentModel.DataAnnotations;

using Ambio.Application.Users;
using Ambio.Application.Users.Dtos;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Ambio.Infrastructure.Users;

/// <summary>Creates the single account from <see cref="AccountOptions"/> at first start, through
/// <see cref="IUserService.RegisterUserAsync"/> and its guard (ADR 0028).</summary>
internal sealed class AccountStartup
{
    private const string MissingAccount =
        $"No account exists yet: set {AccountOptions.Section}:{nameof(AccountOptions.Email)} and " +
        $"{AccountOptions.Section}:{nameof(AccountOptions.Password)} " +
        "(Ambio__Account__Email, Ambio__Account__Password) to create it.";

    private readonly IUserService _userService;
    private readonly AccountOptions _options;
    private readonly ILogger<AccountStartup> _logger;

    public AccountStartup(IUserService userService, IOptions<AccountOptions> options, ILogger<AccountStartup> logger)
    {
        _userService = userService;
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>Does nothing once an account exists. Otherwise creates it, or throws when the settings are missing or
    /// refused, so the app doesn't start without its account.</summary>
    public async Task CreateAccountIfNoneAsync(CancellationToken cancellationToken)
    {
        if (await _userService.HasUserAsync(cancellationToken))
        {
            return;
        }

        var input = new RegisterInput { Email = _options.Email, Password = _options.Password };
        var errors = new List<ValidationResult>();
        if (!Validator.TryValidateObject(input, new ValidationContext(input), errors, validateAllProperties: true))
        {
            throw new InvalidOperationException($"{MissingAccount} {string.Join(" ", errors.Select(e => e.ErrorMessage))}");
        }

        var result = await _userService.RegisterUserAsync(input, cancellationToken);
        if (!result.IsSuccess)
        {
            throw new InvalidOperationException($"{MissingAccount} {result.GetMessage}");
        }

        _logger.LogInformation("The account was created from the {Section} settings, which are now ignored.", AccountOptions.Section);
    }
}
