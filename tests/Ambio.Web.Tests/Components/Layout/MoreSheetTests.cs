using Ambio.Web.Components.Layout;

using Bunit;
using Bunit.TestDoubles;

using Microsoft.AspNetCore.Components.Forms;

namespace Ambio.Web.Tests.Components.Layout;

public class MoreSheetTests : BunitContext
{
    private const string UserEmail = "jane.doe@example.com";
    private const string AccountPath = "Account/Manage";
    private const string LogoutPath = "Account/Logout";
    private const string LoginPath = "Account/Login";

    private readonly BunitAuthorizationContext _authorization;

    public MoreSheetTests()
    {
        _authorization = AddAuthorization();
        ComponentFactories.AddStub<AntiforgeryToken>();
    }

    [Fact]
    public void Render_HasSheetId()
    {
        var cut = Render<MoreSheet>();

        Assert.NotNull(cut.Find($"dialog#{MoreSheet.Id}"));
    }

    [Fact]
    public void Render_WhenSignedIn_ShowsAccountAndLogOut()
    {
        _authorization.SetAuthorized(UserEmail);

        var cut = Render<MoreSheet>();

        Assert.Contains("Account", cut.Find($"a[href='{AccountPath}']").TextContent);
        Assert.Equal(LogoutPath, cut.Find("form").GetAttribute("action"));
        Assert.Contains("Log out", cut.Find("button[type='submit']").TextContent);
        Assert.Empty(cut.FindAll($"a[href='{LoginPath}']"));
    }

    [Fact]
    public void Render_WhenSignedOut_ShowsLogIn()
    {
        var cut = Render<MoreSheet>();

        Assert.Contains("Log in", cut.Find($"a[href='{LoginPath}']").TextContent);
        Assert.Empty(cut.FindAll("form"));
        Assert.Empty(cut.FindAll($"a[href='{AccountPath}']"));
    }
}
