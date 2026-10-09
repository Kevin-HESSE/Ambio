using Ambio.Web.Components.Shared;

using Bunit;

namespace Ambio.Web.Tests.Components.Shared;

public class ButtonLinkTests : BunitContext
{
    private const string Href = "companies/new";
    private const string Label = "New company";

    [Fact]
    public void Render_WithIconAndLabel_RendersIconBeforeLabel()
    {
        var link = RenderLink(parameters => parameters
            .Add(button => button.Icon, "plus-lg")
            .AddChildContent(Label));

        Assert.Equal(Href, link.GetAttribute("href"));
        Assert.Equal("I", link.FirstElementChild?.TagName);
        Assert.Contains("bi-plus-lg", link.FirstElementChild!.ClassList);
        Assert.Equal(Label, link.TextContent.Trim());
    }

    [Theory]
    [MemberData(nameof(VariantClasses))]
    public void Render_WithVariant_AddsItsClassToBtn(ButtonVariant variant, string expectedClass)
    {
        var link = RenderLink(parameters => parameters.Add(button => button.Variant, variant));

        Assert.Equal(expectedClass, link.GetAttribute("class"));
    }

    [Fact]
    public void Render_IconOnly_AddsBtnIcon()
    {
        var link = RenderLink(parameters => parameters
            .Add(button => button.Variant, ButtonVariant.Primary)
            .Add(button => button.IconOnly, true));

        Assert.Equal("btn btn-primary btn-icon", link.GetAttribute("class"));
    }

    [Fact]
    public void Render_WithAdditionalAttributes_AddsClassAndPassesOtherAttributes()
    {
        var link = RenderLink(parameters => parameters
            .Add(button => button.Variant, ButtonVariant.Primary)
            .AddUnmatched("class", "w-100")
            .AddUnmatched("aria-label", Label));

        Assert.Equal("btn w-100 btn-primary", link.GetAttribute("class"));
        Assert.Equal(Label, link.GetAttribute("aria-label"));
    }

    private AngleSharp.Dom.IElement RenderLink(Action<ComponentParameterCollectionBuilder<ButtonLink>> parameters) =>
        Render<ButtonLink>(builder =>
        {
            builder.Add(button => button.Href, Href);
            parameters(builder);
        }).Find("a");

    public static TheoryData<ButtonVariant, string> VariantClasses => new()
    {
        { ButtonVariant.Default, "btn" },
        { ButtonVariant.Primary, "btn btn-primary" },
        { ButtonVariant.Secondary, "btn btn-secondary" },
        { ButtonVariant.Danger, "btn btn-danger" },
    };
}
