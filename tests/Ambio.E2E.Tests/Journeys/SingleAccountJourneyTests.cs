using System.Text.RegularExpressions;

using Ambio.E2E.Tests.Fixtures;

using Microsoft.Playwright;

using static Microsoft.Playwright.Assertions;

namespace Ambio.E2E.Tests.Journeys;

[Trait("Category", "E2E")]
public class SingleAccountJourneyTests(AmbioAppFixture app) : IClassFixture<AmbioAppFixture>
{
    private const string UserEmail = "user@example.com";
    private const string UserPassword = "Passw0rd!";
    private const string RegisterPath = "Account/Register";
    private const string LoginPath = "Account/Login";
    private const string ConfirmationLinkText = "Click here to confirm your account";
    private const string EmailConfirmedMessage = "Thank you for confirming your email.";
    private const string AccountAlreadyExistsMessage = "An account already exists";

    [Fact]
    public async Task FirstUser_RegistersConfirmsLogsInAndClosesRegistration()
    {
        await using var context = await app.Browser.NewContextAsync(new() { BaseURL = app.BaseUrl });
        var page = await context.NewPageAsync();

        await page.GotoAsync(RegisterPath);
        await page.GetByLabel("Email").FillAsync(UserEmail);
        await page.GetByLabel("Password", new() { Exact = true }).FillAsync(UserPassword);
        await page.GetByLabel("Confirm Password").FillAsync(UserPassword);
        await page.GetByRole(AriaRole.Button, new() { Name = "Register" }).ClickAsync();

        await page.GetByRole(AriaRole.Link, new() { Name = ConfirmationLinkText }).ClickAsync();
        await Expect(page.GetByRole(AriaRole.Alert)).ToHaveTextAsync(EmailConfirmedMessage);

        await page.GotoAsync(LoginPath);
        await page.GetByLabel("Email").FillAsync(UserEmail);
        await page.GetByLabel("Password").FillAsync(UserPassword);
        await page.GetByRole(AriaRole.Button, new() { Name = "Log in" }).ClickAsync();
        await Expect(page.GetByRole(AriaRole.Link, new() { Name = UserEmail })).ToBeVisibleAsync();

        await page.GetByRole(AriaRole.Button, new() { Name = "Logout" }).ClickAsync();
        await Expect(page.GetByRole(AriaRole.Link, new() { Name = "Login" })).ToBeVisibleAsync();

        await page.GotoAsync(RegisterPath);
        await Expect(page).ToHaveURLAsync(new Regex(LoginPath));
        await Expect(page.GetByRole(AriaRole.Alert)).ToHaveTextAsync(AccountAlreadyExistsMessage);
    }
}
