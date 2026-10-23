using Ambio.Application.Companies;
using Ambio.Application.Companies.Dtos;
using Ambio.Web.Components.Companies;
using Ambio.Web.Components.Companies.Pages;
using Ambio.Web.Components.Companies.Shared;
using Ambio.Web.Tests.Fixtures;

using Bogus;

using Bunit;

using Microsoft.Extensions.DependencyInjection;

using NSubstitute;

namespace Ambio.Web.Tests.Components.Companies.Pages;

public class CompanyListTests : BunitContext
{
    private readonly ICompanyService _companyService = Substitute.For<ICompanyService>();
    private readonly Faker<CompanySummaryDto> _companySummaryFactory;

    public CompanyListTests()
    {
        Services.AddSingleton(_companyService);
        _companySummaryFactory = CompanySummaryFactory.GetGenerator();
    }

    [Fact]
    public void Render_WithoutCompanies_ShowsEmptyStateWithNewCompanyLink()
    {
        GivenCompanies();

        var cut = Render<CompanyList>();

        Assert.NotNull(cut.Find($".empty a[href='{CompanyRoutes.New}']"));
        Assert.Empty(cut.FindAll("table"));
    }

    [Fact]
    public void Render_WithCompanies_ShowsOneTableRowPerCompanyInServiceOrder()
    {
        var companies = _companySummaryFactory.Generate(2);
        GivenCompanies(companies);

        var cut = Render<CompanyList>();

        var rows = cut.FindAll("tbody tr");
        Assert.Equal(2, rows.Count);
        Assert.Contains(companies[0].Name, rows[0].TextContent);
        Assert.Contains(companies[0].Industry!, rows[0].TextContent);
        Assert.Contains(companies[0].Kind.ToLabel(), rows[0].TextContent);
        Assert.Contains(companies[0].Location!, rows[0].TextContent);
        Assert.Contains(companies[1].Name, rows[1].TextContent);
        Assert.Empty(cut.FindAll(".empty"));
    }

    [Fact]
    public void Render_WithCompanies_ShowsCardsWithIndustryAndLocation()
    {
        IReadOnlyList<CompanySummaryDto> companies =
            [_companySummaryFactory.Generate(), _companySummaryFactory.Generate(CompanySummaryFactory.WithoutIndustryRuleSet)];
        GivenCompanies(companies);

        var cut = Render<CompanyList>();

        var details = cut.FindAll(".list-card .list-card-subtitle").Select(d => d.TextContent);
        Assert.Equal([$"{companies[0].Industry} · {companies[0].Location}", companies[1].Location], details);
    }

    [Fact]
    public void Render_WithCompanies_LinksEachRowAndCardToItsDetails()
    {
        var companies = _companySummaryFactory.Generate(2);
        GivenCompanies(companies);

        var cut = Render<CompanyList>();

        string[] expected = [CompanyRoutes.Details(companies[0].Slug), CompanyRoutes.Details(companies[1].Slug)];
        Assert.Equal(expected, cut.FindAll("tbody a").Select(link => link.GetAttribute("href")));
        Assert.Equal(expected, cut.FindAll("a.list-card").Select(link => link.GetAttribute("href")));
    }

    [Fact]
    public void Render_WithCompanies_LinksToNewCompany()
    {
        var company = _companySummaryFactory.Generate();
        GivenCompanies(company);

        var cut = Render<CompanyList>();

        Assert.NotNull(cut.Find($".page-header a[href='{CompanyRoutes.New}']"));
    }

    private void GivenCompanies(params IReadOnlyList<CompanySummaryDto> companies) =>
        _companyService.ListAsync(Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(new CompanyListDto(companies, HasArchived: false));
}
