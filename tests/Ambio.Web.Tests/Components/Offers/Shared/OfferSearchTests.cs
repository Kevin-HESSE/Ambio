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
    public void Render_BeforeTheCircuit_DisablesTheField()
    {
        SetRendererInfo(new RendererInfo("Static", isInteractive: false));

        var cut = RenderSearch();

        Assert.True(cut.Find($"#{Id}").HasAttribute("disabled"));
    }

    [Fact]
    public void Input_SeveralQuickKeystrokes_SearchesOnceWithTheLastText()
    {
        var cut = RenderSearch();

        var field = cut.Find($"#{Id}");
        field.Input(Text[..1]);
        field.Input(Text[..2]);
        field.Input(Text);

        cut.WaitForAssertion(() => _jobOfferService.Received(1).SearchAsync(Text, Arg.Any<CancellationToken>()));
        _jobOfferService.Received(1).SearchAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
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

        cut.WaitForAssertion(() => Assert.Contains(Text, cut.Find(".offer-search-empty").TextContent));
    }

    [Fact]
    public async Task Input_BlankText_DoesNotSearch()
    {
        var cut = RenderSearch();

        cut.Find($"#{Id}").Input(" ");
        await Task.Delay(OfferSearch.SearchDelay * 2, Xunit.TestContext.Current.CancellationToken);

        await _jobOfferService.DidNotReceive().SearchAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        Assert.Empty(cut.FindAll(".offer-search-results"));
    }

    [Fact]
    public void Click_OnAResult_SelectsItsOffer()
    {
        JobOfferSummaryDto? selected = null;
        var cut = RenderSearch(offer => selected = offer);
        cut.Find($"#{Id}").Input(Text);
        cut.WaitForElement(".offer-search-result");

        cut.FindAll("button.offer-search-result")[1].Click();

        Assert.Equal(_offers[1], selected);
    }

    private IRenderedComponent<OfferSearch> RenderSearch(Action<JobOfferSummaryDto>? onSelect = null) =>
        Render<OfferSearch>(parameters => parameters
            .Add(p => p.Id, Id)
            .Add(p => p.OnSelect, onSelect ?? (_ => { })));
}
