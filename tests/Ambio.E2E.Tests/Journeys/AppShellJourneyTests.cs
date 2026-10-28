using Ambio.E2E.Tests.Fixtures;

using Microsoft.Playwright;

using static Microsoft.Playwright.Assertions;

namespace Ambio.E2E.Tests.Journeys;

[Trait("Category", "E2E")]
public class AppShellJourneyTests : IClassFixture<AmbioAppFixture>
{
    private readonly AmbioAppFixture _app;

    public AppShellJourneyTests(AmbioAppFixture app)
    {
        _app = app;
    }

    private const string MoreSheetSelector = "#more-sheet";

    [Fact]
    public async Task MobileUser_ChoosesDarkThemeAndLogsOutFromMoreSheet()
    {
        var page = await _app.NewLoggedInPageAsync(new()
        {
            ViewportSize = new() { Width = 390, Height = 844 },
            ColorScheme = ColorScheme.Light,
        });
        var html = page.Locator("html");

        await Expect(page.GetByRole(AriaRole.Navigation).GetByRole(AriaRole.Button, new() { Name = "More" })).ToBeVisibleAsync();
        await Expect(html).ToHaveAttributeAsync("data-bs-theme", "light");

        var sheet = page.Locator(MoreSheetSelector);
        await page.GetByRole(AriaRole.Button, new() { Name = "More" }).ClickAsync();
        await sheet.GetByText("Dark", new() { Exact = true }).ClickAsync();
        await Expect(html).ToHaveAttributeAsync("data-bs-theme", "dark");

        await page.ReloadAsync();
        await Expect(html).ToHaveAttributeAsync("data-bs-theme", "dark");
        await page.GetByRole(AriaRole.Button, new() { Name = "More" }).ClickAsync();
        await Expect(sheet.GetByRole(AriaRole.Radio, new() { Name = "Dark" })).ToBeCheckedAsync();

        await sheet.GetByRole(AriaRole.Button, new() { Name = "Log out" }).ClickAsync();
        await Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Log in", Level = 1 })).ToBeVisibleAsync();
        await Expect(html).ToHaveAttributeAsync("data-bs-theme", "dark");
    }
}
