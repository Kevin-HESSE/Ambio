using Ambio.Web.Components.Shared;

using Bunit;

namespace Ambio.Web.Tests.Components.Shared;

public class ShowArchivedTests : BunitContext
{
    private const string Id = "filter-archived";

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Render_SendsTrueUnderItsQueryAndShowsWhetherItIsChecked(bool isChecked)
    {
        var cut = Render<ShowArchived>(parameters => parameters
            .Add(p => p.Id, Id)
            .Add(p => p.Checked, isChecked));

        var checkbox = cut.Find($"input#{Id}");
        Assert.Equal(
            ("checkbox", ShowArchived.Query, "true", isChecked),
            (checkbox.GetAttribute("type"), checkbox.GetAttribute("name"), checkbox.GetAttribute("value"), checkbox.HasAttribute("checked")));
        Assert.Equal(Id, cut.Find("label").GetAttribute("for"));
    }

    [Theory]
    [InlineData("true")]
    [InlineData("TRUE")]
    public void IsChecked_True_ReturnsTrue(string value)
    {
        Assert.True(ShowArchived.IsChecked(value));
    }

    [Theory]
    [MemberData(nameof(OtherValues))]
    public void IsChecked_AnyOtherValue_ReturnsFalse(string value)
    {
        Assert.False(ShowArchived.IsChecked(value));
    }

    [Fact]
    public void IsChecked_Null_ReturnsFalse()
    {
        Assert.False(ShowArchived.IsChecked(null));
    }

    public static TheoryData<string> OtherValues =>
    [
        "",
        "false",
        "1",
        "on",
    ];
}
