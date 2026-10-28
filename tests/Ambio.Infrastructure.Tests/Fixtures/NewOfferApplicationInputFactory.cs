using Ambio.Application.JobApplications.Dtos;
using Ambio.Domain.JobApplications;

using Bogus;

namespace Ambio.Infrastructure.Tests.Fixtures;

/// <summary>New application forms for a draft, as sent to <c>IJobApplicationService</c>: set the offer with
/// <c>with</c>.</summary>
public static class NewOfferApplicationInputFactory
{
    public static Faker<NewOfferApplicationInput> GetGenerator() =>
        new Faker<NewOfferApplicationInput>()
            .RuleFor(a => a.Channel, faker => faker.PickRandom<Channel>())
            .RuleFor(a => a.ChannelDetail, faker => faker.Internet.Email())
            .RuleFor(a => a.Notes, faker => faker.Lorem.Sentence());
}
