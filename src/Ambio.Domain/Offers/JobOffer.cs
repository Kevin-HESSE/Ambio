using Ambio.Domain.Common;
using Ambio.Domain.Companies;

namespace Ambio.Domain.Offers;

/// <summary>A job ad, independent of whether it is applied to. It was published, for Ambio, on the day it was
/// recorded: its <see cref="CreatedAt"/>.</summary>
public class JobOffer : ITimeStampable
{
    private readonly List<JobOfferPosting> _postings = [];

    public Guid Id { get; } = Guid.CreateVersion7();

    // The end client. It becomes optional with the recruiter company (Phase 2b).
    public required Guid CompanyId { get; set; }

    public Company Company { get; private set; } = null!;

    public required string Title
    {
        get;
        set
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value);
            field = value.Trim();
        }
    }

    // Plain-text copy of the ad, kept when the ad goes offline.
    public string? Description { get; set; }

    public string? Location { get; set; }

    public required ContractType ContractType { get; set; }

    public required RemotePolicy RemotePolicy { get; set; }

    // Free text, as announced: "45–55 k€", "TJM 500".
    public string? Salary { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? ArchivedAt { get; set; }

    public IReadOnlyList<JobOfferPosting> Postings => _postings;

    /// <summary>Records a site where the offer was seen. The site and the URL are trimmed; an URL is recorded
    /// only once per offer.</summary>
    /// <exception cref="InvalidOperationException">The offer already has a posting with this URL.</exception>
    public JobOfferPosting AddPosting(string site, string url)
    {
        var posting = new JobOfferPosting { JobOfferId = Id, Site = site, Url = url };

        if (_postings.Any(p => p.Url == posting.Url))
        {
            throw new InvalidOperationException($"The offer already has a posting with the URL {posting.Url}.");
        }

        _postings.Add(posting);
        return posting;
    }
}
