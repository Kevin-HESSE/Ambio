using Ambio.Application.Offers.Dtos;
using Ambio.Domain.Offers;

using Bogus;

namespace Ambio.Infrastructure.Tests.Fixtures;

/// <summary>New offer forms with every field, as sent to <c>IJobOfferService</c>.</summary>
public static class JobOfferInputFactory
{
    public static Faker<JobOfferInput> GetGenerator() =>
        new Faker<JobOfferInput>()
            .RuleFor(o => o.CompanyName, faker => faker.Company.CompanyName())
            .RuleFor(o => o.Title, faker => faker.Name.JobTitle())
            .RuleFor(o => o.ContractType, faker => faker.PickRandom<ContractType>())
            .RuleFor(o => o.RemotePolicy, faker => faker.PickRandom<RemotePolicy>())
            .RuleFor(o => o.Location, faker => faker.Address.City())
            .RuleFor(o => o.Salary, faker => $"{faker.Random.Int(30, 80)} k€")
            .RuleFor(o => o.Site, faker => faker.PickRandom("LinkedIn", "Indeed", "Welcome to the Jungle"))
            .RuleFor(o => o.Url, faker => faker.Internet.UrlWithPath("https"))
            .RuleFor(o => o.Description, faker => faker.Lorem.Paragraph());

    /// <summary>Forms with the company name <paramref name="name"/>, on a copy of the generator.</summary>
    public static Faker<JobOfferInput> WithCustomCompanyName(this Faker<JobOfferInput> generator, string name) =>
        generator.Clone().RuleFor(o => o.CompanyName, name);
}
