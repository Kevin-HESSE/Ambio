using Ambio.Application.Companies;
using Ambio.Application.Companies.Dtos;
using Ambio.Domain.Companies;
using Ambio.Web.Components.Companies;
using Ambio.Web.Components.Companies.Pages;

using Bunit;

using Microsoft.Extensions.DependencyInjection;

using NSubstitute;

namespace Ambio.Web.Tests.Components.Companies.Pages;

public class CompanyListTests : BunitContext
{
    private readonly ICompanyService _companyService = Substitute.For<ICompanyService>();

    public CompanyListTests()
    {
        Services.AddSingleton(_companyService);
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
        GivenCompanies(Brightwave, Talentis);

        var cut = Render<CompanyList>();

        var rows = cut.FindAll("tbody tr");
        Assert.Equal(2, rows.Count);
        Assert.Contains("Brightwave", rows[0].TextContent);
        Assert.Contains("Software", rows[0].TextContent);
        Assert.Contains("Employer", rows[0].TextContent);
        Assert.Contains("Lyon", rows[0].TextContent);
        Assert.Contains("Recruitment agency", rows[1].TextContent);
        Assert.Empty(cut.FindAll(".empty"));
    }

    [Fact]
    public void Render_WithCompanies_ShowsCardsWithIndustryAndLocation()
    {
        GivenCompanies(Brightwave, Talentis);

        var cut = Render<CompanyList>();

        var details = cut.FindAll(".company-card .company-details").Select(d => d.TextContent);
        Assert.Equal(["Software · Lyon", "Paris"], details);
    }

    [Fact]
    public void Render_WithCompanies_LinksEachRowAndCardToItsDetails()
    {
        GivenCompanies(Brightwave, Talentis);

        var cut = Render<CompanyList>();

        string[] expected = [CompanyRoutes.Details(Brightwave.Slug), CompanyRoutes.Details(Talentis.Slug)];
        Assert.Equal(expected, cut.FindAll("tbody a").Select(link => link.GetAttribute("href")));
        Assert.Equal(expected, cut.FindAll("a.company-card").Select(link => link.GetAttribute("href")));
    }

    [Fact]
    public void Render_WithCompanies_LinksToNewCompany()
    {
        GivenCompanies(Brightwave);

        var cut = Render<CompanyList>();

        Assert.NotNull(cut.Find($".page-header a[href='{CompanyRoutes.New}']"));
    }

    private void GivenCompanies(params CompanySummaryDto[] companies) =>
        _companyService.ListAsync(Arg.Any<CancellationToken>()).Returns(companies);

    private static CompanySummaryDto Brightwave { get; } =
        new(Guid.CreateVersion7(), "brightwave", "Brightwave", CompanyKind.Employer, "Software", "Lyon");

    private static CompanySummaryDto Talentis { get; } =
        new(Guid.CreateVersion7(), "talentis-recruitment", "Talentis Recruitment", CompanyKind.RecruitmentAgency, null, "Paris");
}
