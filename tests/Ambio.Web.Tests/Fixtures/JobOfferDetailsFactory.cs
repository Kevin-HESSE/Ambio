using Ambio.Application.Common;
using Ambio.Application.Offers.Dtos;
using Ambio.Domain.Offers;

using Bogus;

namespace Ambio.Web.Tests.Fixtures;

/// <summary>Offers with their postings, as returned by the mocked <c>IJobOfferService</c>.</summary>
public static class JobOfferDetailsFactory
{
    private const string RequiredOnly = "RequiredOnly";

    /// <summary>Pass to <c>Generate</c>: the default rules, then only the required fields kept.</summary>
    public const string RequiredOnlyRuleSet = $"default,{RequiredOnly}";

    public static Faker<JobOfferDetailsDto> GetGenerator(int numberOfPosting = 2)
    {
        var postingFactory = JobOfferPostingFactory.GetGenerator();

        return new Faker<JobOfferDetailsDto>()
            .WithRecord()
            .RuleFor(o => o.Id, Guid.CreateVersion7)
            .RuleFor(o => o.Title, f => f.Name.JobTitle())
            .RuleFor(o => o.CompanyName, f => f.Company.CompanyName())
            .RuleFor(o => o.CompanySlug, (f, o) => Slug.From(o.CompanyName, "company"))
            .RuleFor(o => o.ContractType, f => f.PickRandom<ContractType>())
            .RuleFor(o => o.RemotePolicy, f => f.PickRandom<RemotePolicy>())
            .RuleFor(o => o.Location, f => f.Address.City())
            .RuleFor(o => o.Salary, f => $"{f.Random.Int(30, 80)} k€")
            .RuleFor(o => o.Description, f => f.Lorem.Paragraphs(2))
            .RuleFor(o => o.RegisterAt, f => f.Date.RecentOffset().UtcDateTime)
            .RuleFor(o => o.Postings, _ => postingFactory.Generate(numberOfPosting))

            .RuleSet(RequiredOnly, set =>
            {
                set.RuleFor(o => o.Location, f => null);
                set.RuleFor(o => o.Salary, f => null);
                set.RuleFor(o => o.Description, f => null);
            });
    }
}
