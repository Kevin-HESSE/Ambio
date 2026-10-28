namespace Ambio.Domain.Offers;

public enum ContractType
{
    // Permanent contract (CDI)
    Permanent,
    // Fixed-term contract (CDD)
    FixedTerm,
    // Freelance or contractor mission
    Freelance,
    // Internship (stage)
    Internship,
    // Work-study contract (alternance)
    Apprenticeship,
    // Any other kind of contract
    Other
}
