using Ambio.Application.Common;
using Ambio.Application.Offers.Dtos;
using Ambio.Domain.Offers;

using Bogus;

namespace Ambio.Web.Tests.Fixtures;

/// <summary>Rows of the offers lists, as returned by the mocked <c>IJobOfferService</c>.</summary>
public static class JobOfferSummaryFactory
{
    public static Faker<JobOfferSummaryDto> GetGenerator() =>
        new Faker<JobOfferSummaryDto>()
            .WithRecord()
            .RuleFor(j => j.Id, Guid.CreateVersion7)
            .RuleFor(j => j.Title, faker => faker.Name.JobTitle())
            .RuleFor(j => j.CompanyName, faker => faker.Company.CompanyName())
            .RuleFor(j => j.CompanySlug, (faker, j) => Slug.From(j.CompanyName, "company"))
            .RuleFor(j => j.ContractType, faker => faker.PickRandom<ContractType>())
            .RuleFor(j => j.RemotePolicy, faker => faker.PickRandom<RemotePolicy>())
            .RuleFor(j => j.PostingCount, faker => faker.Random.Int(1, 5))
            .RuleFor(j => j.RegisterAt, faker => faker.Date.RecentOffset().UtcDateTime);
}
