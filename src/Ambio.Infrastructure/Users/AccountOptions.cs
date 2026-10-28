namespace Ambio.Infrastructure.Users;

/// <summary>The single account, created at first start from the <see cref="Section"/> section while no account exists
/// (ADR 0028). Once the account exists, these settings are ignored.</summary>
public sealed class AccountOptions
{
    public const string Section = "Ambio:Account";

    public string Email { get; set; } = "";

    public string Password { get; set; } = "";
}
