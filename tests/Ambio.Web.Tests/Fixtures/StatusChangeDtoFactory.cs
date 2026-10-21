using Ambio.Application.JobApplications.Dtos;
using Ambio.Domain.JobApplications;

using Bogus;

namespace Ambio.Web.Tests.Fixtures;

/// <summary>Changes of the status history of an application, as returned by the mocked
/// <c>IJobApplicationService</c>: a draft sent, with a comment, by default.</summary>
public static class StatusChangeDtoFactory
{
    public static Faker<StatusChangeDto> GetGenerator() =>
        new Faker<StatusChangeDto>()
            .WithRecord()
            .RuleFor(c => c.From, ApplicationStatus.Draft)
            .RuleFor(c => c.To, ApplicationStatus.Applied)
            .RuleFor(c => c.ChangedAt, f => f.Date.RecentOffset().UtcDateTime)
            .RuleFor(c => c.Comment, f => f.Lorem.Sentence());
}
