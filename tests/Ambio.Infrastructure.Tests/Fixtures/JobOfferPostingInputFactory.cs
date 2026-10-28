using Ambio.Application.Offers.Dtos;

using Bogus;

namespace Ambio.Infrastructure.Tests.Fixtures;

/// <summary>Add posting forms, as sent to <c>IJobOfferService</c>.</summary>
public static class JobOfferPostingInputFactory
{
    public static Faker<JobOfferPostingInput> GetGenerator() =>
        new Faker<JobOfferPostingInput>()
            .RuleFor(p => p.Site, faker => faker.PickRandom("LinkedIn", "Indeed", "Welcome to the Jungle"))
            .RuleFor(p => p.Url, faker => faker.Internet.UrlWithPath("https"));
}
