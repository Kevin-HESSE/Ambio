using System.Text.RegularExpressions;

using Microsoft.Playwright;

using static Microsoft.Playwright.Assertions;

namespace Ambio.E2E.Tests.Fixtures;

/// <summary>Steps shared by the journeys, run on a Playwright page.</summary>
public static class PageExtensions
{
    private const string LoginPath = "Account/Login";
    private const string NewApplicationPath = "applications/new";

    /// <summary>Logs in through the Log in page, at any width, and waits until the app has left it.
    /// The app creates the account at start, see <see cref="AmbioAppFixture"/>.</summary>
    public static async Task LogInAsync(this IPage page, string email, string password)
    {
        await page.GotoAsync(LoginPath);
        await page.GetByLabel("Email").FillAsync(email);
        await page.GetByLabel("Password").FillAsync(password);
        await page.GetByRole(AriaRole.Button, new() { Name = "Log in" }).ClickAsync();
        await Expect(page).Not.ToHaveURLAsync(new Regex(LoginPath));
    }

    /// <summary>Creates a spontaneous application to a new company from the new application form, sent on
    /// <paramref name="sentOn"/> (yyyy-MM-dd) or left a draft, and waits for its page.</summary>
    public static async Task CreateSpontaneousApplicationAsync(this IPage page, string companyName, string targetPosition, string? sentOn)
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
