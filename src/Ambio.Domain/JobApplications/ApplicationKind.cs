namespace Ambio.Domain.JobApplications;

/// <summary>What an application answers: an offer (<see cref="OfferApplication"/>) or none
/// (<see cref="SpontaneousApplication"/>).</summary>
public enum ApplicationKind
{
    // An answer to a job offer
    Offer,
    // Sent to a company without any offer
    Spontaneous
}
