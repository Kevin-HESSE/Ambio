using Ambio.Web.Components.Shared;

namespace Ambio.Web.Tests.Components.Shared;

public class CssClassHelperTests
{
    private const string DefaultClass = "btn";
    private const string EnabledClass = "btn-primary";
    private const string DisabledClass = "btn-icon";
    private const string CallerClass = "w-100";

    [Fact]
    public void Serialize_WithConditionalClasses_KeepsOnlyEnabledOnesAfterDefaults()
    {
        var classes = CssClassHelper.Serialize([DefaultClass, "btn-lg"], ConditionalClasses());

        Assert.Equal($"{DefaultClass} btn-lg {EnabledClass}", classes);
    }

    [Fact]
    public void Serialize_WithoutDefaultsOrEnabledClasses_ReturnsEmpty()
    {
        var classes = CssClassHelper.Serialize([], new Dictionary<string, bool> { [DisabledClass] = false });

        Assert.Equal("", classes);
    }

    [Fact]
    public void Serialize_WithClassAttribute_AddsItBetweenDefaultsAndConditionalClasses()
    {
        var attributes = new Dictionary<string, object> { ["class"] = CallerClass };

        var classes = CssClassHelper.Serialize([DefaultClass], ConditionalClasses(), attributes);

        Assert.Equal($"{DefaultClass} {CallerClass} {EnabledClass}", classes);
    }

    [Theory]
    [MemberData(nameof(AttributesWithoutClass))]
    public void Serialize_WithoutUsableClassAttribute_AddsNothing(Dictionary<string, object> attributes)
    {
        var classes = CssClassHelper.Serialize([DefaultClass], ConditionalClasses(), attributes);

        Assert.Equal($"{DefaultClass} {EnabledClass}", classes);
    }

    private static Dictionary<string, bool> ConditionalClasses() => new()
    {
        [EnabledClass] = true,
        [DisabledClass] = false,
    };

    public static TheoryData<Dictionary<string, object>> AttributesWithoutClass =>
    [
        new Dictionary<string, object>(),
        new Dictionary<string, object> { ["aria-label"] = "New company" },
        new Dictionary<string, object> { ["class"] = "" },
        new Dictionary<string, object> { ["class"] = 42 },
    ];
}
