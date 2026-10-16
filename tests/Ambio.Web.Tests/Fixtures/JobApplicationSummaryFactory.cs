using Ambio.Application.JobApplications.Dtos;
using Ambio.Domain.JobApplications;

using Bogus;

namespace Ambio.Web.Tests.Fixtures;

/// <summary>Rows of the applications lists, as returned by the mocked <c>IJobApplicationService</c>: sent applications
/// to an offer by default, drafts with <see cref="IsDraftRuleSet"/>, spontaneous ones with
/// <see cref="IsSpontaneousRuleSet"/>.</summary>
public static class JobApplicationSummaryFactory
{
    private const string IsDraft = "draft";
    private const string IsSpontaneous = "spontaneous";
    public const string IsDraftRuleSet = $"default,{IsDraft}";
    public const string IsSpontaneousRuleSet = $"default,{IsSpontaneous}";

    public static Faker<JobApplicationSummaryDto> GetGenerator() =>
        new Faker<JobApplicationSummaryDto>()
            .WithRecord()
            .RuleFor(a => a.Id, Guid.CreateVersion7)
            .RuleFor(a => a.Kind, ApplicationKind.Offer)
            .RuleFor(a => a.Title, faker => faker.Name.JobTitle())
            .RuleFor(a => a.CompanyName, faker => faker.Company.CompanyName())
            .RuleFor(a => a.Status, faker => faker.PickRandom(Enum.GetValues<ApplicationStatus>().Where(s => s != ApplicationStatus.Draft)))
            .RuleFor(a => a.Channel, faker => faker.PickRandom<Channel>())
            .RuleFor(a => a.AppliedVia, faker => faker.PickRandom("LinkedIn", "Indeed", "Welcome to the Jungle"))
            .RuleFor(a => a.SentOn, faker => DateOnly.FromDateTime(faker.Date.Recent()))
            .RuleSet(IsDraft, set => set
                .RuleFor(a => a.Status, ApplicationStatus.Draft)
                .RuleFor(a => a.SentOn, (DateOnly?)null))
            .RuleSet(IsSpontaneous, set => set
                .RuleFor(a => a.Kind, ApplicationKind.Spontaneous)
                .RuleFor(a => a.AppliedVia, (string?)null));
}
