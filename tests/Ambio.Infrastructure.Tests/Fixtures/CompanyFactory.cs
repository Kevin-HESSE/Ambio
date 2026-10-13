using Ambio.Application.Common;
using Ambio.Domain.Companies;

using Bogus;

namespace Ambio.Infrastructure.Tests.Fixtures;

/// <summary>Companies to save straight through the DbContext, without the service.</summary>
public static class CompanyFactory
{
    private const string IsArchived = "archived";
    public const string IsArchivedRuleSet = $"default,{IsArchived}";

    public const string SlugFallback = "company";

    /// <summary>Companies with every field. The slug is the one of the name, without suffix: it is set last, so it
    /// follows a name given with <c>RuleFor</c>.</summary>
    public static Faker<Company> GetGenerator() =>
        new Faker<Company>()
            .RuleFor(c => c.Name, faker => faker.Company.CompanyName())
            .RuleFor(c => c.Kind, faker => faker.PickRandom<CompanyKind>())
            .RuleFor(c => c.Industry, faker => faker.Commerce.Department())
            .RuleFor(c => c.Size, faker => faker.PickRandom<CompanySize>())
            .RuleFor(c => c.Location, faker => faker.Address.City())
            .RuleFor(c => c.Website, faker => $"https://{faker.Internet.DomainName()}")
            .RuleFor(c => c.LinkedInUrl, faker => $"https://www.linkedin.com/company/{faker.Internet.DomainWord()}")
            .RuleFor(c => c.Description, faker => faker.Company.CatchPhrase())
            .FinishWith((_, c) => c.Slug = Slug.From(c.Name, SlugFallback))
            .RuleSet(IsArchived, set =>
                set.RuleFor(c => c.ArchivedAt, faker => faker.Date.Past())
            );

    /// <summary>Companies named <paramref name="name"/>, for the tests that rely on its name or its slug. The rule
    /// goes on a copy: the generator it is called on keeps random names.</summary>
    public static Faker<Company> WithCustomName(this Faker<Company> generator, string name) =>
        generator.Clone().RuleFor(c => c.Name, name);
}
