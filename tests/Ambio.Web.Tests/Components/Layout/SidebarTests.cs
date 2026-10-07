using Ambio.Web.Components.Layout;

using Bunit;
using Bunit.TestDoubles;

using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.DependencyInjection;

namespace Ambio.Web.Tests.Components.Layout;

public class SidebarTests : BunitContext
{
    private const string UserEmail = "jane.doe@example.com";
    private const string CurrentPath = "Account/Manage/Email";
    private const string AccountPath = "Account/Manage";
    private const string LogoutPath = "Account/Logout";
    private const string LoginPath = "Account/Login";

    private readonly BunitAuthorizationContext _authorization;

    public SidebarTests()
    {
        _authorization = AddAuthorization();
        ComponentFactories.AddStub<AntiforgeryToken>();
    }

    [Fact]
    public void Render_OnHome_MarksDashboardActive()
    {
        var cut = Render<Sidebar>();

        var dashboard = cut.FindAll("a.nav-item").Single(link => link.TextContent.Contains("Dashboard"));
        Assert.Contains("active", dashboard.ClassList);
    }

    [Fact]
    public void Render_WhenSignedIn_ShowsAccountAndLogOutToCurrentPage()
    {
        _authorization.SetAuthorized(UserEmail);
        Services.GetRequiredService<BunitNavigationManager>().NavigateTo(CurrentPath);

        var cut = Render<Sidebar>();

        Assert.Equal(UserEmail, cut.Find($"a[href='{AccountPath}']").TextContent);
        Assert.Equal(LogoutPath, cut.Find("form").GetAttribute("action"));
        Assert.Equal(CurrentPath, cut.Find("input[name='ReturnUrl']").GetAttribute("value"));
        Assert.NotNull(cut.Find("button[type='submit'][aria-label='Log out']"));
        Assert.Empty(cut.FindAll($"a[href='{LoginPath}']"));
    }

    [Theory]
    [InlineData("jane.doe@example.com", "JD")]
    [InlineData("user@example.com", "U")]
    public void Render_WhenSignedIn_ShowsInitials(string email, string initials)
    {
        _authorization.SetAuthorized(email);

        var cut = Render<Sidebar>();

        Assert.Equal(initials, cut.Find(".avatar").TextContent);
    }

    [Fact]
    public void Render_WhenSignedOut_ShowsLogIn()
    {
        var cut = Render<Sidebar>();

        Assert.NotNull(cut.Find($"a[href='{LoginPath}']"));
        Assert.Empty(cut.FindAll("form"));
        Assert.Empty(cut.FindAll($"a[href='{AccountPath}']"));
    }
}
