using Ambio.Domain.Offers;

using Bogus;

namespace Ambio.Infrastructure.Tests.Fixtures;

/// <summary>Job offers to save straight through the DbContext, without the service.</summary>
public static class JobOfferFactory
{
    private const string IsArchived = "archived";
    public const string IsArchivedRuleSet = $"default,{IsArchived}";

    /// <summary>Offers of the company with every field, a random 6-digit short id, and no posting. Build them through <c>JobOfferStory</c>, which
    /// saves their company first.</summary>
    public static Faker<JobOffer> GetGenerator(Guid companyId) =>
        new Faker<JobOffer>()
            .RuleFor(o => o.CompanyId, companyId)
            .RuleFor(o => o.ShortId, faker => faker.Random.String2(6, "0123456789"))
            .RuleFor(o => o.Title, faker => faker.Name.JobTitle())
            .RuleFor(o => o.Description, faker => faker.Lorem.Paragraph())
            .RuleFor(o => o.Location, faker => faker.Address.City())
            .RuleFor(o => o.ContractType, faker => faker.PickRandom<ContractType>())
            .RuleFor(o => o.RemotePolicy, faker => faker.PickRandom<RemotePolicy>())
            .RuleFor(o => o.Salary, faker => $"{faker.Random.Int(30, 80)} k€")
            .RuleSet(IsArchived, set =>
                set.FinishWith((faker, o) => o.Archive(faker.Date.Past()))
            );
}
