using Ambio.Web.Components.Layout;

using Bunit;

namespace Ambio.Web.Tests.Components.Layout;

public class ThemeSelectorTests : BunitContext
{
    private const string SelectorName = "theme-sidebar";

    [Fact]
    public void Render_OffersSystemLightDarkToShellScript()
    {
        var cut = Render<ThemeSelector>(parameters => parameters.Add(selector => selector.Name, SelectorName));

        var group = cut.Find("[role='radiogroup']");
        Assert.True(group.HasAttribute("data-theme-selector"));
        Assert.Equal("Theme", group.GetAttribute("aria-label"));
        Assert.Equal(["system", "light", "dark"], cut.FindAll("input[type='radio']").Select(radio => radio.GetAttribute("value")));
        Assert.All(cut.FindAll("input[type='radio']"), radio => Assert.Equal(SelectorName, radio.GetAttribute("name")));
    }

    [Fact]
    public void Render_ChecksNothing()
    {
        var cut = Render<ThemeSelector>(parameters => parameters.Add(selector => selector.Name, SelectorName));

        Assert.Empty(cut.FindAll("input:checked"));
    }
}
