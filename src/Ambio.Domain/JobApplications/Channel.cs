namespace Ambio.Domain.JobApplications;

/// <summary>How an application was sent.</summary>
public enum Channel
{
    // A job board or careers site form
    OnlineForm,
    // An email to the company or the recruiter
    Email,
    // Through someone who referred the candidate
    Referral,
    // Handed over in person
    InPerson,
    // Any other way
    Other
}
