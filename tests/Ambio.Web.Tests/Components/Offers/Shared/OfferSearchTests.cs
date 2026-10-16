using Ambio.Application.Offers;
using Ambio.Application.Offers.Dtos;
using Ambio.Web.Components.Offers;
using Ambio.Web.Components.Offers.Shared;
using Ambio.Web.Tests.Fixtures;

using Bunit;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

using NSubstitute;

namespace Ambio.Web.Tests.Components.Offers.Shared;

public class OfferSearchTests : BunitContext
{
    private const string Id = "application-offer";
    private const string Text = "data";

    private readonly IJobOfferService _jobOfferService = Substitute.For<IJobOfferService>();
    private readonly JobOfferSummaryDto[] _offers;

    public OfferSearchTests()
    {
        Services.AddSingleton(_jobOfferService);
        SetRendererInfo(new RendererInfo("Server", isInteractive: true));
        _offers = [.. JobOfferSummaryFactory.GetGenerator().Generate(2)];
        _jobOfferService.SearchAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(_offers);
    }

    [Fact]
    public void Input_Text_ShowsTheMatchingOffersAndTheNewOfferLink()
    {
        var cut = RenderSearch();

        cut.Find($"#{Id}").Input(Text);

        cut.WaitForAssertion(() => Assert.Equal(
            _offers.Select(offer => offer.Title),
            cut.FindAll(".offer-search-title").Select(title => title.TextContent)));
        Assert.Equal(OfferRoutes.New, cut.Find("a.offer-search-new").GetAttribute("href"));
    }

    [Fact]
    public void Input_TextWithoutMatch_SaysSo()
    {
        _jobOfferService.SearchAsync(Text, Arg.Any<CancellationToken>()).Returns([]);
        var cut = RenderSearch();

        cut.Find($"#{Id}").Input(Text);

        cut.WaitForAssertion(() => Assert.Contains(Text, cut.Find(".search-field-empty").TextContent));
    }

    [Fact]
    public void Click_OnAResult_SelectsItsOffer()
    {
        JobOfferSummaryDto? selected = null;
        var cut = RenderSearch(offer => selected = offer);
        cut.Find($"#{Id}").Input(Text);
        cut.WaitForElement("button.search-field-result");

        cut.FindAll("button.search-field-result")[1].Click();

        // The pick goes through SearchField first.
        cut.WaitForAssertion(() => Assert.Equal(_offers[1], selected));
    }

    private IRenderedComponent<OfferSearch> RenderSearch(Action<JobOfferSummaryDto>? onSelect = null) =>
        Render<OfferSearch>(parameters => parameters
            .Add(p => p.Id, Id)
            .Add(p => p.OnSelect, onSelect ?? (_ => { })));
}
