using Ambio.Web.Components.Shared;

using Bunit;

namespace Ambio.Web.Tests.Components.Shared;

public class SegmentedControlTests : BunitContext
{
    private const string GroupName = "kind";
    private const string GroupLabel = "Kind";

    [Fact]
    public void Render_ShowsOneLabelledRadioPerOptionInOneGroup()
    {
        var cut = RenderControl();

        Assert.Equal(GroupLabel, cut.Find("[role='radiogroup']").GetAttribute("aria-label"));
        var radios = cut.FindAll("input[type='radio']");
        Assert.Equal(Options.Select(option => option.Value), radios.Select(radio => radio.GetAttribute("value")));
        Assert.All(radios, radio => Assert.Equal(GroupName, radio.GetAttribute("name")));
        Assert.All(Options, option =>
            Assert.Equal(option.Label, cut.Find($"label[for='{GroupName}-{option.Value}']").TextContent.Trim()));
    }

    [Fact]
    public void Render_ChecksValueOnly()
    {
        var cut = RenderControl(value: "spontaneous");

        var checkedRadio = Assert.Single(cut.FindAll("input:checked"));
        Assert.Equal("spontaneous", checkedRadio.GetAttribute("value"));
    }

    [Fact]
    public void Render_WithoutValue_ChecksNothing()
    {
        var cut = RenderControl();

        Assert.Empty(cut.FindAll("input:checked"));
    }

    [Fact]
    public void Render_IconsOnly_KeepsLabelsAsAccessibleNamesAndTooltips()
    {
        var cut = RenderControl(iconsOnly: true);

        Assert.All(Options, option =>
        {
            var label = cut.Find($"label[for='{GroupName}-{option.Value}']");
            Assert.Equal(option.Label, label.GetAttribute("title"));
            Assert.Contains("visually-hidden", label.QuerySelector("span")!.ClassList);
        });
    }

    [Fact]
    public void Change_OnAnOption_GivesItsValue()
    {
        string? changed = null;
        var cut = RenderControl(value: "offer", valueChanged: value => changed = value);

        cut.Find($"#{GroupName}-spontaneous").Change(true);

        Assert.Equal("spontaneous", changed);
    }

    [Fact]
    public void Render_Disabled_DisablesEveryOption()
    {
        var cut = RenderControl(disabled: true);

        Assert.All(cut.FindAll("input[type='radio']"), radio => Assert.True(radio.HasAttribute("disabled")));
    }

    private IRenderedComponent<SegmentedControl> RenderControl(
        string? value = null, bool iconsOnly = false, bool disabled = false, Action<string>? valueChanged = null) =>
        Render<SegmentedControl>(parameters => parameters
            .Add(control => control.Name, GroupName)
            .Add(control => control.Label, GroupLabel)
            .Add(control => control.Options, Options)
            .Add(control => control.Value, value)
            .Add(control => control.IconsOnly, iconsOnly)
            .Add(control => control.Disabled, disabled)
            .Add(control => control.ValueChanged, valueChanged ?? (_ => { })));

    private static readonly IReadOnlyList<SegmentedOption> Options =
    [
        new("offer", "From an offer", "briefcase"),
        new("spontaneous", "Spontaneous", "send"),
    ];
}
