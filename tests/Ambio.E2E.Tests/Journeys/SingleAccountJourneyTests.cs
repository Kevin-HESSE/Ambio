using System.Text.RegularExpressions;

using Ambio.E2E.Tests.Fixtures;

using Microsoft.Playwright;

using static Microsoft.Playwright.Assertions;

namespace Ambio.E2E.Tests.Journeys;

[Trait("Category", "E2E")]
public class SingleAccountJourneyTests : IClassFixture<AmbioAppFixture>
{
    private readonly AmbioAppFixture _app;

    public SingleAccountJourneyTests(AmbioAppFixture app)
    {
        _app = app;
    }

    private const string ApplicationsPath = "applications";
    private const string LoginPath = "Account/Login";
    private const string ManagePath = "Account/Manage";
    private const string RegisterPath = "Account/Register";
    private const string RegistrationClosedMessage = "Registration is closed";

    [Fact]
    public async Task Visitor_IsSentToLogInThenLogsInWithTheAccountCreatedAtStart()
    {
        await using var context = await _app.Browser.NewContextAsync(new() { BaseURL = _app.BaseUrl });
        var page = await context.NewPageAsync();

        // The Log in page gets its styles and scripts before anyone is logged in.
        foreach (var asset in new[] { "_framework/blazor.web.js", "app.css" })
        {
            var response = await context.APIRequest.GetAsync(asset, new() { MaxRedirects = 0 });
            Assert.Equal(200, response.Status);
        }

        // Every page asks to log in, the account pages and the old Register page included.
        await page.GotoAsync(RegisterPath);
        await Expect(page).ToHaveURLAsync(new Regex(LoginPath));
        await page.GotoAsync(ManagePath);
        await Expect(page).ToHaveURLAsync(new Regex(LoginPath));
        await Expect(page.GetByText(RegistrationClosedMessage)).ToBeVisibleAsync();
        await Expect(page.GetByRole(AriaRole.Link, new() { Name = "Create it" })).ToHaveCountAsync(0);

        // The pages shown before logging in open without an account.
        await page.GetByRole(AriaRole.Link, new() { Name = "Forgot your password?" }).ClickAsync();
        await Expect(page.GetByRole(AriaRole.Heading, new() { Level = 1 })).ToContainTextAsync("Forgot your password");

        // The account created at start logs in, back on the page asked first.
        await page.GotoAsync(ApplicationsPath);
        await Expect(page).ToHaveURLAsync(new Regex($"{LoginPath}\\?ReturnUrl=%2F{ApplicationsPath}$"));
        await page.GetByLabel("Email").FillAsync(AmbioAppFixture.UserEmail);
        await page.GetByLabel("Password").FillAsync(AmbioAppFixture.UserPassword);
        await page.GetByRole(AriaRole.Button, new() { Name = "Log in" }).ClickAsync();
        await Expect(page).ToHaveURLAsync(new Regex($"/{ApplicationsPath}$"));
        await Expect(page.GetByRole(AriaRole.Link, new() { Name = AmbioAppFixture.UserEmail })).ToBeVisibleAsync();

        await page.GetByRole(AriaRole.Button, new() { Name = "Log out" }).ClickAsync();
        await Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Log in", Level = 1 })).ToBeVisibleAsync();
    }
}
