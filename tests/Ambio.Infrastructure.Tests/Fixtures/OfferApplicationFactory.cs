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

    // A sent application is sent within the year before this day, which comes before the frozen clock of the service
    // tests: a change they make always follows its sending, whatever the day the tests run.
    private static readonly DateTime SentBefore = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

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
                {
                    var sentAt = faker.Date.Past(refDate: SentBefore);
                    a.ChangeStatus(ApplicationStatus.Applied, sentAt, appliedOn: DateOnly.FromDateTime(sentAt));
                })
            )
            .RuleSet(IsArchived, set =>
                set.FinishWith((faker, a) => a.Archive(faker.Date.Past()))
            );
}
