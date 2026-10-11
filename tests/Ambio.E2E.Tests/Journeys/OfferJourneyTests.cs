using System.Text.RegularExpressions;

using Ambio.Application.Common.Attributes;
using Ambio.E2E.Tests.Fixtures;

using Microsoft.Playwright;

using static Microsoft.Playwright.Assertions;

namespace Ambio.E2E.Tests.Journeys;

[Trait("Category", "E2E")]
public class OfferJourneyTests : IClassFixture<AmbioAppFixture>
{
    private readonly AmbioAppFixture _app;

    public OfferJourneyTests(AmbioAppFixture app)
    {
        _app = app;
    }

    private const string UserEmail = "user@example.com";
    private const string UserPassword = "Passw0rd!";
    private const string OffersPath = "offers";
    private const string CompaniesPath = "companies";
    private const string CompanyName = "Kelvio";
    private const string CompanySlug = "kelvio";
    private const string Title = "Backend Developer";
    private const string Site = "LinkedIn";
    private const string Url = "https://www.linkedin.com/jobs/view/4102938475";
    private const string UrlWithoutDomain = "https://linkedin";

    [Fact]
    public async Task User_CreatesOfferWithNewCompanyThenFindsItOnTheCompanyPage()
    {
        await _app.CreateConfirmedUserAsync(UserEmail, UserPassword);
        await using var context = await _app.Browser.NewContextAsync(new() { BaseURL = _app.BaseUrl });
        var page = await context.NewPageAsync();
        await page.LogInAsync(UserEmail, UserPassword);

        await page.GotoAsync(OffersPath);
        await Expect(page.GetByRole(AriaRole.Heading, new() { Name = "No offers yet" })).ToBeVisibleAsync();
        await page.GetByRole(AriaRole.Link, new() { Name = "New offer" }).First.ClickAsync();
        await page.GetByLabel("Company", new() { Exact = true }).FillAsync(CompanyName);
        await page.GetByLabel("Title", new() { Exact = true }).FillAsync(Title);
        await page.GetByLabel("Contract").SelectOptionAsync(new SelectOptionValue { Label = "Freelance" });
        await page.GetByLabel("Site", new() { Exact = true }).FillAsync(Site);
        await page.GetByLabel("Link", new() { Exact = true }).FillAsync(UrlWithoutDomain);
        await page.GetByRole(AriaRole.Button, new() { Name = "Create offer" }).ClickAsync();
        await Expect(page.GetByText(new HttpUrlAttribute().FormatErrorMessage("Url"))).ToBeVisibleAsync();

        await page.GetByLabel("Link", new() { Exact = true }).FillAsync(Url);
        await page.GetByRole(AriaRole.Button, new() { Name = "Create offer" }).ClickAsync();
        await Expect(page).ToHaveURLAsync(new Regex($"/{OffersPath}$"));
        var row = page.GetByRole(AriaRole.Row).Filter(new() { HasText = Title });
        await Expect(row).ToContainTextAsync(CompanyName);
        await Expect(row).ToContainTextAsync("Freelance");

        await page.GotoAsync(CompaniesPath);
        await page.GetByRole(AriaRole.Link, new() { Name = CompanyName }).First.ClickAsync();
        await Expect(page).ToHaveURLAsync(new Regex($"/{CompaniesPath}/{CompanySlug}$"));
        await Expect(page.GetByRole(AriaRole.Link, new() { Name = "Offers 1" })).ToHaveAttributeAsync("aria-current", "page");
        await Expect(page.GetByText(Title)).ToBeVisibleAsync();
    }
}
