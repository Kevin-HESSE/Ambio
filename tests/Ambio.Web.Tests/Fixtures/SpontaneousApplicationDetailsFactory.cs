using Ambio.Application.Common;
using Ambio.Application.JobApplications.Dtos;
using Ambio.Domain.JobApplications;

using Bogus;

namespace Ambio.Web.Tests.Fixtures;

/// <summary>Sent spontaneous applications, as returned by the mocked <c>IJobApplicationService</c>.</summary>
public static class SpontaneousApplicationDetailsFactory
{
    public static Faker<SpontaneousApplicationDetailsDto> GetGenerator() =>
        new Faker<SpontaneousApplicationDetailsDto>()
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
            .RuleFor(a => a.CompanyLocation, faker => faker.Address.City());
}
