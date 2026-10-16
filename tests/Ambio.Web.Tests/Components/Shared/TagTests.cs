using Ambio.Web.Components.Shared;

using Bunit;

namespace Ambio.Web.Tests.Components.Shared;

public class TagTests : BunitContext
{
    private const string Label = "Email";
    private const string Icon = "envelope";

    [Fact]
    public void Render_ShowsTheIconAndTheLabelInANeutralTag()
    {
        var cut = RenderTag(TagVariant.Neutral);

        var tag = cut.Find(".tag");
        Assert.Equal(Label, tag.TextContent.Trim());
        Assert.Contains($"bi-{Icon}", tag.QuerySelector("i")!.ClassList);
        Assert.DoesNotContain("tag-accent", tag.ClassList);
    }

    [Fact]
    public void Render_Accent_AddsTheAccentClass()
    {
        var cut = RenderTag(TagVariant.Accent);

        Assert.Contains("tag-accent", cut.Find(".tag").ClassList);
    }

    private IRenderedComponent<Tag> RenderTag(TagVariant variant) =>
        Render<Tag>(parameters => parameters
            .Add(p => p.Icon, Icon)
            .Add(p => p.Variant, variant)
            .AddChildContent(Label));
}
