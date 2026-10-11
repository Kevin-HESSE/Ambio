using Ambio.Application.Companies;
using Ambio.Application.Companies.Dtos;
using Ambio.Application.Offers;
using Ambio.Application.Offers.Dtos;
using Ambio.Domain.Companies;
using Ambio.Domain.Offers;
using Ambio.Web.Components.Companies;
using Ambio.Web.Components.Companies.Pages;
using Ambio.Web.Components.Offers;
using Ambio.Web.Tests.Fixtures;

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
    private readonly IJobOfferService _jobOfferService = Substitute.For<IJobOfferService>();

    public CompanyDetailsTests()
    {
        Services.AddSingleton(_companyService);
        Services.AddSingleton(_jobOfferService);
        _jobOfferService.ListByCompanyAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns([]);
    }

    [Fact]
    public void Render_WithCompany_ShowsNameAndKind()
    {
        GivenCompany(Brightwave);

        var cut = RenderDetails(Brightwave.Slug);

        Assert.Equal(CompanyName, cut.Find("h1").TextContent);
        Assert.Equal("IT services", cut.Find(".detail-tags .tag").TextContent.Trim());
    }

    [Fact]
    public void Render_WithEveryField_ShowsWholeProfile()
    {
        GivenCompany(Brightwave);

        var cut = RenderDetails(Brightwave.Slug);

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

        var cut = RenderDetails(Brightwave.Slug);

        Assert.Equal(Description, cut.Find(".company-description").TextContent);
    }

    [Fact]
    public void Render_WithOnlyRequiredFields_SaysDetailsAndDescriptionAreMissing()
    {
        GivenCompany(Talentis);

        var cut = RenderDetails(Talentis.Slug);

        Assert.Empty(cut.FindAll(".company-profile dl"));
        Assert.Equal("No details yet.", cut.Find(".company-profile-empty").TextContent);
        Assert.Equal("No description yet.", cut.Find(".company-description-empty").TextContent);
    }

    [Fact]
    public void Render_WithCompany_LinksToEditPage()
    {
        GivenCompany(Brightwave);

        var cut = RenderDetails(Brightwave.Slug);

        Assert.Equal("Edit", cut.Find($".detail-actions a[href='{CompanyRoutes.Edit(Brightwave.Slug)}']").TextContent.Trim());
    }

    [Fact]
    public void Render_WithCompany_BreadcrumbLinksToList()
    {
        GivenCompany(Brightwave);

        var cut = RenderDetails(Brightwave.Slug);

        Assert.Equal("Companies", cut.Find($".detail-breadcrumb a[href='{CompanyRoutes.List}']").TextContent);
    }

    [Fact]
    public void Render_WithOffers_ShowsActiveOffersTabWithCountAndLink()
    {
        GivenCompany(Brightwave);
        GivenOffers(Brightwave, BackendDeveloper, MobileDeveloper);

        var cut = RenderDetails(Brightwave.Slug);

        var tab = cut.Find("nav.tabs a.tab.active");
        Assert.Equal(CompanyRoutes.Details(Brightwave.Slug, CompanyRoutes.OffersTab), tab.GetAttribute("href"));
        Assert.Equal("2", tab.QuerySelector(".tab-count")!.TextContent);
    }

    [Fact]
    public void Render_WithOffers_ListsThemWithTheirTags()
    {
        GivenCompany(Brightwave);
        GivenOffers(Brightwave, BackendDeveloper, MobileDeveloper);

        var cut = RenderDetails(Brightwave.Slug);

        var offers = cut.FindAll(".offer-item").Select(item => (
            item.QuerySelector(".offer-title")!.TextContent,
            string.Join(", ", item.QuerySelectorAll(".tag").Select(tag => tag.TextContent.Trim()))));
        Assert.Equal([("Backend Developer", "Permanent, Hybrid"), ("Mobile Developer", "Freelance, Full remote")], offers);
        Assert.Empty(cut.FindAll(".empty"));
    }

    [Fact]
    public void Render_WithoutOffers_ShowsEmptyTabWithNewOfferLink()
    {
        GivenCompany(Brightwave);

        var cut = RenderDetails(Brightwave.Slug);

        Assert.Equal("0", cut.Find("nav.tabs .tab-count").TextContent);
        Assert.NotNull(cut.Find($".empty a[href='{OfferRoutes.New}']"));
        Assert.Empty(cut.FindAll(".offer-item"));
    }

    [Fact]
    public void Render_WithUnknownSlug_RequestsNotFound()
    {
        var notFound = false;
        Services.GetRequiredService<NavigationManager>().OnNotFound += (_, _) => notFound = true;

        var cut = RenderDetails("kelvio");

        Assert.True(notFound);
        Assert.Empty(cut.FindAll("h1"));
    }

    private void GivenCompany(CompanyDetailsDto company) =>
        _companyService.GetAsync(company.Slug, Arg.Any<CancellationToken>()).Returns(company);

    private void GivenOffers(CompanyDetailsDto company, params JobOfferSummaryDto[] offers) =>
        _jobOfferService.ListByCompanyAsync(company.Id, Arg.Any<CancellationToken>()).Returns(offers);

    private IRenderedComponent<CompanyDetails> RenderDetails(string slug) =>
        Render<CompanyDetails>(parameters => parameters.Add(page => page.Slug, slug));

    private static CompanyDetailsDto Brightwave { get; } = CompanyDetailsFactory.New(
        name: CompanyName,
        kind: CompanyKind.ServiceCompany,
        industry: "Software",
        size: CompanySize.Medium,
        location: "Lyon",
        website: "https://brightwave.io",
        linkedInUrl: "https://www.linkedin.com/company/brightwave/",
        description: Description);

    private static CompanyDetailsDto Talentis { get; } =
        CompanyDetailsFactory.New(
            name: "Talentis Recruitment",
            kind: CompanyKind.RecruitmentAgency);

    private static JobOfferSummaryDto BackendDeveloper { get; } =
        JobOfferSummaryFactory.New(title: "Backend Developer", companyName: CompanyName);

    private static JobOfferSummaryDto MobileDeveloper { get; } =
        JobOfferSummaryFactory.New("Mobile Developer", CompanyName, ContractType.Freelance, RemotePolicy.FullRemote);
}
