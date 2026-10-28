using Ambio.Application.Common;
using Ambio.Application.Companies.Dtos;
using Ambio.Domain.Companies;

using Bogus;

namespace Ambio.Web.Tests.Fixtures;

/// <summary>Company profiles, as returned by the mocked <c>ICompanyService</c>.</summary>
public static class CompanyDetailsFactory
{
    private const string RequiredOnly = "RequiredOnly";

    /// <summary>Pass to <c>Generate</c>: the default rules, then only the required fields kept.</summary>
    public const string RequiredOnlyRuleSet = $"default,{RequiredOnly}";

    public static Faker<CompanyDetailsDto> GetGenerator()
    {
        return new Faker<CompanyDetailsDto>()
            .WithRecord()
            .RuleFor(c => c.Id, f => f.Random.Guid())
            .RuleFor(c => c.Name, f => f.Company.CompanyName())
            .RuleFor(c => c.Slug, (f, c) => Slug.From(c.Name, "company"))
            .RuleFor(c => c.Kind, f => f.PickRandom<CompanyKind>())
            .RuleFor(c => c.Industry, f => f.Commerce.Department())
            .RuleFor(c => c.Size, f => f.PickRandom<CompanySize>())
            .RuleFor(c => c.Location, f => f.Address.FullAddress())
            .RuleFor(c => c.Website, f => f.Internet.Url())
            .RuleFor(c => c.LinkedInUrl, f => f.Internet.Url())
            .RuleFor(c => c.Description, f => f.Lorem.Paragraph())

            .RuleSet(RequiredOnly, set =>
            {
                set.RuleFor(c => c.Industry, f => null);
                set.RuleFor(c => c.Size, f => null);
                set.RuleFor(c => c.Location, f => null);
                set.RuleFor(c => c.Website, f => null);
                set.RuleFor(c => c.LinkedInUrl, f => null);
                set.RuleFor(c => c.Description, f => null);
            });
    }
}
