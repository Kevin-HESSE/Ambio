using Ambio.Web.Components.Layout;

using Bunit;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace Ambio.Web.Tests.Components.Layout;

public class AppBarTests : BunitContext
{
    private const string PageTitle = "Dashboard";

    public AppBarTests()
    {
        AddAuthorization();
        ComponentFactories.AddStub<AntiforgeryToken>();
    }

    [Fact]
    public void Render_WhenPageSetsTitle_ShowsItInAppBar()
    {
        RenderFragment page = builder =>
        {
            builder.OpenComponent<AppBarTitle>(0);
            builder.AddComponentParameter(1, nameof(AppBarTitle.ChildContent), (RenderFragment)(title => title.AddContent(0, PageTitle)));
            builder.CloseComponent();
        };

        var cut = Render<MainLayout>(parameters => parameters.Add(layout => layout.Body, page));

        Assert.Equal(PageTitle, cut.Find(".appbar-title").TextContent);
    }

    [Fact]
    public void Render_WhenPageSetsNoTitle_LeavesTitleEmpty()
    {
        var cut = Render<MainLayout>(parameters => parameters.Add(layout => layout.Body, (RenderFragment)(_ => { })));

        Assert.Empty(cut.Find(".appbar-title").TextContent);
    }
}
