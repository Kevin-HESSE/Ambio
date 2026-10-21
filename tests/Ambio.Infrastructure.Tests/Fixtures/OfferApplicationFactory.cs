using Ambio.Domain.JobApplications;

using Bogus;

namespace Ambio.Infrastructure.Tests.Fixtures;

/// <summary>Applications to an offer, to save straight through the DbContext, without the service.</summary>
public static class OfferApplicationFactory
{
    private const string IsSent = "sent";
    private const string IsArchived = "archived";
    public const string IsSentRuleSet = $"default,{IsSent}";
    public const string IsArchivedRuleSet = $"default,{IsArchived}";

    /// <summary>Drafts to the offer, with every optional field and a random 6-digit short id. Build them through
    /// <c>OfferApplicationStory</c>, which saves their offer first.</summary>
    public static Faker<OfferApplication> GetGenerator(Guid jobOfferId) =>
        new Faker<OfferApplication>()
            .RuleFor(a => a.JobOfferId, jobOfferId)
            .RuleFor(a => a.ShortId, faker => faker.Random.String2(6, "0123456789"))
            .RuleFor(a => a.Channel, faker => faker.PickRandom<Channel>())
            .RuleFor(a => a.ChannelDetail, faker => faker.Internet.Email())
            .RuleFor(a => a.Notes, faker => faker.Lorem.Sentence())
            .RuleSet(IsSent, set =>
                set.FinishWith((faker, a) =>
                    a.ChangeStatus(ApplicationStatus.Applied, faker.Date.Past().ToUniversalTime(), appliedOn: DateOnly.FromDateTime(faker.Date.Past())))
            )
            .RuleSet(IsArchived, set =>
                set.RuleFor(a => a.ArchivedAt, faker => faker.Date.Past())
            );
}
