using Ambio.Application.Offers;
using Ambio.Application.Offers.Dtos;
using Ambio.Domain.Offers;
using Ambio.Web.Components.Offers;
using Ambio.Web.Components.Offers.Pages;

using Bunit;

using Microsoft.Extensions.DependencyInjection;

using NSubstitute;

namespace Ambio.Web.Tests.Components.Offers.Pages;

public class OfferListTests : BunitContext
{
    private readonly IJobOfferService _jobOfferService = Substitute.For<IJobOfferService>();

    public OfferListTests()
    {
        Services.AddSingleton(_jobOfferService);
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
        GivenOffers(FullStack, DataEngineer);

        var cut = Render<OfferList>();

        var rows = cut.FindAll("tbody tr").Select(row => (
            row.QuerySelector(".offer-title")!.TextContent,
            row.QuerySelector(".offer-company")!.TextContent,
            row.QuerySelector(".num")!.TextContent,
            row.QuerySelector(".offer-date")!.TextContent));
        Assert.Equal(
            [
                ("Full-stack Engineer", "Kelvio", "3", "28 Aug 2026"),
                ("Data Engineer", "Lumen Health", "1", "21 Sep 2026"),
            ],
            rows);
        Assert.Empty(cut.FindAll(".empty"));
    }

    [Fact]
    public void Render_WithOffers_ShowsContractAndRemotePolicyTags()
    {
        GivenOffers(FullStack, DataEngineer);

        var cut = Render<OfferList>();

        var tags = cut.FindAll("tbody tr").Select(row => row.QuerySelectorAll(".tag").Select(tag => tag.TextContent.Trim()));
        Assert.Equal([["Permanent", "Hybrid"], ["Fixed-term", "On-site"]], tags);
    }

    [Fact]
    public void Render_WithOffers_ShowsCardsWithCompanyDateAndPostings()
    {
        GivenOffers(FullStack, DataEngineer);

        var cut = Render<OfferList>();

        var cards = cut.FindAll(".offer-card").Select(card => (
            card.QuerySelector(".offer-title")!.TextContent,
            card.QuerySelector(".offer-company")!.TextContent,
            card.QuerySelector(".offer-date")!.TextContent,
            card.QuerySelector(".offer-postings")!.TextContent));
        Assert.Equal(
            [
                ("Full-stack Engineer", "Kelvio", "28 Aug", "3 postings"),
                ("Data Engineer", "Lumen Health", "21 Sep", "1 posting"),
            ],
            cards);
    }

    [Fact]
    public void Render_WithOffers_LinksToNewOffer()
    {
        GivenOffers(FullStack);

        var cut = Render<OfferList>();

        Assert.NotNull(cut.Find($".page-header a[href='{OfferRoutes.New}']"));
    }

    private void GivenOffers(params JobOfferSummaryDto[] offers) =>
        _jobOfferService.ListAsync(Arg.Any<CancellationToken>()).Returns(offers);

    private static JobOfferSummaryDto FullStack { get; } = new(
        Guid.CreateVersion7(), "Full-stack Engineer", "Kelvio", "kelvio", ContractType.Permanent, RemotePolicy.Hybrid, 3,
        new DateTime(2026, 8, 28, 9, 0, 0, DateTimeKind.Utc));

    private static JobOfferSummaryDto DataEngineer { get; } = new(
        Guid.CreateVersion7(), "Data Engineer", "Lumen Health", "lumen-health", ContractType.FixedTerm, RemotePolicy.OnSite, 1,
        new DateTime(2026, 9, 21, 9, 0, 0, DateTimeKind.Utc));
}
