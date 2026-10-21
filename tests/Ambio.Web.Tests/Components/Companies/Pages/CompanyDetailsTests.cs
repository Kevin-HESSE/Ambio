using Ambio.Application.Companies;
using Ambio.Application.Companies.Dtos;
using Ambio.Application.JobApplications;
using Ambio.Application.JobApplications.Dtos;
using Ambio.Application.Offers;
using Ambio.Application.Offers.Dtos;
using Ambio.Web.Components.Companies;
using Ambio.Web.Components.Companies.Pages;
using Ambio.Web.Components.Companies.Shared;
using Ambio.Web.Components.JobApplications;
using Ambio.Web.Components.JobApplications.Shared;
using Ambio.Web.Components.Offers;
using Ambio.Web.Components.Offers.Shared;
using Ambio.Web.Components.Shared;
using Ambio.Web.Tests.Fixtures;

using Bogus;

using Bunit;
using Bunit.TestDoubles;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

using NSubstitute;

namespace Ambio.Web.Tests.Components.Companies.Pages;

public class CompanyDetailsTests : BunitContext
{
    private readonly ICompanyService _companyService = Substitute.For<ICompanyService>();
    private readonly IJobOfferService _jobOfferService = Substitute.For<IJobOfferService>();
    private readonly IJobApplicationService _jobApplicationService = Substitute.For<IJobApplicationService>();
    private readonly Faker<CompanyDetailsDto> _companyDetailsFactory;
    private readonly Faker<JobOfferSummaryDto> _jobOfferSummaryFactory;

    public CompanyDetailsTests()
    {
        Services.AddSingleton(_companyService);
        Services.AddSingleton(_jobOfferService);
        Services.AddSingleton(_jobApplicationService);
        _jobOfferService.ListByCompanyAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns([]);
        _jobApplicationService.ListByCompanyAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns([]);
        _companyDetailsFactory = CompanyDetailsFactory.GetGenerator();
        _jobOfferSummaryFactory = JobOfferSummaryFactory.GetGenerator();
    }

    [Fact]
    public void Render_WithCompany_ShowsNameAndKind()
    {
        var company = _companyDetailsFactory.Generate();
        GivenCompany(company);

        var cut = RenderDetails(company.Slug);

        Assert.Equal(company.Name, cut.Find("h1").TextContent);
        Assert.Equal(company.Kind.ToLabel(), cut.Find(".detail-tags .tag").TextContent.Trim());
    }

    [Fact]
    public void Render_WithEveryField_ShowsWholeProfile()
    {
        var company = _companyDetailsFactory.Generate();
        GivenCompany(company);

        var cut = RenderDetails(company.Slug);

        var profile = cut.FindAll(".company-profile dl > div")
            .Select(row => (row.QuerySelector("dt")!.TextContent, row.QuerySelector("dd")!.TextContent.Trim()));
        Assert.Equal(
            [
                ("Website", ExternalLink.ShortText(company.Website!)),
                ("Industry", company.Industry!),
                ("Size", company.Size!.Value.ToLabel()),
                ("Location", company.Location!),
                ("LinkedIn", ExternalLink.ShortText(company.LinkedInUrl!)),
            ],
            profile);
        Assert.NotNull(cut.Find($".company-profile a[href='{company.Website}'][target='_blank']"));
    }

    [Fact]
    public void Render_WithDescription_ShowsIt()
    {
        var company = _companyDetailsFactory.Generate();
        GivenCompany(company);

        var cut = RenderDetails(company.Slug);

        Assert.Equal(company.Description, cut.Find(".company-description").TextContent);
    }

    [Fact]
    public void Render_WithOnlyRequiredFields_SaysDetailsAndDescriptionAreMissing()
    {
        var company = _companyDetailsFactory.Generate(CompanyDetailsFactory.RequiredOnlyRuleSet);
        GivenCompany(company);

        var cut = RenderDetails(company.Slug);

        Assert.Empty(cut.FindAll(".company-profile dl"));
        Assert.Equal("No details yet.", cut.Find(".company-profile-empty").TextContent);
        Assert.Equal("No description yet.", cut.Find(".company-description-empty").TextContent);
    }

    [Fact]
    public void Render_WithCompany_LinksToEditPage()
    {
        var company = _companyDetailsFactory.Generate();
        GivenCompany(company);

        var cut = RenderDetails(company.Slug);

        Assert.Equal("Edit", cut.Find($".detail-actions a[href='{CompanyRoutes.Edit(company.Slug)}']").TextContent.Trim());
    }

    [Fact]
    public void Render_WithCompany_BreadcrumbLinksToList()
    {
        var company = _companyDetailsFactory.Generate();
        GivenCompany(company);

        var cut = RenderDetails(company.Slug);

        Assert.Equal("Companies", cut.Find($".detail-breadcrumb a[href='{CompanyRoutes.List}']").TextContent);
    }

    [Fact]
    public void Render_WithOffers_ShowsActiveOffersTabWithCountAndLink()
    {
        var company = _companyDetailsFactory.Generate();
        var offers = _jobOfferSummaryFactory.Generate(2);
        GivenCompany(company);
        GivenOffers(company, offers);

        var cut = RenderDetails(company.Slug);

        var tab = cut.Find("nav.tabs a.tab.active");
        Assert.Equal(CompanyRoutes.Details(company.Slug, CompanyRoutes.OffersTab), tab.GetAttribute("href"));
        Assert.Equal("2", tab.QuerySelector(".tab-count")!.TextContent);
    }

