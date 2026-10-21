using Ambio.Application.Common;
using Ambio.Application.JobApplications.Dtos;
using Ambio.Domain.JobApplications;
using Ambio.Domain.Offers;

using Bogus;

namespace Ambio.Web.Tests.Fixtures;

/// <summary>Applications to an offer, as returned by the mocked <c>IJobApplicationService</c>: sent through one of
/// the offer's postings by default, drafts without posting nor notes with <see cref="IsDraftRuleSet"/>.</summary>
public static class OfferApplicationDetailsFactory
{
    private const string IsDraft = "draft";
    public const string IsDraftRuleSet = $"default,{IsDraft}";

    public static Faker<OfferApplicationDetailsDto> GetGenerator() =>
        new Faker<OfferApplicationDetailsDto>()
            .WithRecord()
            .RuleFor(a => a.Id, Guid.CreateVersion7)
            .RuleFor(a => a.ShortId, faker => faker.Random.String2(6, "0123456789"))
            .RuleFor(a => a.Title, faker => faker.Name.JobTitle())
            .RuleFor(a => a.CompanyName, faker => faker.Company.CompanyName())
            .RuleFor(a => a.CompanySlug, (_, a) => Slug.From(a.CompanyName, "company"))
            .RuleFor(a => a.Status, faker => faker.PickRandom(Enum.GetValues<ApplicationStatus>().Where(s => s != ApplicationStatus.Draft)))
            .RuleFor(a => a.Channel, faker => faker.PickRandom<Channel>())
            .RuleFor(a => a.ChannelDetail, faker => faker.Internet.Email())
            .RuleFor(a => a.SentOn, faker => DateOnly.FromDateTime(faker.Date.Recent()))
            .RuleFor(a => a.Notes, faker => faker.Lorem.Paragraph())
            .RuleFor(a => a.CreatedAt, faker => faker.Date.Past())
            // A single change, to its status.
            .RuleFor(a => a.StatusHistory, (_, a) => [StatusChangeDtoFactory.GetGenerator().RuleFor(c => c.To, a.Status).Generate()])
            .RuleFor(a => a.JobOfferId, Guid.CreateVersion7)
            .RuleFor(a => a.JobOfferShortId, faker => faker.Random.String2(6, "0123456789"))
            .RuleFor(a => a.Location, faker => faker.Address.City())
            .RuleFor(a => a.ContractType, faker => faker.PickRandom<ContractType>())
            .RuleFor(a => a.RemotePolicy, faker => faker.PickRandom<RemotePolicy>())
            .RuleFor(a => a.Salary, faker => $"{faker.Random.Int(30, 80)} k€")
            .RuleFor(a => a.AppliedViaPostingId, _ => Guid.CreateVersion7())
            .RuleFor(a => a.AppliedViaSite, faker => faker.PickRandom("LinkedIn", "Indeed", "Welcome to the Jungle"))
            .RuleFor(a => a.AppliedViaUrl, faker => faker.Internet.Url())
            .RuleSet(IsDraft, set => set
                .RuleFor(a => a.Status, ApplicationStatus.Draft)
                .RuleFor(a => a.StatusHistory, [])
                .RuleFor(a => a.SentOn, (DateOnly?)null)
                .RuleFor(a => a.Notes, (string?)null)
                .RuleFor(a => a.AppliedViaPostingId, (Guid?)null)
                .RuleFor(a => a.AppliedViaSite, (string?)null)
                .RuleFor(a => a.AppliedViaUrl, (string?)null));
}
