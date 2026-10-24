namespace Ambio.Application.CoverLetters;

/// <summary>Error messages returned by <see cref="ICoverLetterService"/>.</summary>
public static class CoverLetterErrors
{
    public const string NotFound = "This letter doesn't exist anymore.";

    public const string ApplicationNotFound = "This application doesn't exist anymore.";
}
