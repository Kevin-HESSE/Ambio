using Ambio.Web.Components.Layout;

using Bunit;

using Microsoft.Extensions.DependencyInjection;

namespace Ambio.Web.Tests.Components.Layout;

public class ToastHostTests : BunitContext
{
    private const string Message = "Application archived";
    private const string FormId = "restore-form";

    private readonly ToastService _toasts = new();

    public ToastHostTests()
    {
        Services.AddSingleton(_toasts);
    }

    [Fact]
    public void Render_WithoutToast_LeavesTheStatusRegionEmpty()
    {
        var cut = Render<ToastHost>();

        Assert.Empty(cut.Find(".toast-host[role='status']").Children);
    }

    [Fact]
    public void Show_AfterRender_ShowsTheMessageAndAnActionSendingItsForm()
    {
        var cut = Render<ToastHost>();

        _toasts.Show(new Toast(Message, Icon: "archive", new ToastAction("Undo", FormId)));

        var action = cut.Find(".toast .toast-action");
        Assert.Equal(Message, cut.Find(".toast-message").TextContent);
        Assert.Equal(("submit", FormId, "Undo"), (action.GetAttribute("type"), action.GetAttribute("form"), action.TextContent));
    }

    [Fact]
    public void Show_WithoutAction_ShowsTheMessageAlone()
    {
        var cut = Render<ToastHost>();

        _toasts.Show(new Toast(Message));

        Assert.Equal(Message, cut.Find(".toast-message").TextContent);
        Assert.Empty(cut.FindAll(".toast-action, .toast .bi"));
    }
}
