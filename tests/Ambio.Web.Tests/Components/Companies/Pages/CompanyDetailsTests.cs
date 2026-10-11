using Ambio.Application.Companies;
using Ambio.Application.Companies.Dtos;
using Ambio.Domain.Companies;
using Ambio.Web.Components.Companies;
using Ambio.Web.Components.Companies.Pages;

using Bunit;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

using NSubstitute;

namespace Ambio.Web.Tests.Components.Companies.Pages;

public class CompanyDetailsTests : BunitContext
{
    private const string CompanyName = "Brightwave";
    private const string Description = "Energy-monitoring software.\nAbout 120 people.";

    private readonly ICompanyService _companyService = Substitute.For<ICompanyService>();

    public CompanyDetailsTests()
    {
        Services.AddSingleton(_companyService);
    }

    [Fact]
    public void Render_WithCompany_ShowsNameAndKind()
    {
        GivenCompany(Brightwave);

        var cut = RenderDetails(Brightwave.Id);

        Assert.Equal(CompanyName, cut.Find("h1").TextContent);
        Assert.Equal("IT services", cut.Find(".detail-tags .tag").TextContent.Trim());
    }

    [Fact]
    public void Render_WithEveryField_ShowsWholeProfile()
    {
        GivenCompany(Brightwave);

        var cut = RenderDetails(Brightwave.Id);

        var profile = cut.FindAll(".company-profile dl > div")
            .Select(row => (row.QuerySelector("dt")!.TextContent, row.QuerySelector("dd")!.TextContent.Trim()));
        Assert.Equal(
            [
                ("Website", "brightwave.io"),
                ("Industry", "Software"),
                ("Size", "51–250 employees"),
                ("Location", "Lyon"),
                ("LinkedIn", "linkedin.com/company/brightwave"),
            ],
            profile);
        Assert.NotNull(cut.Find($".company-profile a[href='{Brightwave.Website}'][target='_blank']"));
    }

    [Fact]
    public void Render_WithDescription_ShowsIt()
    {
        GivenCompany(Brightwave);

        var cut = RenderDetails(Brightwave.Id);

        Assert.Equal(Description, cut.Find(".company-description").TextContent);
    }

    [Fact]
    public void Render_WithOnlyRequiredFields_SaysDetailsAndDescriptionAreMissing()
    {
        GivenCompany(Talentis);

        var cut = RenderDetails(Talentis.Id);

        Assert.Empty(cut.FindAll(".company-profile dl"));
        Assert.Equal("No details yet.", cut.Find(".company-profile-empty").TextContent);
        Assert.Equal("No description yet.", cut.Find(".company-description-empty").TextContent);
    }

    [Fact]
    public void Render_WithCompany_LinksToEditPage()
    {
        GivenCompany(Brightwave);

        var cut = RenderDetails(Brightwave.Id);

        Assert.Equal("Edit", cut.Find($".detail-actions a[href='{CompanyRoutes.Edit(Brightwave.Id)}']").TextContent.Trim());
    }

    [Fact]
    public void Render_WithCompany_BreadcrumbLinksToList()
    {
        GivenCompany(Brightwave);

        var cut = RenderDetails(Brightwave.Id);

        Assert.Equal("Companies", cut.Find($".detail-breadcrumb a[href='{CompanyRoutes.List}']").TextContent);
    }

    [Fact]
    public void Render_WithUnknownId_RequestsNotFound()
    {
        var notFound = false;
        Services.GetRequiredService<NavigationManager>().OnNotFound += (_, _) => notFound = true;

        var cut = RenderDetails(Guid.CreateVersion7());

        Assert.True(notFound);
        Assert.Empty(cut.FindAll("h1"));
    }

    private void GivenCompany(CompanyDetailsDto company) =>
        _companyService.GetAsync(company.Id, Arg.Any<CancellationToken>()).Returns(company);

    private IRenderedComponent<CompanyDetails> RenderDetails(Guid id) =>
        Render<CompanyDetails>(parameters => parameters.Add(page => page.Id, id));

    private static CompanyDetailsDto Brightwave { get; } = new(
        Guid.CreateVersion7(),
        CompanyName,
        CompanyKind.ServiceCompany,
        "Software",
        CompanySize.Medium,
        "Lyon",
        "https://brightwave.io",
        "https://www.linkedin.com/company/brightwave/",
        Description);

    private static CompanyDetailsDto Talentis { get; } = new(
        Guid.CreateVersion7(), "Talentis Recruitment", CompanyKind.RecruitmentAgency, null, null, null, null, null, null);
}
