using Ambio.Domain.Offers;

namespace Ambio.Domain.JobApplications;

/// <summary>An answer to a job offer: its company is the offer's.</summary>
public class OfferApplication : JobApplication
{
    public required Guid JobOfferId { get; init; }

    public JobOffer JobOffer { get; private set; } = null!;

    // The posting whose site was used to apply, when known: always one of the offer's postings.
    public Guid? AppliedViaPostingId { get; private set; }

    public JobOfferPosting? AppliedViaPosting { get; private set; }

    /// <summary>Records the posting whose site was used to apply.</summary>
    /// <exception cref="InvalidOperationException">The posting belongs to another offer.</exception>
    public void ApplyVia(JobOfferPosting posting)
    {
        if (posting.JobOfferId != JobOfferId)
        {
            throw new InvalidOperationException($"The posting {posting.Id} belongs to another offer.");
        }

        AppliedViaPostingId = posting.Id;
    }
}
