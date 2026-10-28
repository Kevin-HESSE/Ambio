using Ambio.Domain.Companies;
using Ambio.Domain.Offers;
using Ambio.Infrastructure.Tests.Fixtures;

namespace Ambio.Infrastructure.Tests.Stories;

/// <summary>A company and its offers: an offer can't be saved without its company. Each test builds its own
/// story, so no data is shared between tests.</summary>
public sealed class JobOfferStory
{
    private readonly List<JobOffer> _jobOffers = [];

    public Company Company { get; } = CompanyFactory.GetGenerator().Generate();

    public IReadOnlyList<JobOffer> JobOffers => _jobOffers;

    /// <summary>Adds <paramref name="count"/> offers of the company, each with <paramref name="numberOfPosting"/>
    /// postings.</summary>
    public JobOfferStory WithJobOffers(int count, int numberOfPosting = 0, string ruleSet = "default")
    {
        var jobOfferFactory = JobOfferFactory
            .GetGenerator(Company.Id)
            .FinishWith((faker, offer) =>
            {
                for (var i = 0; i < numberOfPosting; i++)
                {
                    offer.AddPosting(site: faker.Internet.DomainName(), url: faker.Internet.UrlWithPath("https"));
                }
            });

        _jobOffers.AddRange(jobOfferFactory.Generate(count, ruleSet));
        return this;
    }

    /// <summary>Saves the company, then its offers with their postings.</summary>
    public async Task SaveAsync(SqliteServiceProvider services)
    {
        await services.AddAsync(Company);
        await services.AddAsync([.. _jobOffers]);
    }
}
