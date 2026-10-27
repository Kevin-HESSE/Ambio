using Ambio.Application.Dashboard.Dtos;
using Ambio.Domain.JobApplications;

using Bogus;

namespace Ambio.Web.Tests.Fixtures;

/// <summary>Events of the dashboard's recent activity, as returned by the mocked <c>IDashboardService</c>: a change to
/// any status but Draft, which marks a creation, of a sent application.</summary>
public static class ActivityFactory
{
    public static Faker<ActivityDto> GetGenerator() =>
        new Faker<ActivityDto>()
            .WithRecord()
            .RuleFor(a => a.Application, _ => JobApplicationSummaryFactory.GetGenerator().Generate())
            .RuleFor(a => a.Status, faker => faker.PickRandom(Enum.GetValues<ApplicationStatus>().Where(s => s != ApplicationStatus.Draft)))
            .RuleFor(a => a.OccurredAt, faker => faker.Date.Recent().ToUniversalTime());
}
