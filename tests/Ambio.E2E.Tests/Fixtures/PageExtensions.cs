using System.Text.RegularExpressions;

using Microsoft.Playwright;

using static Microsoft.Playwright.Assertions;

namespace Ambio.E2E.Tests.Fixtures;

/// <summary>Steps shared by the journeys, run on a Playwright page.</summary>
public static class PageExtensions
{
    private const string LoginPath = "Account/Login";

    /// <summary>Logs in through the Log in page, at any width, and waits until the app has left it.
    /// The account must exist and be confirmed, see <see cref="AmbioAppFixture.CreateConfirmedUserAsync"/>.</summary>
    public static async Task LogInAsync(this IPage page, string email, string password)
    {
        await page.GotoAsync(LoginPath);
        await page.GetByLabel("Email").FillAsync(email);
        await page.GetByLabel("Password").FillAsync(password);
        await page.GetByRole(AriaRole.Button, new() { Name = "Log in" }).ClickAsync();
        await Expect(page).Not.ToHaveURLAsync(new Regex(LoginPath));
    }
}
