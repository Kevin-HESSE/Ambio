using System.Text.RegularExpressions;

using Ambio.E2E.Tests.Fixtures;

using Microsoft.Playwright;

using static Microsoft.Playwright.Assertions;

namespace Ambio.E2E.Tests.Journeys;

[Trait("Category", "E2E")]
public class StatusChangeJourneyTests : IClassFixture<AmbioAppFixture>
{
    private readonly AmbioAppFixture _app;

    public StatusChangeJourneyTests(AmbioAppFixture app)
    {
        _app = app;
    }

    private const string NewCompanyPath = "companies/new";
    private const string NewOfferPath = "offers/new";
    private const string ApplicationsPath = "applications";
    private const string CompanyName = "Orbital Freight";
    private const string Title = "Platform Engineer";
    private const string Site = "Indeed";
    private const string Url = "https://www.indeed.com/viewjob?jk=4102938475";
    private const string SentOn = "2026-09-03";
    private const string SentOnText = "3 Sep 2026";
    private const string Comment = "Sent through the Indeed form";
    private const string ApplicationPathPattern = "/applications/[0-9]{6}-platform-engineer-orbital-freight$";

    [Fact]
    public async Task User_CreatesACompanyAnOfferAndADraftThenSendsItAndMovesItToInterview()
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

        // The offer is already chosen: the application is created as a draft.
        await page.Locator(".offer-applications").GetByRole(AriaRole.Link, new() { Name = "New" }).ClickAsync();
        await Expect(page.Locator(".application-offer-name")).ToContainTextAsync(Title);
        await page.GetByRole(AriaRole.Button, new() { Name = "Create application" }).ClickAsync();
        await Expect(page).ToHaveURLAsync(new Regex(ApplicationPathPattern));
        await Expect(page.Locator(".status-panel-status .status-badge")).ToHaveTextAsync("Draft");
        await Expect(page.Locator(".status-timeline-item")).ToHaveCountAsync(1);

        // A draft can only be sent or dropped; sending it asks for the day.
        var sheet = page.GetByRole(AriaRole.Dialog, new() { Name = "Change status" });
        await page.Locator(".change-status-open").ClickAsync();
        await Expect(sheet.Locator(".change-status-option")).ToHaveTextAsync([new Regex("Applied"), new Regex("Withdrawn")]);
        await sheet.Locator(".change-status-option", new() { HasText = "Applied" }).ClickAsync();
        await sheet.GetByRole(AriaRole.Button, new() { Name = "Change status" }).ClickAsync();
        await Expect(sheet.GetByText("Enter the day you sent it.")).ToBeVisibleAsync();
        await sheet.GetByLabel("Sent on").FillAsync(SentOn);
        await sheet.GetByLabel("Comment").FillAsync(Comment);
        await sheet.GetByRole(AriaRole.Button, new() { Name = "Change status" }).ClickAsync();

        // The page is refreshed: status, sending day and history.
        await Expect(sheet).ToBeHiddenAsync();
        await Expect(page.Locator(".status-panel-status .status-badge")).ToHaveTextAsync("Applied");
        await Expect(page.Locator(".application-sent-on")).ToContainTextAsync(SentOnText);
        var history = page.Locator(".status-timeline-item");
        await Expect(history).ToHaveCountAsync(2);
        await Expect(history.First).ToContainTextAsync(Comment);

        // The sheet now offers what follows Applied.
        await page.Locator(".change-status-open").ClickAsync();
        await sheet.Locator(".change-status-option", new() { HasText = "Interview" }).ClickAsync();
        await sheet.GetByRole(AriaRole.Button, new() { Name = "Change status" }).ClickAsync();
        await Expect(page.Locator(".status-panel-status .status-badge")).ToHaveTextAsync("Interview");
        await Expect(history).ToHaveCountAsync(3);

        await page.GotoAsync(ApplicationsPath);
        await Expect(page.Locator("tbody tr")).ToContainTextAsync("Interview");
    }
}
