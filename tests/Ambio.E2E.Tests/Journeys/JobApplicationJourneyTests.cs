using System.Text.RegularExpressions;

using Ambio.E2E.Tests.Fixtures;

using Microsoft.Playwright;

using static Microsoft.Playwright.Assertions;

namespace Ambio.E2E.Tests.Journeys;

[Trait("Category", "E2E")]
public class JobApplicationJourneyTests : IClassFixture<AmbioAppFixture>
{
    private readonly AmbioAppFixture _app;

    public JobApplicationJourneyTests(AmbioAppFixture app)
    {
        _app = app;
    }

    private const string ApplicationsPath = "applications";
    private const string NewOfferPath = "offers/new";
    private const string OfferPathPattern = "/offers/[0-9a-f-]{36}$";
    private const string CompanyName = "Lumen Health";
    private const string Title = "Développeur données";
    private const string Search = "developpeur lumen";
    private const string Site = "LinkedIn";
    private const string Url = "https://www.linkedin.com/jobs/view/4102938475";
    private const string SentOn = "2026-09-03";
    private const string SentOnText = "3 Sep 2026";
    private const string ApplicationPathPattern = "/applications/[0-9]{6}-developpeur-donnees-lumen-health$";

    [Fact]
    public async Task User_AppliesToAnOfferFoundBySearchThenOpensTheApplicationFromTheListOfferAndCompanyPages()
    {
        var page = await _app.NewLoggedInPageAsync();

        await page.GotoAsync(NewOfferPath);
        await page.GetByLabel("Company", new() { Exact = true }).FillAsync(CompanyName);
        await page.GetByLabel("Title", new() { Exact = true }).FillAsync(Title);
        await page.GetByLabel("Site", new() { Exact = true }).FillAsync(Site);
        await page.GetByLabel("Link", new() { Exact = true }).FillAsync(Url);
        await page.GetByRole(AriaRole.Button, new() { Name = "Create offer" }).ClickAsync();
        await Expect(page).ToHaveURLAsync(new Regex(OfferPathPattern));
        var offerUrl = page.Url;
        await Expect(page.Locator(".offer-applications")).ToContainTextAsync("No application yet.");

        await page.GotoAsync(ApplicationsPath);
        await Expect(page.GetByRole(AriaRole.Heading, new() { Name = "No applications yet" })).ToBeVisibleAsync();
        await page.Locator(".empty").GetByRole(AriaRole.Link, new() { Name = "New application" }).ClickAsync();

        // The form is interactive: the search waits for the circuit, then for the typing to stop.
        await page.GetByLabel("Job offer").FillAsync(Search);
        await page.GetByRole(AriaRole.Button, new() { Name = Title }).ClickAsync();
        await Expect(page.Locator(".application-offer-name")).ToContainTextAsync(Title);
        await Expect(page.GetByLabel("Applied via")).Not.ToHaveValueAsync("");
        await page.GetByLabel("Channel", new() { Exact = true }).SelectOptionAsync(new SelectOptionValue { Label = "Email" });
        await page.GetByLabel("Already sent").CheckAsync();
        await page.GetByLabel("Sent on").FillAsync(SentOn);
        await page.GetByRole(AriaRole.Button, new() { Name = "Create application" }).ClickAsync();

        await Expect(page).ToHaveURLAsync(new Regex($"/{ApplicationsPath}$"));
        var row = page.Locator("tbody tr");
        await Expect(row).ToHaveCountAsync(1);
        await Expect(row).ToContainTextAsync(Title);
        await Expect(row).ToContainTextAsync("Applied");
        await Expect(row).ToContainTextAsync(SentOnText);

        await row.GetByRole(AriaRole.Link, new() { Name = Title }).ClickAsync();
        await Expect(page).ToHaveURLAsync(new Regex(ApplicationPathPattern));
        await Expect(page.GetByRole(AriaRole.Heading, new() { Name = Title, Level = 1 })).ToBeVisibleAsync();
        await Expect(page.Locator(".application-via")).ToContainTextAsync(Site);
        await Expect(page.Locator(".application-sent-on")).ToContainTextAsync(SentOnText);
        var applicationUrl = page.Url;

        // An old or mistyped slug leads to the current URL.
        await page.GotoAsync(Regex.Replace(applicationUrl, "-[^/]*$", "-old-title"));
        await Expect(page).ToHaveURLAsync(applicationUrl);

        await page.GotoAsync(offerUrl);
        var offerItem = page.Locator(".offer-applications .application-item");
        await Expect(offerItem).ToContainTextAsync($"Sent {SentOnText} via {Site}");
        await offerItem.ClickAsync();
        await Expect(page).ToHaveURLAsync(applicationUrl);

        await page.Locator(".detail-subtitle").GetByRole(AriaRole.Link, new() { Name = CompanyName }).ClickAsync();
        await page.GetByRole(AriaRole.Link, new() { Name = "Applications 1" }).ClickAsync();
        await Expect(page.GetByRole(AriaRole.Link, new() { Name = "Applications 1" })).ToHaveAttributeAsync("aria-current", "page");
        var companyItem = page.Locator(".application-item");
        await Expect(companyItem).ToContainTextAsync(Title);
        await companyItem.ClickAsync();
        await Expect(page).ToHaveURLAsync(applicationUrl);
    }
}
