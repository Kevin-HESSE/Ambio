using Ambio.Application.JobApplications.Dtos;
using Ambio.Domain.JobApplications;

using Bogus;

namespace Ambio.Infrastructure.Tests.Fixtures;

/// <summary>Edit forms of an application to an offer, without posting, as sent to <c>IJobApplicationService</c>.</summary>
public static class OfferApplicationInputFactory
{
    public static Faker<OfferApplicationInput> GetGenerator() =>
        new Faker<OfferApplicationInput>()
            .RuleFor(a => a.Channel, faker => faker.PickRandom<Channel>())
            .RuleFor(a => a.ChannelDetail, faker => faker.Internet.Email())
            .RuleFor(a => a.Notes, faker => faker.Lorem.Sentence());
}
