using System.Text.RegularExpressions;

using Ambio.E2E.Tests.Fixtures;

using Microsoft.Playwright;

using static Microsoft.Playwright.Assertions;

namespace Ambio.E2E.Tests.Journeys;

[Trait("Category", "E2E")]
public class SpontaneousApplicationJourneyTests : IClassFixture<AmbioAppFixture>
{
    private readonly AmbioAppFixture _app;

    public SpontaneousApplicationJourneyTests(AmbioAppFixture app)
    {
        _app = app;
    }

    private const string ApplicationsPath = "applications";
    private const string NewApplicationPath = "applications/new";
    private const string CompaniesPath = "companies";
    private const string CompanyName = "Northwind Labs";
    private const string CompanySlug = "northwind-labs";
    private const string TargetPosition = "Backend Developer";
    private const string SentOn = "2026-09-08";
    private const string SentOnText = "8 Sep 2026";
    private const string ApplicationPathPattern = "/applications/[0-9]{6}-backend-developer-northwind-labs$";

    [Fact]
    public async Task User_SendsASpontaneousApplicationToANewCompanyThenFindsItInTheListAndOnTheCompanyPage()
    {
        var page = await _app.NewLoggedInPageAsync();

        await page.GotoAsync(NewApplicationPath);
        // The toggle waits for the circuit; its radio is hidden behind its label, which takes the click.
        var spontaneous = page.GetByRole(AriaRole.Radio, new() { Name = "Spontaneous" });
        await Expect(spontaneous).ToBeEnabledAsync();
        await page.GetByText("Spontaneous", new() { Exact = true }).ClickAsync();
        await Expect(spontaneous).ToBeCheckedAsync();
        await page.GetByLabel("Company", new() { Exact = true }).FillAsync(CompanyName);
        await page.GetByRole(AriaRole.Button, new() { Name = $"Create the company “{CompanyName}”" }).ClickAsync();
        await Expect(page.Locator(".application-company-name")).ToContainTextAsync(CompanyName);
        await page.GetByLabel("Target position").FillAsync(TargetPosition);
        await page.GetByLabel("Channel", new() { Exact = true }).SelectOptionAsync(new SelectOptionValue { Label = "Email" });
        await page.GetByLabel("Already sent").CheckAsync();
        await page.GetByLabel("Sent on").FillAsync(SentOn);
        await page.GetByRole(AriaRole.Button, new() { Name = "Create application" }).ClickAsync();

        await Expect(page).ToHaveURLAsync(new Regex(ApplicationPathPattern));
        await Expect(page.GetByRole(AriaRole.Heading, new() { Name = TargetPosition, Level = 1 })).ToBeVisibleAsync();
        await Expect(page.Locator(".detail-tags")).ToContainTextAsync("Spontaneous");

        await page.GotoAsync(ApplicationsPath);
        var row = page.Locator("tbody tr");
        await Expect(row).ToHaveCountAsync(1);
        await Expect(row).ToContainTextAsync(TargetPosition);
        await Expect(row).ToContainTextAsync(CompanyName);
        await Expect(row).ToContainTextAsync("Spontaneous");
        await Expect(row).ToContainTextAsync(SentOnText);

        await page.GotoAsync(CompaniesPath);
        await page.GetByRole(AriaRole.Link, new() { Name = CompanyName }).First.ClickAsync();
        await Expect(page).ToHaveURLAsync(new Regex($"/{CompaniesPath}/{CompanySlug}$"));
        await page.GetByRole(AriaRole.Link, new() { Name = "Applications 1" }).ClickAsync();
        var item = page.Locator(".application-item");
        await Expect(item).ToContainTextAsync(TargetPosition);
        await Expect(item).ToContainTextAsync("Spontaneous");
    }
}
