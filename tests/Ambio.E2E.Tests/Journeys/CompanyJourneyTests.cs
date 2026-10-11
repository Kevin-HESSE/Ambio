using System.Text.RegularExpressions;

using Ambio.Application.Common.Attributes;
using Ambio.E2E.Tests.Fixtures;

using Microsoft.Playwright;

using static Microsoft.Playwright.Assertions;

namespace Ambio.E2E.Tests.Journeys;

[Trait("Category", "E2E")]
public class CompanyJourneyTests : IClassFixture<AmbioAppFixture>
{
    private readonly AmbioAppFixture _app;

    public CompanyJourneyTests(AmbioAppFixture app)
    {
        _app = app;
    }

    private const string UserEmail = "user@example.com";
    private const string UserPassword = "Passw0rd!";
    private const string CompaniesPath = "companies";
    private const string CompanyName = "Brightwave";
    private const string CompanySlug = "brightwave";
    private const string NewCompanyName = "Brightwave Energy";
    private const string NewCompanySlug = "brightwave-energy";
    private const string Location = "Lyon";
    private const string NewLocation = "Grenoble";
    private const string Website = "https://brightwave.io";
    private const string WebsiteWithoutDomain = "https://brightwave";

    [Fact]
    public async Task User_CreatesCompanyThenEditsIt()
    {
        await _app.CreateConfirmedUserAsync(UserEmail, UserPassword);
        await using var context = await _app.Browser.NewContextAsync(new() { BaseURL = _app.BaseUrl });
        var page = await context.NewPageAsync();
        await page.LogInAsync(UserEmail, UserPassword);

        await page.GotoAsync(CompaniesPath);
        await page.GetByRole(AriaRole.Link, new() { Name = "New company" }).First.ClickAsync();
        await page.GetByLabel("Name", new() { Exact = true }).FillAsync(CompanyName);
        await page.GetByLabel("Type").SelectOptionAsync(new SelectOptionValue { Label = "Recruitment agency" });
        await page.GetByLabel("Location").FillAsync(Location);
        await page.GetByLabel("Website").FillAsync(WebsiteWithoutDomain);
        await page.GetByRole(AriaRole.Button, new() { Name = "Create company" }).ClickAsync();
        await Expect(page.GetByText(new HttpUrlAttribute().FormatErrorMessage("Website"))).ToBeVisibleAsync();

        await page.GetByLabel("Website").FillAsync(Website);
        await page.GetByRole(AriaRole.Button, new() { Name = "Create company" }).ClickAsync();
        await Expect(page).ToHaveURLAsync(new Regex($"/{CompaniesPath}/{CompanySlug}$"));
        await Expect(page.GetByRole(AriaRole.Heading, new() { Name = CompanyName, Level = 1 })).ToBeVisibleAsync();
        await Expect(page.GetByText("Recruitment agency")).ToBeVisibleAsync();
        await Expect(page.GetByRole(AriaRole.Link, new() { Name = "brightwave.io" })).ToHaveAttributeAsync("href", Website);
        await page.GetByRole(AriaRole.Link, new() { Name = "Edit", Exact = true }).ClickAsync();
        await Expect(page.GetByLabel("Name", new() { Exact = true })).ToHaveValueAsync(CompanyName);
        await Expect(page.GetByLabel("Location")).ToHaveValueAsync(Location);
        await page.GetByLabel("Name", new() { Exact = true }).FillAsync(NewCompanyName);
        await page.GetByLabel("Location").FillAsync(NewLocation);
        await page.GetByRole(AriaRole.Button, new() { Name = "Save changes" }).ClickAsync();

        await Expect(page).ToHaveURLAsync(new Regex($"/{CompaniesPath}/{NewCompanySlug}$"));
        await Expect(page.GetByRole(AriaRole.Heading, new() { Name = NewCompanyName, Level = 1 })).ToBeVisibleAsync();
        await Expect(page.GetByText(NewLocation)).ToBeVisibleAsync();

        await page.GotoAsync(CompaniesPath);
        await page.GetByRole(AriaRole.Link, new() { Name = NewCompanyName }).First.ClickAsync();
        await Expect(page).ToHaveURLAsync(new Regex($"/{CompaniesPath}/{NewCompanySlug}$"));
    }
}
