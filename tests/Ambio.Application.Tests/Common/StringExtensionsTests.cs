using Ambio.Application.Common;

namespace Ambio.Application.Tests.Common;

public class StringExtensionsTests
{
    [Fact]
    public void NullIfBlank_WithNull_ReturnsNull()
    {
        Assert.Null(((string?)null).NullIfBlank());
    }

    [Theory]
    [MemberData(nameof(BlankValues))]
    public void NullIfBlank_WithBlankValue_ReturnsNull(string value)
    {
        Assert.Null(value.NullIfBlank());
    }

    [Fact]
    public void NullIfBlank_WithText_ReturnsItTrimmed()
    {
        Assert.Equal("Lyon", " Lyon\t".NullIfBlank());
    }

    public static TheoryData<string> BlankValues =>
    [
        "",
        " ",
        "\t\n",
    ];
}
