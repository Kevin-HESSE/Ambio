using Ambio.Application.JobApplications.Dtos;
using Ambio.Domain.JobApplications;

using Bogus;

namespace Ambio.Infrastructure.Tests.Fixtures;

/// <summary>Edit forms of a spontaneous application, as sent to <c>IJobApplicationService</c>.</summary>
public static class SpontaneousApplicationInputFactory
{
    public static Faker<SpontaneousApplicationInput> GetGenerator() =>
        new Faker<SpontaneousApplicationInput>()
            .RuleFor(a => a.TargetPosition, faker => faker.Name.JobTitle())
            .RuleFor(a => a.Channel, faker => faker.PickRandom<Channel>())
            .RuleFor(a => a.ChannelDetail, faker => faker.Internet.Email())
            .RuleFor(a => a.Notes, faker => faker.Lorem.Sentence());
}
