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
    [MemberData(nameof(UrlsWithoutDomainName))]
    public void IsValid_WithoutDomainName_ReturnsFalse(string url)
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
        Assert.Equal("Enter the full link, such as https://example.com", _attribute.FormatErrorMessage("Website"));
    }

    public static TheoryData<string> ValidUrls =>
    [
        "https://brightwave.io",
        "http://brightwave.io/careers",
        "https://www.linkedin.com/company/brightwave/",
        " https://brightwave.io ",
        "https://careers.kelvio.co.uk/jobs?id=42",
        "https://café.fr",
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

    public static TheoryData<string> UrlsWithoutDomainName =>
    [
        "https://acme",
        "https://acme.",
        "https://acme.c",
        "https://acme.123",
        "https://localhost:5160",
        "https://192.168.1.10",
    ];
}
