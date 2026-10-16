using Ambio.Domain.Companies;
using Ambio.Domain.JobApplications;
using Ambio.Domain.Offers;
using Ambio.Infrastructure.Tests.Fixtures;

namespace Ambio.Infrastructure.Tests.Stories;

/// <summary>A company, one of its offers and the applications to it: an application can't be saved without its
/// offer. Each test builds its own story, so no data is shared between tests.</summary>
public sealed class OfferApplicationStory
{
    private readonly JobOfferStory _offerStory;
    private readonly List<OfferApplication> _applications = [];

    /// <summary>A new company and its offer, seen on <paramref name="numberOfPosting"/> sites.</summary>
    public OfferApplicationStory(int numberOfPosting = 1)
    {
        _offerStory = new JobOfferStory().WithJobOffers(1, numberOfPosting);
    }

    public Company Company => _offerStory.Company;

    public JobOffer JobOffer => _offerStory.JobOffers[0];

    public IReadOnlyList<OfferApplication> Applications => _applications;

    /// <summary>Adds <paramref name="count"/> applications to the offer.</summary>
    public OfferApplicationStory WithApplications(int count, string ruleSet = "default")
    {
        _applications.AddRange(OfferApplicationFactory.GetGenerator(JobOffer.Id).Generate(count, ruleSet));
        return this;
    }

    /// <summary>Saves the company and its offer, then the applications.</summary>
    public async Task SaveAsync(SqliteServiceProvider services)
    {
        await _offerStory.SaveAsync(services);
        await services.AddAsync([.. _applications]);
    }
}
