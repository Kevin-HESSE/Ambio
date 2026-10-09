using Ambio.Web.Components.Shared;

using Bunit;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace Ambio.Web.Tests.Components.Shared;

public class FormFieldTests : BunitContext
{
    private const string Id = "company-name";
    private const string Label = "Name";
    private const string ErrorMessage = "Enter the name of the company.";

    private readonly Model _model = new();
    private readonly EditContext _editContext;

    public FormFieldTests()
    {
        _editContext = new EditContext(_model);
    }

    [Fact]
    public void Render_Default_LinksLabelToInputWithoutOptionalMarker()
    {
        var cut = RenderField();

        var label = cut.Find("label");
        Assert.Equal(Id, label.GetAttribute("for"));
        Assert.Equal(Label, label.TextContent.Trim());
        Assert.NotNull(cut.Find($"input#{Id}"));
        Assert.Equal("field", cut.Find("div").GetAttribute("class"));
    }

    [Fact]
    public void Render_Optional_ShowsOptionalMarker()
    {
        var cut = RenderField(parameters => parameters.Add(field => field.Optional, true));

        Assert.Equal($"{Label} (optional)", cut.Find("label").TextContent);
    }

    [Fact]
    public void Render_FullWidth_AddsFullWidthClass()
    {
        var cut = RenderField(parameters => parameters.Add(field => field.FullWidth, true));

        Assert.Equal("field full-width", cut.Find("div").GetAttribute("class"));
    }

    [Fact]
    public void Render_WithValidationMessage_ShowsItAfterInput()
    {
        var messages = new ValidationMessageStore(_editContext);
        messages.Add(() => _model.Name, ErrorMessage);

        var cut = RenderField();

        var message = cut.Find($"input#{Id} + .validation-message");
        Assert.Equal(ErrorMessage, message.TextContent);
    }

    private IRenderedComponent<CascadingValue<EditContext>> RenderField(
        Action<ComponentParameterCollectionBuilder<FormField<string>>>? parameters = null) =>
        Render<CascadingValue<EditContext>>(cascading => cascading
            .Add(value => value.Value, _editContext)
            .AddChildContent<FormField<string>>(field =>
            {
                field
                    .Add(f => f.Id, Id)
                    .Add(f => f.Label, Label)
                    .Add(f => f.For, () => _model.Name)
                    .AddChildContent($"<input id=\"{Id}\" />");
                parameters?.Invoke(field);
            }));

    private sealed class Model
    {
        public string Name { get; set; } = "";
    }
}
