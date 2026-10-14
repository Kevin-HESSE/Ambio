using Ambio.Application.Offers.Dtos;

using Bogus;

namespace Ambio.Web.Tests.Fixtures;

/// <summary>Postings of an offer, as returned by the mocked <c>IJobOfferService</c>.</summary>
public static class JobOfferPostingFactory
{
    public static Faker<JobOfferPostingDto> GetGenerator() =>
        new Faker<JobOfferPostingDto>()
            .WithRecord()
            .RuleFor(p => p.Id, Guid.CreateVersion7)
            .RuleFor(p => p.Site, f => f.PickRandom("LinkedIn", "Indeed", "Welcome to the Jungle"))
            .RuleFor(p => p.Url, f => f.Internet.UrlWithPath("https"))
            .RuleFor(p => p.SeenAt, f => f.Date.RecentOffset().UtcDateTime);
}
