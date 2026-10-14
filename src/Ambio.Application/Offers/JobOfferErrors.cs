namespace Ambio.Application.Offers;

/// <summary>Error messages returned by <see cref="IJobOfferService"/>.</summary>
public static class JobOfferErrors
{
    public const string NotFound = "This offer doesn't exist.";

    public const string PostingAlreadyExists = "This offer already has a posting with this link.";

    public const string PostingNotFound = "This posting doesn't exist.";

    public const string LastPosting = "An offer keeps at least one posting.";
}
