namespace Ambio.Application.Common;

/// <summary>Error messages returned by <see cref="Interfaces.IUserService"/>.</summary>
public static class UserErrors
{
    public const string AlreadyExists = "User already exists";
    public const string NotFound = "User not found";
    public const string EmailConfirmationFailed = "Sending Email confirmation failed";
}
