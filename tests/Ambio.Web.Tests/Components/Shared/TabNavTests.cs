using Ambio.Web.Components.Shared;

using Bunit;

namespace Ambio.Web.Tests.Components.Shared;

public class TabNavTests : BunitContext
{
    private const string Label = "Company sections";
    private const string OffersHref = "companies/kelvio?tab=offers";
    private const string ApplicationsHref = "companies/kelvio?tab=applications";

    private static readonly TabNavItem[] Tabs =
    [
        new("offers", "Offers", OffersHref, 2),
        new("applications", "Applications", ApplicationsHref),
    ];

    [Fact]
    public void Render_Default_LinksEveryTabInOrderInsideNamedNavigation()
    {
        var cut = RenderTabs("offers");

        Assert.Equal(Label, cut.Find("nav.tabs").GetAttribute("aria-label"));
        Assert.Equal([OffersHref, ApplicationsHref], cut.FindAll("a.tab").Select(tab => tab.GetAttribute("href")));
    }

    [Fact]
    public void Render_WithActiveTab_MarksOnlyItAsCurrentPage()
    {
        var cut = RenderTabs("applications");

        var active = cut.FindAll("a.tab.active");
        Assert.Equal(ApplicationsHref, Assert.Single(active).GetAttribute("href"));
        Assert.Equal("page", active[0].GetAttribute("aria-current"));
        Assert.Null(cut.Find($"a[href='{OffersHref}']").GetAttribute("aria-current"));
    }

    [Fact]
    public void Render_WithCount_ShowsItOnlyOnTheTabsThatHaveOne()
    {
        var cut = RenderTabs("offers");

        Assert.Equal("2", cut.Find($"a[href='{OffersHref}'] .tab-count").TextContent);
        Assert.Empty(cut.Find($"a[href='{ApplicationsHref}']").QuerySelectorAll(".tab-count"));
    }

    private IRenderedComponent<TabNav> RenderTabs(string active) =>
        Render<TabNav>(parameters => parameters
            .Add(nav => nav.Label, Label)
            .Add(nav => nav.Tabs, Tabs)
            .Add(nav => nav.Active, active));
}
