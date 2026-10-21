using Ambio.Application.Companies;
using Ambio.Application.Companies.Dtos;
using Ambio.Application.JobApplications;
using Ambio.Application.Offers;
using Ambio.Application.Offers.Dtos;
using Ambio.Web.Components.JobApplications;
using Ambio.Web.Components.JobApplications.Pages;
using Ambio.Web.Tests.Fixtures;

using Bunit;
using Bunit.TestDoubles;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

using NSubstitute;

namespace Ambio.Web.Tests.Components.JobApplications.Pages;

public class JobApplicationNewTests : BunitContext
{
    private readonly IJobOfferService _jobOfferService = Substitute.For<IJobOfferService>();
    private readonly ICompanyService _companyService = Substitute.For<ICompanyService>();
    private readonly JobOfferDetailsDto _offer = JobOfferDetailsFactory.GetGenerator().Generate();
    private readonly CompanyDetailsDto _company = CompanyDetailsFactory.GetGenerator().Generate();

    public JobApplicationNewTests()
    {
        Services.AddSingleton(_jobOfferService);
        Services.AddSingleton(_companyService);
        Services.AddSingleton(Substitute.For<IJobApplicationService>());
        // The page is static; its form is interactive once the circuit is up.
        SetRendererInfo(new RendererInfo("Static", isInteractive: false));
    }

    [Fact]
    public void Render_WithOfferInTheUrl_GivesItToTheForm()
    {
        _jobOfferService.GetAsync(_offer.ShortId, Arg.Any<CancellationToken>()).Returns(_offer);
        Services.GetRequiredService<BunitNavigationManager>().NavigateTo(JobApplicationRoutes.NewForOffer(_offer.ShortId));

        var cut = Render<JobApplicationNew>();

        Assert.Equal($"{_offer.Title} · {_offer.CompanyName}", cut.Find(".application-offer-name").TextContent);
    }

    [Fact]
    public void Render_WithUnknownOfferInTheUrl_ShowsTheSearch()
    {
        Services.GetRequiredService<BunitNavigationManager>().NavigateTo(JobApplicationRoutes.NewForOffer(_offer.ShortId));

        var cut = Render<JobApplicationNew>();

        Assert.NotNull(cut.Find("input#application-offer[type='search']"));
    }

    [Fact]
    public void Render_WithoutOffer_ShowsTheSearchWithoutLoadingAnOffer()
    {
        Services.GetRequiredService<BunitNavigationManager>().NavigateTo(JobApplicationRoutes.New);

        var cut = Render<JobApplicationNew>();

        Assert.NotNull(cut.Find("input#application-offer[type='search']"));
        _jobOfferService.DidNotReceive().GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public void Render_WithCompanyInTheUrl_StartsSpontaneousWithIt()
    {
        _companyService.GetAsync(_company.Slug, Arg.Any<CancellationToken>()).Returns(_company);
        Services.GetRequiredService<BunitNavigationManager>().NavigateTo(JobApplicationRoutes.NewForCompany(_company.Slug));

        var cut = Render<JobApplicationNew>();

        Assert.Equal(_company.Name, cut.Find(".application-company-name").TextContent.Trim());
    }

    [Fact]
    public void Render_WithUnknownCompanyInTheUrl_ShowsTheOfferSearch()
    {
        Services.GetRequiredService<BunitNavigationManager>().NavigateTo(JobApplicationRoutes.NewForCompany(_company.Slug));

        var cut = Render<JobApplicationNew>();

        Assert.NotNull(cut.Find("input#application-offer[type='search']"));
    }
}
