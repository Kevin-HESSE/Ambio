using Ambio.Web.Components.Shared;

using Bunit;

namespace Ambio.Web.Tests.Components.Shared;

public class SheetTests : BunitContext
{
    private const string SheetId = "filters-sheet";
    private const string SheetLabel = "Filters";
    private const string Content = "Status";

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
}
