using Ambio.E2E.Tests.Fixtures;

using Microsoft.Playwright;

using static Microsoft.Playwright.Assertions;

namespace Ambio.E2E.Tests.Journeys;

[Trait("Category", "E2E")]
public class AppShellJourneyTests(AmbioAppFixture app) : IClassFixture<AmbioAppFixture>
{
    private const string UserEmail = "user@example.com";
    private const string UserPassword = "Passw0rd!";
    private const string LoginPath = "Account/Login";
    private const string MoreSheetSelector = "#more-sheet";

    [Fact]
    public async Task MobileUser_ChoosesDarkThemeAndLogsOutFromMoreSheet()
    {
        await app.CreateConfirmedUserAsync(UserEmail, UserPassword);
        await using var context = await app.Browser.NewContextAsync(new()
        {
            BaseURL = app.BaseUrl,
            ViewportSize = new() { Width = 390, Height = 844 },
            ColorScheme = ColorScheme.Light,
        });
        var page = await context.NewPageAsync();
        var html = page.Locator("html");

        await page.GotoAsync(LoginPath);
        await page.GetByLabel("Email").FillAsync(UserEmail);
        await page.GetByLabel("Password").FillAsync(UserPassword);
        await page.GetByRole(AriaRole.Button, new() { Name = "Log in" }).ClickAsync();
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
        await page.GetByRole(AriaRole.Button, new() { Name = "More" }).ClickAsync();
        await Expect(sheet.GetByRole(AriaRole.Link, new() { Name = "Log in" })).ToBeVisibleAsync();
    }
}
