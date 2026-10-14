using Ambio.Web.Components.Shared;

using Bunit;

namespace Ambio.Web.Tests.Components.Shared;

public class BannerTests : BunitContext
{
    private const string Icon = "info-circle";
    private const string Title = "This offer may already exist";
    private const string Text = "If it's the same job, add this link to it.";
    private const string Action = "Create a new offer anyway";
    private const string CallerClass = "form-page";

    [Fact]
    public void Render_WithTitleAndText_ShowsThemNextToTheIcon()
    {
        var cut = Render<Banner>(parameters => parameters
            .Add(banner => banner.Icon, Icon)
            .Add(banner => banner.Title, Title)
            .AddChildContent(Text));

        Assert.Equal("status", cut.Find(".banner").GetAttribute("role"));
        Assert.Equal("true", cut.Find($".banner-icon.bi-{Icon}").GetAttribute("aria-hidden"));
        Assert.Equal(Title, cut.Find(".banner-title").TextContent);
        Assert.Equal(Text, cut.Find(".banner-text").TextContent);
        Assert.Empty(cut.FindAll(".banner-actions"));
    }

    [Fact]
    public void Render_WithActions_ShowsThemUnderTheText()
    {
        var cut = Render<Banner>(parameters => parameters
            .Add(banner => banner.Icon, Icon)
            .Add(banner => banner.Title, Title)
            .Add(banner => banner.Actions, $"<button type=\"button\">{Action}</button>"));

        Assert.Equal(Action, cut.Find(".banner-actions button").TextContent);
    }

    [Theory]
    [InlineData(BannerVariant.Neutral, "banner")]
    [InlineData(BannerVariant.Info, "banner banner-info")]
    public void Render_WithVariant_SetsItsClass(BannerVariant variant, string expectedClass)
    {
        var cut = Render<Banner>(parameters => parameters
            .Add(banner => banner.Icon, Icon)
            .Add(banner => banner.Title, Title)
            .Add(banner => banner.Variant, variant));

        Assert.Equal(expectedClass, cut.Find("div").GetAttribute("class"));
    }

    [Fact]
    public void Render_WithClassAndAttributes_AddsThemToTheBanner()
    {
        var cut = Render<Banner>(parameters => parameters
            .Add(banner => banner.Icon, Icon)
            .Add(banner => banner.Title, Title)
            .Add(banner => banner.Variant, BannerVariant.Info)
            .AddUnmatched("class", CallerClass)
            .AddUnmatched("id", "duplicates"));

        var banner = cut.Find("div");
        Assert.Equal($"banner {CallerClass} banner-info", banner.GetAttribute("class"));
        Assert.Equal("duplicates", banner.GetAttribute("id"));
    }
}
