using Ambio.Application.JobApplications.Dtos;
using Ambio.Domain.JobApplications;

using Bogus;

namespace Ambio.Infrastructure.Tests.Fixtures;

/// <summary>New spontaneous application forms for a draft, as sent to <c>IJobApplicationService</c>: set the company
/// name with <c>with</c>.</summary>
public static class NewSpontaneousApplicationInputFactory
{
    public static Faker<NewSpontaneousApplicationInput> GetGenerator() =>
        new Faker<NewSpontaneousApplicationInput>()
            .RuleFor(a => a.TargetPosition, faker => faker.Name.JobTitle())
            .RuleFor(a => a.Channel, faker => faker.PickRandom<Channel>())
            .RuleFor(a => a.ChannelDetail, faker => faker.Internet.Email())
            .RuleFor(a => a.Notes, faker => faker.Lorem.Sentence());
}
