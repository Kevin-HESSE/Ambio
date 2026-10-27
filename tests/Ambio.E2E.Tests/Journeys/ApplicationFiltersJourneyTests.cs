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
        await page.CreateSpontaneousApplicationAsync(SentCompany, SentPosition, SentOn);
        await page.CreateSpontaneousApplicationAsync(DraftCompany, DraftPosition, sentOn: null);

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
}
