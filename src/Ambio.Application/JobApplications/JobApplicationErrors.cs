namespace Ambio.Application.JobApplications;

/// <summary>Error messages returned by <see cref="IJobApplicationService"/>.</summary>
public static class JobApplicationErrors
{
    public const string NotFound = "This application doesn't exist anymore.";

    public const string OfferNotFound = "This offer doesn't exist anymore.";

    public const string PostingNotFound = "Choose one of the sites where this offer was seen.";
}
