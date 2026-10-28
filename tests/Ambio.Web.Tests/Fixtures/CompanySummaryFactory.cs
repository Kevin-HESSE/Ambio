using Ambio.Application.Common;
using Ambio.Application.Companies.Dtos;
using Ambio.Domain.Companies;

using Bogus;

namespace Ambio.Web.Tests.Fixtures;

/// <summary>Rows of the companies list, as returned by the mocked <c>ICompanyService</c>.</summary>
public static class CompanySummaryFactory
{
    private const string WithoutIndustry = "WithoutIndustry";

    /// <summary>Pass to <c>Generate</c>: the default rules, then a company without industry.</summary>
    public const string WithoutIndustryRuleSet = $"default,{WithoutIndustry}";

    public static Faker<CompanySummaryDto> GetGenerator()
    {
        return new Faker<CompanySummaryDto>()
            .WithRecord()
            .RuleFor(c => c.Id, Guid.CreateVersion7)
            .RuleFor(c => c.Name, f => f.Company.CompanyName())
            .RuleFor(c => c.Slug, (f, c) => Slug.From(c.Name, "company"))
            .RuleFor(c => c.Kind, f => f.PickRandom<CompanyKind>())
            .RuleFor(c => c.Industry, f => f.Commerce.Department())
            .RuleFor(c => c.Location, f => f.Address.City())
            .RuleSet(WithoutIndustry, set =>
            {
                set.RuleFor(c => c.Industry, f => null);
            });
    }
}
