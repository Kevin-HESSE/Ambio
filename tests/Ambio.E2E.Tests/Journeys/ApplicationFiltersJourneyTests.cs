using System.Text.RegularExpressions;

using Ambio.E2E.Tests.Fixtures;

using Microsoft.Playwright;

using static Microsoft.Playwright.Assertions;

namespace Ambio.E2E.Tests.Journeys;

[Trait("Category", "E2E")]
public class ApplicationFiltersJourneyTests : IClassFixture<AmbioAppFixture>
{
    private readonly AmbioAppFixture _app;

    public ApplicationFiltersJourneyTests(AmbioAppFixture app)
    {
        _app = app;
    }

    private const string ApplicationsPath = "applications";
    private const string NewApplicationPath = "applications/new";
    private const string SentCompany = "Brightwave";
    private const string SentPosition = "Développeur Backend";
    private const string DraftCompany = "Kelvio";
    private const string DraftPosition = "Data Engineer";
    private const string SentOn = "2026-09-03";
    private const string SearchWithoutAccents = "developpeur";

    [Fact]
    public async Task User_SearchesAndFiltersTheApplicationsOnDesktopThenWithTheChipsAndTheSheetOnAPhone()
    {
        var page = await _app.NewLoggedInPageAsync(new() { ViewportSize = new() { Width = 1440, Height = 900 } });
        await CreateSpontaneousApplicationAsync(page, SentCompany, SentPosition, SentOn);
        await CreateSpontaneousApplicationAsync(page, DraftCompany, DraftPosition, sentOn: null);

        // Desktop: the search applies on Enter, and finds the title without its accents.
        await page.GotoAsync(ApplicationsPath);
        var rows = page.Locator("tbody tr");
        await Expect(rows).ToHaveCountAsync(2);
        var search = page.Locator("form.application-filters").GetByRole(AriaRole.Searchbox, new() { Name = "Search" });
        await search.FillAsync(SearchWithoutAccents);
        await search.PressAsync("Enter");
        await Expect(page).ToHaveURLAsync(new Regex($"/{ApplicationsPath}\\?q={SearchWithoutAccents}$"));
        await Expect(rows).ToHaveCountAsync(1);
        await Expect(rows).ToContainTextAsync(SentPosition);

        // A select applies as soon as it changes, and the empty fields stay out of the URL.
        await page.GetByRole(AriaRole.Combobox, new() { Name = "Status" }).SelectOptionAsync(new SelectOptionValue { Label = "Draft" });
        await Expect(page).ToHaveURLAsync(new Regex($"/{ApplicationsPath}\\?q={SearchWithoutAccents}&status=draft$"));
        await Expect(page.GetByRole(AriaRole.Heading, new() { Name = "No matching applications" })).ToBeVisibleAsync();

        await page.Locator("form.application-filters").GetByRole(AriaRole.Link, new() { Name = "Clear filters" }).ClickAsync();
        await Expect(page).ToHaveURLAsync(new Regex($"/{ApplicationsPath}$"));
        await Expect(rows).ToHaveCountAsync(2);

        // Phone: a status chip, then the Filters sheet, which keeps the status of the chip.
        var phone = await _app.NewLoggedInPageAsync(new() { ViewportSize = new() { Width = 390, Height = 844 }, IsMobile = true, HasTouch = true });
        await phone.GotoAsync(ApplicationsPath);
        var cards = phone.Locator(".list-card");
        await Expect(cards).ToHaveCountAsync(2);
        await phone.Locator(".status-chips").GetByRole(AriaRole.Link, new() { Name = "Applied" }).ClickAsync();
        await Expect(phone).ToHaveURLAsync(new Regex($"/{ApplicationsPath}\\?status=applied$"));
        await Expect(cards).ToHaveCountAsync(1);
        await Expect(cards).ToContainTextAsync(SentPosition);

        await phone.GetByRole(AriaRole.Button, new() { Name = "Filters" }).ClickAsync();
        var sheet = phone.GetByRole(AriaRole.Dialog, new() { Name = "Filters" });
        await sheet.GetByLabel("Company").SelectOptionAsync(new SelectOptionValue { Label = DraftCompany });
        await sheet.GetByRole(AriaRole.Button, new() { Name = "Show results" }).ClickAsync();
        await Expect(phone).ToHaveURLAsync(new Regex($"/{ApplicationsPath}\\?status=applied&company=kelvio$"));
        await Expect(sheet).ToBeHiddenAsync();
        await Expect(phone.Locator("form.application-filters-mobile .count-badge")).ToHaveTextAsync("1");
        await Expect(phone.GetByRole(AriaRole.Heading, new() { Name = "No matching applications" })).ToBeVisibleAsync();

        await phone.Locator(".status-chips").GetByRole(AriaRole.Link, new() { Name = "All" }).ClickAsync();
        await Expect(phone).ToHaveURLAsync(new Regex($"/{ApplicationsPath}\\?company=kelvio$"));
        await Expect(cards).ToHaveCountAsync(1);
        await Expect(cards).ToContainTextAsync(DraftPosition);
    }

    // A spontaneous application to a new company, sent on sentOn or left a draft.
    private static async Task CreateSpontaneousApplicationAsync(IPage page, string companyName, string targetPosition, string? sentOn)
    {
        await page.GotoAsync(NewApplicationPath);
        // The toggle waits for the circuit; its radio is hidden behind its label, which takes the click.
        await Expect(page.GetByRole(AriaRole.Radio, new() { Name = "Spontaneous" })).ToBeEnabledAsync();
        await page.GetByText("Spontaneous", new() { Exact = true }).ClickAsync();
        await page.GetByLabel("Company", new() { Exact = true }).FillAsync(companyName);
        await page.GetByRole(AriaRole.Button, new() { Name = $"Create the company “{companyName}”" }).ClickAsync();
        await page.GetByLabel("Target position").FillAsync(targetPosition);
        if (sentOn is not null)
        {
            await page.GetByLabel("Already sent").CheckAsync();
            await page.GetByLabel("Sent on").FillAsync(sentOn);
        }
        await page.GetByRole(AriaRole.Button, new() { Name = "Create application" }).ClickAsync();
        await Expect(page.GetByRole(AriaRole.Heading, new() { Name = targetPosition, Level = 1 })).ToBeVisibleAsync();
    }
}
