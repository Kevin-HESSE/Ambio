using System.Text.RegularExpressions;

using Ambio.E2E.Tests.Fixtures;

using Microsoft.Playwright;

using static Microsoft.Playwright.Assertions;

namespace Ambio.E2E.Tests.Journeys;

[Trait("Category", "E2E")]
public class CoverLetterJourneyTests : IClassFixture<AmbioAppFixture>
{
    private readonly AmbioAppFixture _app;

    public CoverLetterJourneyTests(AmbioAppFixture app)
    {
        _app = app;
    }

    private const string NewCompanyPath = "companies/new";
    private const string NewOfferPath = "offers/new";
    private const string CompanyName = "Brightwave";
    private const string Title = "Backend Developer";
    private const string Site = "Welcome to the Jungle";
    private const string Url = "https://www.welcometothejungle.com/fr/companies/brightwave/jobs/backend";
    private const string LetterTitle = "Cover letter · Brightwave";
    private const string LetterText = "Dear Brightwave team,\n\nI am applying for the Backend Developer position.\n\nBest regards,\nJane Doe";
    private const string MessageTitle = "Short message for the form";
    private const string MessageText = "Hello, I would enjoy joining your platform team.";
    private const string UnsavedText = "Hello, I would enjoy joining your platform team, and I can start in January.";

    [Fact]
    public async Task User_WritesTwoCoverLettersCopiesOneDeletesItAndKeepsAnUnsavedTextThroughAStatusChange()
    {
        var page = await _app.NewLoggedInPageAsync();
        await page.Context.GrantPermissionsAsync(["clipboard-read", "clipboard-write"]);

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

        // Without letters, the tab opens on a new one; the fields are filled once the circuit is up.
        await page.GetByRole(AriaRole.Link, new() { NameRegex = new Regex("^Cover letters") }).ClickAsync();
        await Expect(page.GetByText("No letters yet.")).ToBeVisibleAsync();
        var newLetter = page.GetByRole(AriaRole.Button, new() { Name = "New", Exact = true });
        await Expect(newLetter).ToBeEnabledAsync();
        await WriteAsync(page, LetterTitle, LetterText);

        // The page is refreshed: the count of the tab follows.
        var count = page.Locator("nav.tabs a.tab.active .tab-count");
        await Expect(count).ToHaveTextAsync("1");
        await Expect(page.Locator(".cover-letter-status")).ToContainTextAsync("Saved");

        // New empties the editor once its fields are rendered again.
        await newLetter.ClickAsync();
        await Expect(page.GetByLabel("Title", new() { Exact = true })).ToHaveValueAsync("");
        await WriteAsync(page, MessageTitle, MessageText);
        await Expect(count).ToHaveTextAsync("2");
        var items = page.Locator(".cover-letter-item");
        await Expect(items).ToHaveCountAsync(2);
        await Expect(items.Nth(1)).ToHaveAttributeAsync("aria-current", "true");

        // Copy takes the text of the letter shown, and says so.
        await items.Nth(0).ClickAsync();
        await Expect(page.GetByLabel("Text", new() { Exact = true })).ToHaveValueAsync(LetterText);
        await page.GetByRole(AriaRole.Button, new() { Name = "Copy" }).ClickAsync();
        await Expect(page.Locator(".toast-host")).ToContainTextAsync("Letter copied");
        Assert.Equal(LetterText, await page.EvaluateAsync<string>("navigator.clipboard.readText()"));

        var sheet = page.GetByRole(AriaRole.Dialog, new() { Name = "Delete this letter?" });
        await page.GetByRole(AriaRole.Button, new() { Name = "Delete this letter" }).ClickAsync();
        await sheet.GetByRole(AriaRole.Button, new() { Name = "Delete" }).ClickAsync();
        await Expect(sheet).ToBeHiddenAsync();
        await Expect(count).ToHaveTextAsync("1");
        await Expect(items).ToHaveTextAsync([new Regex(MessageTitle)]);

        // The letter left is saved.
        await page.ReloadAsync();
        await Expect(page.GetByLabel("Text", new() { Exact = true })).ToHaveValueAsync(MessageText);

        // A status change refreshes the page, and keeps the text being written once the circuit is up again.
        await Expect(newLetter).ToBeEnabledAsync();
        await page.GetByLabel("Text", new() { Exact = true }).FillAsync(UnsavedText);
        var statusSheet = page.GetByRole(AriaRole.Dialog, new() { Name = "Change status" });
        await page.Locator(".change-status-open").ClickAsync();
        await statusSheet.Locator(".change-status-option", new() { HasText = "Withdrawn" }).ClickAsync();
        await statusSheet.GetByRole(AriaRole.Button, new() { Name = "Change status" }).ClickAsync();
        await Expect(page.Locator(".status-panel-status .status-badge")).ToHaveTextAsync("Withdrawn");
        await Expect(page.GetByLabel("Text", new() { Exact = true })).ToHaveValueAsync(UnsavedText);
    }

    private static async Task WriteAsync(IPage page, string title, string text)
    {
        await page.GetByLabel("Title", new() { Exact = true }).FillAsync(title);
        await page.GetByLabel("Text", new() { Exact = true }).FillAsync(text);
        await page.GetByRole(AriaRole.Button, new() { Name = "Save" }).ClickAsync();
        await Expect(page.Locator(".cover-letter-item[aria-current]")).ToContainTextAsync(title);
    }
}
