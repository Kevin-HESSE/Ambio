using Ambio.Web.Components.Shared;

using Bunit;

namespace Ambio.Web.Tests.Components.Shared;

public class ListCardTests : BunitContext
{
    private const string Title = "Backend Developer";
    private const string Href = "offers/0192f1c4-7b3e-7d21-9a5c-3e8f4b2a6d10";
    private const string Subtitle = "Kelvio";
    private const string Aside = "3 Sep";
    private const string TagText = "Permanent";

    [Fact]
    public void Render_WithHref_LinksTheWholeCard()
    {
        var cut = Render<ListCard>(parameters => parameters
            .Add(p => p.Title, Title)
            .Add(p => p.Href, Href));

        Assert.Equal(Href, cut.Find(".list-card").GetAttribute("href"));
        Assert.Equal(Title, cut.Find(".list-card-title").TextContent);
        Assert.DoesNotContain("t-muted", cut.Find(".list-card-title").ClassList);
    }

    [Fact]
    public void Render_Archived_DimsTheTitle()
    {
        var cut = Render<ListCard>(parameters => parameters
            .Add(p => p.Title, Title)
            .Add(p => p.Archived, true));

        Assert.Contains("t-muted", cut.Find(".list-card-title").ClassList);
    }

    [Fact]
    public void Render_WithoutHref_IsAPlaceholderWithoutHref()
    {
        var cut = Render<ListCard>(parameters => parameters.Add(p => p.Title, Title));

        Assert.False(cut.Find(".list-card").HasAttribute("href"));
    }

    [Fact]
    public void Render_WithEveryPart_ShowsAsideSubtitleAndTags()
    {
        var cut = Render<ListCard>(parameters => parameters
            .Add(p => p.Title, Title)
            .Add(p => p.Aside, Aside)
            .Add(p => p.Subtitle, Subtitle)
            .Add(p => p.Tags, $"<span class=\"tag\">{TagText}</span>"));

        Assert.Equal(
            (Aside, Subtitle, TagText),
            (cut.Find(".list-card-aside").TextContent, cut.Find(".list-card-subtitle").TextContent, cut.Find(".list-card-tags .tag").TextContent));
    }

    [Fact]
    public void Render_WithTitleOnly_LeavesTheOtherPartsOut()
    {
        var cut = Render<ListCard>(parameters => parameters
            .Add(p => p.Title, Title)
            .Add(p => p.Subtitle, " "));

        Assert.Empty(cut.FindAll(".list-card-aside, .list-card-subtitle, .list-card-tags"));
    }
}
