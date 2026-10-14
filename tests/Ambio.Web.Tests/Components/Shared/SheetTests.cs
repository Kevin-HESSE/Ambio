using Ambio.Web.Components.Shared;

using Bunit;

namespace Ambio.Web.Tests.Components.Shared;

public class SheetTests : BunitContext
{
    private const string SheetId = "filters-sheet";
    private const string SheetLabel = "Filters";
    private const string Content = "Status";
    private const string SheetTitle = "Add a posting";

    [Fact]
    public void Render_IsNamedClosedDialogWithContent()
    {
        var cut = Render<Sheet>(parameters => parameters
            .Add(sheet => sheet.Id, SheetId)
            .Add(sheet => sheet.Label, SheetLabel)
            .AddChildContent($"<p>{Content}</p>"));

        var dialog = cut.Find("dialog.sheet");
        Assert.Equal(SheetId, dialog.Id);
        Assert.Equal(SheetLabel, dialog.GetAttribute("aria-label"));
        Assert.False(dialog.HasAttribute("open"));
        Assert.Equal(Content, dialog.QuerySelector("p")?.TextContent);
    }

    [Fact]
    public void Render_WithoutTitle_HasNoHead()
    {
        var cut = Render<Sheet>(parameters => parameters
            .Add(sheet => sheet.Id, SheetId)
            .Add(sheet => sheet.Label, SheetLabel));

        Assert.Empty(cut.FindAll(".sheet-head"));
    }

    [Fact]
    public void Render_WithTitle_IsNamedByItsHeadingWithCloseButton()
    {
        var cut = Render<Sheet>(parameters => parameters
            .Add(sheet => sheet.Id, SheetId)
            .Add(sheet => sheet.Title, SheetTitle));

        var dialog = cut.Find("dialog.sheet");
        var heading = cut.Find(".sheet-head h2");
        Assert.Equal(SheetTitle, heading.TextContent);
        Assert.Equal(heading.Id, dialog.GetAttribute("aria-labelledby"));
        Assert.False(dialog.HasAttribute("aria-label"));
        Assert.True(cut.Find(".sheet-head button[aria-label='Close']").HasAttribute("data-sheet-close"));
    }

    [Fact]
    public async Task CloseAsync_ClosesTheDialogThroughTheShell()
    {
        var close = JSInterop.SetupVoid("ambio.sheet.close", SheetId);
        var cut = Render<Sheet>(parameters => parameters
            .Add(sheet => sheet.Id, SheetId)
            .Add(sheet => sheet.Title, SheetTitle));

        await cut.InvokeAsync(() => cut.Instance.CloseAsync());

        close.VerifyInvoke("ambio.sheet.close");
    }
}
