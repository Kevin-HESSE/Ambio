using Ambio.Application.Companies.Dtos;
using Ambio.Domain.Companies;

using Bogus;

namespace Ambio.Infrastructure.Tests.Fixtures;

/// <summary>Company forms with every field, as sent to <c>ICompanyService</c>.</summary>
public static class CompanyInputFactory
{
    public static Faker<CompanyInput> GetGenerator() =>
        new Faker<CompanyInput>()
            .RuleFor(c => c.Name, faker => faker.Company.CompanyName())
            .RuleFor(c => c.Kind, faker => faker.PickRandom<CompanyKind>())
            .RuleFor(c => c.Industry, faker => faker.Commerce.Department())
            .RuleFor(c => c.Size, faker => faker.PickRandom<CompanySize>())
            .RuleFor(c => c.Location, faker => faker.Address.City())
            .RuleFor(c => c.Website, faker => $"https://{faker.Internet.DomainName()}")
            .RuleFor(c => c.LinkedInUrl, faker => $"https://www.linkedin.com/company/{faker.Internet.DomainWord()}")
            .RuleFor(c => c.Description, faker => faker.Company.CatchPhrase());

    /// <summary>Forms with the name <paramref name="name"/>, on a copy of the generator.</summary>
    public static Faker<CompanyInput> WithCustomName(this Faker<CompanyInput> generator, string name) =>
        generator.Clone().RuleFor(c => c.Name, name);
}
