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
    private readonly JobOfferDetailsDto _offer = JobOfferDetailsFactory.GetGenerator().Generate();

    public JobApplicationNewTests()
    {
        Services.AddSingleton(_jobOfferService);
        Services.AddSingleton(Substitute.For<IJobApplicationService>());
        // The page is static; its form is interactive once the circuit is up.
        SetRendererInfo(new RendererInfo("Static", isInteractive: false));
    }

    [Fact]
    public void Render_WithOfferInTheUrl_GivesItToTheForm()
    {
        _jobOfferService.GetAsync(_offer.Id, Arg.Any<CancellationToken>()).Returns(_offer);
        Services.GetRequiredService<BunitNavigationManager>().NavigateTo(JobApplicationRoutes.NewForOffer(_offer.Id));

        var cut = Render<JobApplicationNew>();

        Assert.Equal($"{_offer.Title} · {_offer.CompanyName}", cut.Find(".application-offer-name").TextContent);
    }

    [Fact]
    public void Render_WithUnknownOfferInTheUrl_ShowsTheSearch()
    {
        Services.GetRequiredService<BunitNavigationManager>().NavigateTo(JobApplicationRoutes.NewForOffer(_offer.Id));

        var cut = Render<JobApplicationNew>();

        Assert.NotNull(cut.Find("input#application-offer[type='search']"));
    }

    [Fact]
    public void Render_WithoutOffer_ShowsTheSearchWithoutLoadingAnOffer()
    {
        Services.GetRequiredService<BunitNavigationManager>().NavigateTo(JobApplicationRoutes.New);

        var cut = Render<JobApplicationNew>();

        Assert.NotNull(cut.Find("input#application-offer[type='search']"));
        _jobOfferService.DidNotReceive().GetAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }
}
