using Ambio.Web.Components.Shared;

using Bunit;

using Microsoft.AspNetCore.Components;

namespace Ambio.Web.Tests.Components.Shared;

public class DetailHeaderTests : BunitContext
{
    private const string Title = "Backend Developer";
    private const string ParentTitle = "Offers";
    private const string ParentHref = "offers";
    private const string MenuItem = "Archive";

    [Fact]
    public void Render_WithMenu_PutsItsItemsInThePopoverAndInTheSheetOfTheAppBarButton()
    {
        var cut = RenderHeader(menu: builder => builder.AddMarkupContent(0, $"<li><button class=\"menu-item\">{MenuItem}</button></li>"));

        var popover = cut.Find(".detail-actions [popover]");
        Assert.Equal(popover.Id, cut.Find(".detail-actions button[popovertarget]").GetAttribute("popovertarget"));
        Assert.Equal(MenuItem, popover.QuerySelector(".menu-list .menu-item")!.TextContent);
        Assert.Equal(MenuItem, cut.Find($"dialog#{DetailMenuButton.SheetId} .menu-list .menu-item").TextContent);
    }

    [Fact]
    public void Render_WithoutMenu_HasNeitherPopoverNorSheet()
    {
        var cut = RenderHeader(menu: null);

        Assert.Empty(cut.FindAll("[popover], dialog"));
    }

    private IRenderedComponent<DetailHeader> RenderHeader(RenderFragment? menu) =>
        Render<DetailHeader>(parameters => parameters
            .Add(p => p.Title, Title)
            .Add(p => p.ParentTitle, ParentTitle)
            .Add(p => p.ParentHref, ParentHref)
            .Add(p => p.Menu, menu));
}
