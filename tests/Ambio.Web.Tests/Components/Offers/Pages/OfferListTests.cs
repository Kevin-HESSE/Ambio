using Ambio.Application.Offers;
using Ambio.Application.Offers.Dtos;
using Ambio.Web.Components.Offers;
using Ambio.Web.Components.Offers.Pages;
using Ambio.Web.Components.Offers.Shared;
using Ambio.Web.Tests.Fixtures;

using Bunit;

using Microsoft.Extensions.DependencyInjection;

using NSubstitute;

namespace Ambio.Web.Tests.Components.Offers.Pages;

public class OfferListTests : BunitContext
{
    private readonly IJobOfferService _jobOfferService = Substitute.For<IJobOfferService>();
    private readonly JobOfferSummaryDto[] _offers;

    public OfferListTests()
    {
        Services.AddSingleton(_jobOfferService);
        var jobOfferSummaryFactory = JobOfferSummaryFactory.GetGenerator();
        _offers =
        [
            jobOfferSummaryFactory.Generate() with
            {
                PostingCount = 3,
                RegisterAt = new DateTime(2026, 8, 28, 9, 0, 0, DateTimeKind.Utc),
            },
            jobOfferSummaryFactory.Generate() with
            {
                PostingCount = 1,
                RegisterAt = new DateTime(2026, 9, 21, 9, 0, 0, DateTimeKind.Utc),
            },
        ];
    }

    [Fact]
    public void Render_WithoutOffers_ShowsEmptyStateWithNewOfferLink()
    {
        GivenOffers();

        var cut = Render<OfferList>();

        Assert.NotNull(cut.Find($".empty a[href='{OfferRoutes.New}']"));
        Assert.Empty(cut.FindAll("table"));
    }

    [Fact]
    public void Render_WithOffers_ShowsOneTableRowPerOfferInServiceOrder()
    {
        GivenOffers(_offers);

        var cut = Render<OfferList>();

        var rows = cut.FindAll("tbody tr").Select(row => (
            row.QuerySelector(".offer-title")!.TextContent,
            row.QuerySelector(".offer-company")!.TextContent,
            row.QuerySelector(".num")!.TextContent,
            row.QuerySelector(".offer-date")!.TextContent));
        Assert.Equal(
            [
                (_offers[0].Title, _offers[0].CompanyName, "3", "28 Aug 2026"),
                (_offers[1].Title, _offers[1].CompanyName, "1", "21 Sep 2026"),
            ],
            rows);
        Assert.Empty(cut.FindAll(".empty"));
    }

    [Fact]
    public void Render_WithOffers_ShowsContractAndRemotePolicyTags()
    {
        GivenOffers(_offers);

        var cut = Render<OfferList>();

        var tags = cut.FindAll("tbody tr").Select(row => row.QuerySelectorAll(".tag").Select(tag => tag.TextContent.Trim()));
        Assert.Equal(
            [
                [_offers[0].ContractType.ToLabel(), _offers[0].RemotePolicy.ToLabel()],
                [_offers[1].ContractType.ToLabel(), _offers[1].RemotePolicy.ToLabel()],
            ],
            tags);
    }

    [Fact]
    public void Render_WithOffers_ShowsCardsWithCompanyDateAndPostings()
    {
        GivenOffers(_offers);

        var cut = Render<OfferList>();

        var cards = cut.FindAll(".list-card").Select(card => (
            card.QuerySelector(".list-card-title")!.TextContent,
            card.QuerySelector(".list-card-subtitle")!.TextContent,
            card.QuerySelector(".list-card-aside")!.TextContent,
            card.QuerySelector(".offer-postings .count-badge")!.TextContent,
            card.QuerySelector(".offer-postings")!.TextContent.Split(' ', StringSplitOptions.RemoveEmptyEntries).Last()));
        Assert.Equal(
            [
                (_offers[0].Title, _offers[0].CompanyName, "28 Aug", "3", "postings"),
                (_offers[1].Title, _offers[1].CompanyName, "21 Sep", "1", "posting"),
            ],
            cards);
    }

    [Fact]
    public void Render_WithOffers_LinksToNewOffer()
    {
        GivenOffers(_offers[0]);

        var cut = Render<OfferList>();

        Assert.NotNull(cut.Find($".page-header a[href='{OfferRoutes.New}']"));
    }

    [Fact]
    public void Render_WithOffers_LinksEachTableRowAndCardToTheOfferPage()
    {
        GivenOffers(_offers);

        var cut = Render<OfferList>();

        var expected = _offers.Select(offer => OfferRoutes.Details(offer.ShortId, offer.Slug));
        Assert.Equal(expected, cut.FindAll("tbody tr a.offer-title").Select(link => link.GetAttribute("href")));
        Assert.Equal(expected, cut.FindAll("a.list-card").Select(link => link.GetAttribute("href")));
    }

    private void GivenOffers(params JobOfferSummaryDto[] offers) =>
        _jobOfferService.ListAsync(Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(new JobOfferListDto(offers, HasArchived: false));
}
