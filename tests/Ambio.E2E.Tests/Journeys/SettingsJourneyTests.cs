using System.Text.RegularExpressions;

using Ambio.E2E.Tests.Fixtures;

using Microsoft.Playwright;

using static Microsoft.Playwright.Assertions;

namespace Ambio.E2E.Tests.Journeys;

[Trait("Category", "E2E")]
public class SettingsJourneyTests : IClassFixture<AmbioAppFixture>
{
    private const string DelayLabel = "Suggest a follow-up after";

    private readonly AmbioAppFixture _app;

    public SettingsJourneyTests(AmbioAppFixture app)
    {
        _app = app;
    }

    [Fact]
    public async Task User_ChangesTheFollowUpDelayAndTheThemeOnTheSettingsPage()
    {
        var page = await _app.NewLoggedInPageAsync(new()
        {
            ViewportSize = new() { Width = 1440, Height = 900 },
            ColorScheme = ColorScheme.Light,
        });
        var sidebar = page.Locator(".sidebar");
        var settings = page.Locator("main");
        var delay = page.GetByLabel(DelayLabel);

        // Before any save, the delay is the default one.
        await sidebar.GetByRole(AriaRole.Link, new() { Name = "Settings" }).ClickAsync();
        await Expect(page).ToHaveURLAsync(new Regex("/settings$"));
        await Expect(delay).ToHaveValueAsync("7");

        await delay.FillAsync("0");
        await page.GetByRole(AriaRole.Button, new() { Name = "Save settings" }).ClickAsync();
        await Expect(settings.GetByText("Enter a number of days between 1 and 30.")).ToBeVisibleAsync();

        await delay.FillAsync("14");
        await page.GetByRole(AriaRole.Button, new() { Name = "Save settings" }).ClickAsync();
        await Expect(page.Locator(".toast")).ToContainTextAsync("Settings saved");

        await page.ReloadAsync();
        await Expect(delay).ToHaveValueAsync("14");

        // The theme applies at once, without saving, and the selector of the sidebar follows.
        await settings.GetByText("Dark", new() { Exact = true }).ClickAsync();
        await Expect(page.Locator("html")).ToHaveAttributeAsync("data-bs-theme", "dark");
        await Expect(sidebar.GetByRole(AriaRole.Radio, new() { Name = "Dark" })).ToBeCheckedAsync();
    }
}
