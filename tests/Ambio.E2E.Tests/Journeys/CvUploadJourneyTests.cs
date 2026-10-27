using System.Text;
using System.Text.RegularExpressions;

using Ambio.Application.CvDocuments;
using Ambio.E2E.Tests.Fixtures;

using Microsoft.Playwright;

using static Microsoft.Playwright.Assertions;

namespace Ambio.E2E.Tests.Journeys;

[Trait("Category", "E2E")]
public class CvUploadJourneyTests : IClassFixture<AmbioAppFixture>
{
    private readonly AmbioAppFixture _app;

    public CvUploadJourneyTests(AmbioAppFixture app)
    {
        _app = app;
    }

    private const string NewCompanyPath = "companies/new";
    private const string NewOfferPath = "offers/new";
    private const string CompanyName = "Kelvio";
    private const string Title = "Platform Engineer";
    private const string Site = "LinkedIn";
    private const string Url = "https://www.linkedin.com/jobs/view/4211";
    private const string CvFileName = "CV_Platform_2026-10.pdf";
    private const string NotPdfFileName = "CV_in_text.pdf";

    private static readonly byte[] Pdf = Encoding.ASCII.GetBytes("%PDF-1.4\n1 0 obj << /Type /Catalog >> endobj\n%%EOF\n");
    private static readonly byte[] NotPdf = Encoding.ASCII.GetBytes("Jane Doe, Platform Engineer");

    [Fact]
    public async Task User_UploadsTheCvSentThenPreviewsAndDownloadsIt()
    {
        var page = await _app.NewLoggedInPageAsync();

        await page.GotoAsync(NewCompanyPath);
        await page.GetByLabel("Name", new() { Exact = true }).FillAsync(CompanyName);
        await page.GetByRole(AriaRole.Button, new() { Name = "Create company" }).ClickAsync();
        await Expect(page.GetByRole(AriaRole.Heading, new() { Name = CompanyName, Level = 1 })).ToBeVisibleAsync();

        await page.GotoAsync(NewOfferPath);
        await page.GetByLabel("Company", new() { Exact = true }).FillAsync(CompanyName);
        await page.GetByLabel("Title", new() { Exact = true }).FillAsync(Title);
        await page.GetByLabel("Site", new() { Exact = true }).FillAsync(Site);
        await page.GetByLabel("Link", new() { Exact = true }).FillAsync(Url);
        await page.GetByRole(AriaRole.Button, new() { Name = "Create offer" }).ClickAsync();
        await Expect(page.GetByRole(AriaRole.Heading, new() { Name = Title, Level = 1 })).ToBeVisibleAsync();

        await page.Locator(".offer-applications").GetByRole(AriaRole.Link, new() { Name = "New" }).ClickAsync();
        await page.GetByRole(AriaRole.Button, new() { Name = "Create application" }).ClickAsync();
        await Expect(page.GetByRole(AriaRole.Heading, new() { Name = Title, Level = 1 })).ToBeVisibleAsync();

        // The input waits for the circuit; a file named .pdf whose content isn't a PDF is refused.
        await page.GetByRole(AriaRole.Link, new() { NameRegex = new Regex("^CV") }).ClickAsync();
        var input = page.GetByLabel("Choose a file");
        await Expect(input).ToBeEnabledAsync();
        await input.SetInputFilesAsync(new FilePayload { Name = NotPdfFileName, MimeType = "application/pdf", Buffer = NotPdf });
        await Expect(page.GetByRole(AriaRole.Alert)).ToHaveTextAsync(CvDocumentErrors.NotPdf);

        // A PDF is stored: the page is refreshed, so the list and the count of the tab follow.
        await input.SetInputFilesAsync(new FilePayload { Name = CvFileName, MimeType = "application/pdf", Buffer = Pdf });
        await Expect(page.Locator("nav.tabs a.tab.active .tab-count")).ToHaveTextAsync("1");
        await Expect(page.Locator(".cv-item-name")).ToHaveTextAsync(CvFileName);
        await Expect(page.GetByRole(AriaRole.Alert)).ToHaveCountAsync(0);

        await page.ReloadAsync();
        await Expect(page.Locator(".cv-item-name")).ToHaveTextAsync(CvFileName);

        // Download saves the file under the name it was uploaded with.
        var download = await page.RunAndWaitForDownloadAsync(() =>
            page.GetByRole(AriaRole.Link, new() { Name = $"Download {CvFileName}" }).ClickAsync());
        Assert.Equal(CvFileName, download.SuggestedFilename);
        Assert.Equal(Pdf, await File.ReadAllBytesAsync(await download.PathAsync(), TestContext.Current.CancellationToken));

        // Preview opens a new tab on the PDF, served inline; headless Chromium has no viewer, so the response is read.
        var preview = page.GetByRole(AriaRole.Link, new() { Name = $"Preview {CvFileName}" });
        await Expect(preview).ToHaveAttributeAsync("target", "_blank");
        var href = (await preview.GetAttributeAsync("href"))!;
        var response = await page.APIRequest.GetAsync(href);
        Assert.Equal(
            (200, "application/pdf"),
            (response.Status, response.Headers["content-type"]));
        Assert.StartsWith("inline", response.Headers["content-disposition"], StringComparison.Ordinal);
        Assert.Equal(Pdf, await response.BodyAsync());

        // Without logging in, the file isn't served: the visitor is sent to the Log in page.
        await using var anonymous = await _app.Browser.NewContextAsync(new() { BaseURL = _app.BaseUrl });
        var refused = await anonymous.APIRequest.GetAsync(href, new() { MaxRedirects = 0 });
        Assert.Equal(302, refused.Status);
        Assert.Contains("Account/Login", refused.Headers["location"], StringComparison.Ordinal);
    }
}
