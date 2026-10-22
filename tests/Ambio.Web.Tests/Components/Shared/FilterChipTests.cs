using Ambio.Web.Components.Shared;

using Bunit;

namespace Ambio.Web.Tests.Components.Shared;

public class FilterChipTests : BunitContext
{
    private const string Href = "applications?status=applied";
    private const string Label = "Applied";
    private const int Count = 9;
    private const string Icon = "send";
    private const string CallerClass = "status-applied";

    [Fact]
    public void Render_WithIcon_LinksToItsListWithTheIconLabelAndCount()
    {
        var cut = Render<FilterChip>(parameters => parameters
            .Add(chip => chip.Href, Href)
            .Add(chip => chip.Label, Label)
            .Add(chip => chip.Count, Count)
            .Add(chip => chip.Icon, Icon));

        var chip = cut.Find("a.filter-chip");
        Assert.Equal(Href, chip.GetAttribute("href"));
        Assert.NotNull(chip.QuerySelector($".bi-{Icon}"));
        Assert.Equal(
            (Label, Count.ToString()),
            (chip.QuerySelector(".filter-chip-label")!.TextContent, chip.QuerySelector(".filter-chip-count")!.TextContent));
    }

    [Fact]
    public void Render_WithoutIcon_ShowsNone()
    {
        var cut = Render<FilterChip>(parameters => parameters
            .Add(chip => chip.Href, Href)
            .Add(chip => chip.Label, Label)
            .Add(chip => chip.Count, Count));

        Assert.Empty(cut.FindAll(".bi"));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Render_ActiveOrNot_MarksOnlyTheActiveChipAsCurrent(bool active)
    {
        var cut = Render<FilterChip>(parameters => parameters
            .Add(chip => chip.Href, Href)
            .Add(chip => chip.Label, Label)
            .Add(chip => chip.Count, Count)
            .Add(chip => chip.Active, active));

        Assert.Equal(active ? "true" : null, cut.Find("a").GetAttribute("aria-current"));
    }

    [Fact]
    public void Render_WithClass_AddsItToTheChip()
    {
        var cut = Render<FilterChip>(parameters => parameters
            .Add(chip => chip.Href, Href)
            .Add(chip => chip.Label, Label)
            .Add(chip => chip.Count, Count)
            .AddUnmatched("class", CallerClass));

        Assert.Equal($"filter-chip t-small text-nowrap {CallerClass}", cut.Find("a").GetAttribute("class"));
    }
}
