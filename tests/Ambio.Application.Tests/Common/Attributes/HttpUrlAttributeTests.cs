using Ambio.Application.Common.Attributes;

namespace Ambio.Application.Tests.Common.Attributes;

public class HttpUrlAttributeTests
{
    private readonly HttpUrlAttribute _attribute = new();

    [Theory]
    [MemberData(nameof(ValidUrls))]
    public void IsValid_WithHttpOrHttpsUrl_ReturnsTrue(string url)
    {
        Assert.True(_attribute.IsValid(url));
    }

    [Theory]
    [MemberData(nameof(InvalidUrls))]
    public void IsValid_WithoutHttpScheme_ReturnsFalse(string url)
    {
        Assert.False(_attribute.IsValid(url));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void IsValid_WithNullOrEmpty_ReturnsTrue(string? url)
    {
        Assert.True(_attribute.IsValid(url));
    }

    [Fact]
    public void FormatErrorMessage_Default_SaysHowToFixTheValue()
    {
        Assert.Equal("Enter the full link, starting with https://", _attribute.FormatErrorMessage("Website"));
    }

    public static TheoryData<string> ValidUrls =>
    [
        "https://brightwave.io",
        "http://brightwave.io/careers",
        "https://www.linkedin.com/company/brightwave/",
        " https://brightwave.io ",
    ];

    public static TheoryData<string> InvalidUrls =>
    [
        "brightwave.io",
        "www.linkedin.com/company/brightwave",
        "ftp://brightwave.io",
        "mailto:jobs@brightwave.io",
        "https://",
        "not a link",
    ];
}
