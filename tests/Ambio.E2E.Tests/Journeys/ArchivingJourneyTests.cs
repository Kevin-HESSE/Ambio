using System.Text.RegularExpressions;

using Ambio.E2E.Tests.Fixtures;

using Microsoft.Playwright;

using static Microsoft.Playwright.Assertions;

namespace Ambio.E2E.Tests.Journeys;

[Trait("Category", "E2E")]
public class ArchivingJourneyTests : IClassFixture<AmbioAppFixture>
{
    private readonly AmbioAppFixture _app;

    public ArchivingJourneyTests(AmbioAppFixture app)
    {
        _app = app;
    }

    private const string ApplicationsPath = "applications";
    private const string NewOfferPath = "offers/new";
    private const string CompanyName = "Orbital Freight";
    private const string CompanyPath = "companies/orbital-freight";
    private const string Title = "Platform Engineer";
    private const string Site = "LinkedIn";
    private const string Url = "https://www.linkedin.com/jobs/view/4109283746";
    private const string OfferPathPattern = "/offers/[0-9]{6}-platform-engineer-orbital-freight$";
    private const string ApplicationPathPattern = "/applications/[0-9]{6}-platform-engineer-orbital-freight$";

    [Fact]
    public async Task User_ArchivesAnApplicationUndoesItArchivesItsCompanyThenRestoresItFromTheApplication()
    {
        var page = await _app.NewLoggedInPageAsync(new() { ViewportSize = new() { Width = 1440, Height = 900 } });

        await page.GotoAsync(NewOfferPath);
        await page.GetByLabel("Company", new() { Exact = true }).FillAsync(CompanyName);
        await page.GetByLabel("Title", new() { Exact = true }).FillAsync(Title);
        await page.GetByLabel("Site", new() { Exact = true }).FillAsync(Site);
        await page.GetByLabel("Link", new() { Exact = true }).FillAsync(Url);
        await page.GetByRole(AriaRole.Button, new() { Name = "Create offer" }).ClickAsync();
        await Expect(page).ToHaveURLAsync(new Regex(OfferPathPattern));

        await page.Locator(".offer-applications").GetByRole(AriaRole.Link, new() { Name = "New" }).ClickAsync();
        await Expect(page.Locator(".application-offer-name")).ToContainTextAsync(Title);
        await page.GetByRole(AriaRole.Button, new() { Name = "Create application" }).ClickAsync();
        await Expect(page).ToHaveURLAsync(new Regex(ApplicationPathPattern));
        var applicationUrl = page.Url;

        // Archive from the ⋯ menu, without confirmation: a toast offers to undo it, and a banner to restore it.
        await page.Locator(".detail-actions").GetByRole(AriaRole.Button, new() { Name = "More actions" }).ClickAsync();
        await page.GetByRole(AriaRole.Button, new() { Name = "Archive" }).ClickAsync();
        var toast = page.Locator(".toast");
        await Expect(toast).ToContainTextAsync("Application archived");
        await Expect(page.Locator(".archived-banner")).ToContainTextAsync("This application is archived");
        await Expect(page.GetByRole(AriaRole.Button, new() { Name = "Restore to change status" })).ToBeVisibleAsync();

        await toast.GetByRole(AriaRole.Button, new() { Name = "Undo" }).ClickAsync();
        await Expect(page.Locator(".archived-banner")).ToHaveCountAsync(0);
        await Expect(toast).ToHaveCountAsync(0);

        // Archiving the company archives its application with it.
        await page.GotoAsync(CompanyPath);
        await page.Locator(".detail-actions").GetByRole(AriaRole.Button, new() { Name = "More actions" }).ClickAsync();
        await page.GetByRole(AriaRole.Button, new() { Name = "Archive" }).ClickAsync();
        await Expect(page.Locator(".toast")).ToContainTextAsync("Company archived");

        await page.GotoAsync(ApplicationsPath);
        await Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Every application is archived" })).ToBeVisibleAsync();
        await page.Locator("form.application-filters").GetByLabel("Show archived").CheckAsync();
        await Expect(page).ToHaveURLAsync(new Regex($"/{ApplicationsPath}\\?archived=true$"));
        var row = page.Locator("tbody tr");
        await Expect(row).ToHaveCountAsync(1);
        await Expect(row).ToContainTextAsync("Archived");

        // Restoring the application brings back its offer and its company.
        await page.GotoAsync(applicationUrl);
        await page.Locator(".archived-banner").GetByRole(AriaRole.Button, new() { Name = "Restore" }).ClickAsync();
        await Expect(page.Locator(".archived-banner")).ToHaveCountAsync(0);
        await page.GotoAsync(CompanyPath);
        await Expect(page.Locator(".archived-banner")).ToHaveCountAsync(0);

        // On a phone, the ⋯ of the app bar opens the menu in a sheet.
        var phone = await _app.NewLoggedInPageAsync(new() { ViewportSize = new() { Width = 390, Height = 844 }, IsMobile = true, HasTouch = true });
        await phone.GotoAsync(applicationUrl);
        await phone.Locator(".appbar").GetByRole(AriaRole.Button, new() { Name = "More actions" }).ClickAsync();
        var sheet = phone.GetByRole(AriaRole.Dialog, new() { Name = "More actions" });
        await sheet.GetByRole(AriaRole.Button, new() { Name = "Archive" }).ClickAsync();
        await Expect(sheet).Not.ToBeVisibleAsync();
        await Expect(phone.Locator(".toast")).ToContainTextAsync("Application archived");
        await Expect(phone.Locator(".archived-banner")).ToBeVisibleAsync();
    }
}
