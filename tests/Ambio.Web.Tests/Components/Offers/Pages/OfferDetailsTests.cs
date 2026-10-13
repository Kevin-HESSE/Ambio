using Ambio.Application.Offers;
using Ambio.Application.Offers.Dtos;
using Ambio.Web.Components.Companies;
using Ambio.Web.Components.Offers;
using Ambio.Web.Components.Offers.Pages;
using Ambio.Web.Components.Offers.Shared;
using Ambio.Web.Tests.Fixtures;

using Bogus;

using Bunit;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

using NSubstitute;

namespace Ambio.Web.Tests.Components.Offers.Pages;

public class OfferDetailsTests : BunitContext
{
    private static readonly DateTime Published = new(2026, 8, 28, 9, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime FirstSeen = new(2026, 8, 28, 9, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime LastSeen = new(2026, 9, 2, 9, 0, 0, DateTimeKind.Utc);

    private readonly IJobOfferService _jobOfferService = Substitute.For<IJobOfferService>();
    private readonly Faker<JobOfferDetailsDto> _jobOfferDetailsFactory;

    public OfferDetailsTests()
    {
        Services.AddSingleton(_jobOfferService);
        // The page is static; its postings card is interactive once the circuit is up.
        SetRendererInfo(new RendererInfo("Static", isInteractive: false));
        _jobOfferDetailsFactory = JobOfferDetailsFactory.GetGenerator();
    }

    [Fact]
    public void Render_WithOffer_ShowsTitleCompanyLocationAndPublicationDate()
    {
        var offer = _jobOfferDetailsFactory.Generate() with { RegisterAt = Published };
        GivenOffer(offer);

        var cut = RenderDetails(offer.Id);

        Assert.Equal(offer.Title, cut.Find("h1").TextContent);
        Assert.Equal(
            $"{offer.CompanyName} · {offer.Location} · Published 28 Aug 2026",
            cut.Find(".detail-subtitle").TextContent.Trim());
        Assert.Equal(offer.CompanyName, cut.Find($".detail-subtitle a[href='{CompanyRoutes.Details(offer.CompanySlug)}']").TextContent);
    }

    [Fact]
    public void Render_WithOffer_ShowsContractRemotePolicyAndSalaryTags()
    {
        var offer = _jobOfferDetailsFactory.Generate();
        GivenOffer(offer);

        var cut = RenderDetails(offer.Id);

        Assert.Equal(
            [offer.ContractType.ToLabel(), offer.RemotePolicy.ToLabel(), offer.Salary!],
            cut.FindAll(".detail-tags .tag").Select(tag => tag.TextContent.Trim()));
    }

    [Fact]
    public void Render_WithOnlyRequiredFields_LeavesOutLocationSalaryAndAdText()
    {
        var offer = _jobOfferDetailsFactory.Generate(JobOfferDetailsFactory.RequiredOnlyRuleSet);
        GivenOffer(offer);

        var cut = RenderDetails(offer.Id);

        Assert.Empty(cut.FindAll(".offer-location"));
        Assert.Equal(2, cut.FindAll(".detail-tags .tag").Count);
        Assert.Equal("No copy of the ad yet.", cut.Find(".offer-description-empty").TextContent);
    }

    [Fact]
    public void Render_WithDescription_ShowsIt()
    {
        var offer = _jobOfferDetailsFactory.Generate();
        GivenOffer(offer);

        var cut = RenderDetails(offer.Id);

        Assert.Equal(offer.Description, cut.Find(".offer-description").TextContent);
    }

    [Fact]
    public void Render_WithPostings_ListsThemWithSiteLinkAndDaySeen()
    {
        var offer = _jobOfferDetailsFactory.Generate();
        offer = offer with
        {
            Postings = [offer.Postings[0] with { SeenAt = FirstSeen }, offer.Postings[1] with { SeenAt = LastSeen }],
        };
        GivenOffer(offer);

        var cut = RenderDetails(offer.Id);

        var postings = cut.FindAll(".offer-posting").Select(posting => (
            posting.QuerySelector(".offer-posting-site")!.TextContent,
            posting.QuerySelector("a[target='_blank']")!.GetAttribute("href"),
            posting.QuerySelector(".offer-posting-date")!.TextContent));
        Assert.Equal(
            [
                (offer.Postings[0].Site, offer.Postings[0].Url, "28 Aug"),
                (offer.Postings[1].Site, offer.Postings[1].Url, "2 Sep"),
            ],
            postings);
        Assert.Equal("2", cut.Find(".offer-postings-count").TextContent);
    }

    [Fact]
    public void Render_WithOffer_BreadcrumbLinksToList()
    {
        var offer = _jobOfferDetailsFactory.Generate();
        GivenOffer(offer);

        var cut = RenderDetails(offer.Id);

        Assert.Equal("Offers", cut.Find($".detail-breadcrumb a[href='{OfferRoutes.List}']").TextContent);
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

    private void GivenOffer(JobOfferDetailsDto offer) =>
        _jobOfferService.GetAsync(offer.Id, Arg.Any<CancellationToken>()).Returns(offer);

    private IRenderedComponent<OfferDetails> RenderDetails(Guid id) =>
        Render<OfferDetails>(parameters => parameters.Add(page => page.Id, id));
}
