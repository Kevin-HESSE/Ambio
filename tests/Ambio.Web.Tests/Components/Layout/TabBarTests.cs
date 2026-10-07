using Ambio.Web.Components.Layout;

using Bunit;
using Bunit.TestDoubles;

using Microsoft.Extensions.DependencyInjection;

namespace Ambio.Web.Tests.Components.Layout;

public class TabBarTests : BunitContext
{
    private const string AccountPath = "Account/Manage/Email";

    [Fact]
    public void Render_OnHome_MarksHomeActive()
    {
        var cut = Render<TabBar>();

        Assert.Contains("active", HomeTab(cut).ClassList);
        Assert.DoesNotContain("active", cut.Find("button.tab").ClassList);
    }

    [Fact]
    public void Render_OnAccountPage_MarksMoreActive()
    {
        Services.GetRequiredService<BunitNavigationManager>().NavigateTo(AccountPath);

        var cut = Render<TabBar>();

        Assert.Contains("active", cut.Find("button.tab").ClassList);
        Assert.DoesNotContain("active", HomeTab(cut).ClassList);
    }

    [Fact]
    public void Render_MoreTab_OpensMoreSheet()
    {
        var cut = Render<TabBar>();

        var more = cut.Find("button.tab");
        Assert.Equal(MoreSheet.Id, more.GetAttribute("data-sheet-open"));
        Assert.Equal(MoreSheet.Id, more.GetAttribute("aria-controls"));
        Assert.Equal("dialog", more.GetAttribute("aria-haspopup"));
    }

    private static AngleSharp.Dom.IElement HomeTab(IRenderedComponent<TabBar> cut) =>
        cut.FindAll("a.tab").Single(tab => tab.TextContent.Contains("Home"));
}
