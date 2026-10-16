using Ambio.Web.Components.Shared;

using Bunit;

using Microsoft.AspNetCore.Components;

using NSubstitute;

namespace Ambio.Web.Tests.Components.Shared;

public class SearchFieldTests : BunitContext
{
    private const string Id = "search";
    private const string Text = "data";

    private static readonly string[] Results = ["Data Engineer", "Data Analyst"];

    private readonly Func<string, CancellationToken, Task<IReadOnlyList<string>>> _search =
        Substitute.For<Func<string, CancellationToken, Task<IReadOnlyList<string>>>>();

    public SearchFieldTests()
    {
        SetRendererInfo(new RendererInfo("Server", isInteractive: true));
        _search(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(Results);
    }

    [Fact]
    public void Render_BeforeTheCircuit_DisablesTheField()
    {
        SetRendererInfo(new RendererInfo("Static", isInteractive: false));

        var cut = RenderField();

        Assert.True(cut.Find($"#{Id}").HasAttribute("disabled"));
    }

    [Fact]
    public void Input_SeveralQuickKeystrokes_SearchesOnceWithTheLastText()
    {
        var cut = RenderField();

        var field = cut.Find($"#{Id}");
        field.Input(Text[..1]);
        field.Input(Text[..2]);
        field.Input(Text);

        cut.WaitForAssertion(() => _search.Received(1)(Text, Arg.Any<CancellationToken>()));
        _search.Received(1)(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public void Input_Text_ShowsAButtonPerResultThenTheFooter()
    {
        var cut = RenderField();

        cut.Find($"#{Id}").Input(Text);

        cut.WaitForAssertion(() => Assert.Equal(
            Results,
            cut.FindAll("button.search-field-result").Select(button => button.TextContent.Trim())));
        Assert.Equal($"Footer {Text}", cut.Find(".search-field-results li:last-child").TextContent);
    }

    [Fact]
    public void Input_TextWithoutMatch_ShowsTheEmptyContent()
    {
        _search(Text, Arg.Any<CancellationToken>()).Returns([]);
        var cut = RenderField();

        cut.Find($"#{Id}").Input(Text);

        cut.WaitForAssertion(() => Assert.Equal($"Empty {Text}", cut.Find(".search-field-empty").TextContent));
    }

    [Fact]
    public async Task Input_BlankText_DoesNotSearch()
    {
        var cut = RenderField();

        cut.Find($"#{Id}").Input(" ");
        await Task.Delay(SearchField<string>.SearchDelay * 2, Xunit.TestContext.Current.CancellationToken);

        await _search.DidNotReceive()(Arg.Any<string>(), Arg.Any<CancellationToken>());
        Assert.Empty(cut.FindAll(".search-field-results"));
    }

    [Fact]
    public void Click_OnAResult_SelectsIt()
    {
        string? selected = null;
        var cut = RenderField(result => selected = result);
        cut.Find($"#{Id}").Input(Text);
        cut.WaitForElement("button.search-field-result");

        cut.FindAll("button.search-field-result")[1].Click();

        Assert.Equal(Results[1], selected);
    }

    private IRenderedComponent<SearchField<string>> RenderField(Action<string>? onSelect = null) =>
        Render<SearchField<string>>(parameters => parameters
            .Add(p => p.Id, Id)
            .Add(p => p.ResultsLabel, "Results")
            .Add(p => p.Search, _search)
            .Add(p => p.ResultContent, result => result)
            .Add(p => p.EmptyContent, text => $"Empty {text}")
            .Add(p => p.Footer, text => $"Footer {text}")
            .Add(p => p.OnSelect, onSelect ?? (_ => { })));
}
