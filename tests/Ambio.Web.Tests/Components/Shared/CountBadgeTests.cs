using Ambio.Web.Components.Shared;

using Bunit;

namespace Ambio.Web.Tests.Components.Shared;

public class CountBadgeTests : BunitContext
{
    private const int Count = 3;
    private const string CallerClass = "tab-count";

    [Fact]
    public void Render_WithCount_ShowsItInABadge()
    {
        var cut = Render<CountBadge>(parameters => parameters.Add(badge => badge.Count, Count));

        var badge = cut.Find("span.count-badge.t-badge");
        Assert.Equal(Count.ToString(), badge.TextContent);
    }

    [Fact]
    public void Render_WithClassAndAttributes_AddsThemToTheBadge()
    {
        var cut = Render<CountBadge>(parameters => parameters
            .Add(badge => badge.Count, Count)
            .AddUnmatched("class", CallerClass)
            .AddUnmatched("aria-hidden", "true"));

        var badge = cut.Find("span");
        Assert.Equal($"count-badge t-badge {CallerClass}", badge.GetAttribute("class"));
        Assert.Equal("true", badge.GetAttribute("aria-hidden"));
    }
}