    [Fact]
    public void Render_WithOffers_ListsThemWithTheirTags()
    {
        var company = _companyDetailsFactory.Generate();
        var jobOffers = _jobOfferSummaryFactory.Generate(2);
        GivenCompany(company);
        GivenOffers(company, jobOffers);

        var cut = RenderDetails(company.Slug);

        var offers = cut.FindAll(".offer-item").Select(item => (
            item.QuerySelector(".offer-title")!.TextContent,
            string.Join(", ", item.QuerySelectorAll(".tag").Select(tag => tag.TextContent.Trim()))));
        Assert.Equal([Summary(jobOffers[0]), Summary(jobOffers[1])], offers);
        Assert.Empty(cut.FindAll(".empty"));
    }

    [Fact]
    public void Render_WithOffers_LinksEachToTheOfferPage()
    {
        var company = _companyDetailsFactory.Generate();
        var offers = _jobOfferSummaryFactory.Generate(2);
        GivenCompany(company);
        GivenOffers(company, offers);

        var cut = RenderDetails(company.Slug);

        Assert.Equal(
            offers.Select(offer => OfferRoutes.Details(offer.ShortId, offer.Slug)),
            cut.FindAll("a.offer-item").Select(link => link.GetAttribute("href")));
    }

    [Fact]
    public void Render_WithoutOffers_ShowsEmptyTabWithNewOfferLink()
    {
        var company = _companyDetailsFactory.Generate();
        GivenCompany(company);

        var cut = RenderDetails(company.Slug);

        Assert.Equal("0", cut.Find("nav.tabs .tab-count").TextContent);
        Assert.NotNull(cut.Find($".empty a[href='{OfferRoutes.New}']"));
        Assert.Empty(cut.FindAll(".offer-item"));
    }

    [Fact]
    public void Render_WithApplications_CountsThemInTheirTabWithoutShowingThem()
    {
        var company = _companyDetailsFactory.Generate();
        GivenCompany(company);
        GivenApplications(company, JobApplicationSummaryFactory.GetGenerator().Generate(3));

        var cut = RenderDetails(company.Slug);

        var tab = cut.Find($"nav.tabs a.tab[href='{CompanyRoutes.Details(company.Slug, CompanyRoutes.ApplicationsTab)}']");
        Assert.Equal("3", tab.QuerySelector(".tab-count")!.TextContent);
        Assert.DoesNotContain("active", tab.ClassList);
        Assert.Empty(cut.FindAll(".application-item"));
    }

    [Fact]
    public void Render_OnApplicationsTab_ListsTheApplicationsWithTheirTitle()
    {
        var company = _companyDetailsFactory.Generate();
        var applications = JobApplicationSummaryFactory.GetGenerator().Generate(2);
        GivenCompany(company);
        GivenApplications(company, applications);
        NavigateToTab(company, CompanyRoutes.ApplicationsTab);

        var cut = RenderDetails(company.Slug);

        Assert.Equal(CompanyRoutes.Details(company.Slug, CompanyRoutes.ApplicationsTab), cut.Find("nav.tabs a.tab.active").GetAttribute("href"));
        var items = cut.FindAll(".application-item").Select(item => (
            item.QuerySelector(".application-item-title")!.TextContent,
            item.QuerySelector(".status-badge")!.TextContent.Trim()));
        Assert.Equal(applications.Select(a => (a.Title, a.Status.ToLabel())), items);
        Assert.Equal(
            applications.Select(a => JobApplicationRoutes.Details(a.ShortId, a.Slug)),
            cut.FindAll("a.application-item").Select(item => item.GetAttribute("href")));
        Assert.Empty(cut.FindAll(".offer-item"));
    }

    [Fact]
    public void Render_OnApplicationsTabWithSpontaneousApplication_TagsIt()
    {
        var company = _companyDetailsFactory.Generate();
        var factory = JobApplicationSummaryFactory.GetGenerator();
        GivenCompany(company);
        GivenApplications(company, factory.Generate(), factory.Generate(JobApplicationSummaryFactory.IsSpontaneousRuleSet));
        NavigateToTab(company, CompanyRoutes.ApplicationsTab);

        var cut = RenderDetails(company.Slug);

        Assert.Equal(
            [false, true],
            cut.FindAll(".application-item").Select(item => item.QuerySelector(".tag-accent") is not null));
    }

    [Fact]
    public void Render_OnApplicationsTabWithoutApplications_ShowsEmptyStateWithNewApplicationToTheCompanyLink()
    {
        var company = _companyDetailsFactory.Generate();
        GivenCompany(company);
        NavigateToTab(company, CompanyRoutes.ApplicationsTab);

        var cut = RenderDetails(company.Slug);

        Assert.NotNull(cut.Find($".empty a[href='{JobApplicationRoutes.NewForCompany(company.Slug)}']"));
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

    private void GivenOffers(CompanyDetailsDto company, params IReadOnlyList<JobOfferSummaryDto> offers) =>
        _jobOfferService.ListByCompanyAsync(company.Id, Arg.Any<CancellationToken>()).Returns(offers);

    private void GivenApplications(CompanyDetailsDto company, params IReadOnlyList<JobApplicationSummaryDto> applications) =>
        _jobApplicationService.ListByCompanyAsync(company.Id, Arg.Any<CancellationToken>()).Returns(applications);

    private void NavigateToTab(CompanyDetailsDto company, string tab) =>
        Services.GetRequiredService<BunitNavigationManager>().NavigateTo(CompanyRoutes.Details(company.Slug, tab));

    private IRenderedComponent<CompanyDetails> RenderDetails(string slug) =>
        Render<CompanyDetails>(parameters => parameters.Add(page => page.Slug, slug));

    private static (string Title, string Tags) Summary(JobOfferSummaryDto offer) =>
        (offer.Title, $"{offer.ContractType.ToLabel()}, {offer.RemotePolicy.ToLabel()}");
}
