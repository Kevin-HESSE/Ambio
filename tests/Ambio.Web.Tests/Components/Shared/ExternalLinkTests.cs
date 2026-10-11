using Ambio.Web.Components.Shared;

using Bunit;

namespace Ambio.Web.Tests.Components.Shared;

public class ExternalLinkTests : BunitContext
{
    private const string Url = "https://www.linkedin.com/company/brightwave/";

    [Theory]
    [MemberData(nameof(ShortTexts))]
    public void Render_WithUrl_ShowsItWithoutSchemeAndWww(string url, string text)
    {
        var cut = Render<ExternalLink>(parameters => parameters.Add(link => link.Href, url));

        Assert.Equal(text, cut.Find(".external-link-text").TextContent);
    }

    [Fact]
    public void Render_WithUrl_OpensItInNewTab()
    {
        var cut = Render<ExternalLink>(parameters => parameters.Add(link => link.Href, Url));

        var link = cut.Find("a");
        Assert.Equal(Url, link.GetAttribute("href"));
        Assert.Equal("_blank", link.GetAttribute("target"));
        Assert.Equal("noopener noreferrer", link.GetAttribute("rel"));
    }

    public static TheoryData<string, string> ShortTexts => new()
    {
        { "https://brightwave.io", "brightwave.io" },
        { "http://brightwave.io/", "brightwave.io" },
        { Url, "linkedin.com/company/brightwave" },
        { "https://careers.kelvio.com/jobs/42?ref=linkedin#apply", "careers.kelvio.com/jobs/42" },
    };
}
