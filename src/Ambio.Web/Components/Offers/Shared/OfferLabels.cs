using Ambio.Domain.Offers;

namespace Ambio.Web.Components.Offers.Shared;

/// <summary>Display names of the offer enums. The database stores the enum names, never these labels.</summary>
public static class OfferLabels
{
    public static string ToLabel(this ContractType contractType) => contractType switch
    {
        ContractType.Permanent => "Permanent",
        ContractType.FixedTerm => "Fixed-term",
        ContractType.Freelance => "Freelance",
        ContractType.Internship => "Internship",
        ContractType.Apprenticeship => "Apprenticeship",
        ContractType.Other => "Other",
        _ => throw new ArgumentOutOfRangeException(nameof(contractType), contractType, null),
    };

    public static string ToLabel(this RemotePolicy remotePolicy) => remotePolicy switch
    {
        RemotePolicy.OnSite => "On-site",
        RemotePolicy.Hybrid => "Hybrid",
        RemotePolicy.FullRemote => "Full remote",
        RemotePolicy.Unknown => "Remote not specified",
        _ => throw new ArgumentOutOfRangeException(nameof(remotePolicy), remotePolicy, null),
    };

    /// <summary>"posting" or "postings", the word after a count of postings.</summary>
    public static string PostingWord(int count) => count == 1 ? "posting" : "postings";
}
