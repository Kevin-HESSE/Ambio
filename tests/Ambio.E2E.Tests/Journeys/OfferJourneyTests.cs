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

    private const string OffersPath = "offers";
    private const string CompaniesPath = "companies";
    private const string CompanyName = "Kelvio";
    private const string CompanySlug = "kelvio";
    private const string Title = "Backend Developer";
    private const string NewTitle = "Senior Backend Developer";
    private const string OfferPathPattern = $"/{OffersPath}/[0-9]{{6}}-backend-developer-kelvio$";
    private const string RenamedPathPattern = $"/{OffersPath}/[0-9]{{6}}-senior-backend-developer-kelvio$";
    private const string Site = "LinkedIn";
    private const string Url = "https://www.linkedin.com/jobs/view/4102938475";
    private const string UrlWithoutDomain = "https://linkedin";
    private const string OtherSite = "Indeed";
    private const string OtherUrl = "https://fr.indeed.com/viewjob?jk=4a1f9e0c2b7d6e35";

    [Fact]
    public async Task User_CreatesOfferManagesItsPostingsRenamesItThenFindsItFromTheCompanyPage()
    {
        var page = await _app.NewLoggedInPageAsync();

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
        await Expect(page).ToHaveURLAsync(new Regex(OfferPathPattern));
        var offerUrl = page.Url;
        await Expect(page.GetByRole(AriaRole.Heading, new() { Name = Title, Level = 1 })).ToBeVisibleAsync();
        await Expect(page.GetByText(CompanyName).First).ToBeVisibleAsync();
        var postings = page.Locator(".offer-posting");
        await Expect(postings).ToHaveCountAsync(1);

        // The postings card is interactive: the Add button is enabled once the circuit is up.
        await page.GetByRole(AriaRole.Button, new() { Name = "Add", Exact = true }).ClickAsync();
        var addSheet = page.GetByRole(AriaRole.Dialog, new() { Name = "Add a posting" });
        await addSheet.GetByLabel("Site", new() { Exact = true }).FillAsync(OtherSite);
        await addSheet.GetByLabel("Link", new() { Exact = true }).FillAsync(UrlWithoutDomain);
        await addSheet.GetByRole(AriaRole.Button, new() { Name = "Add posting" }).ClickAsync();
        await Expect(addSheet.GetByText(new HttpUrlAttribute().FormatErrorMessage("Url"))).ToBeVisibleAsync();

        await addSheet.GetByLabel("Link", new() { Exact = true }).FillAsync(OtherUrl);
        await addSheet.GetByRole(AriaRole.Button, new() { Name = "Add posting" }).ClickAsync();
        await Expect(addSheet).ToBeHiddenAsync();
        await Expect(postings).ToHaveCountAsync(2);
        await Expect(postings.Nth(1)).ToContainTextAsync(OtherSite);

        await page.GetByRole(AriaRole.Button, new() { Name = $"Remove the {Site} posting" }).ClickAsync();
        var removeSheet = page.GetByRole(AriaRole.Dialog, new() { Name = "Remove this posting?" });
        await removeSheet.GetByRole(AriaRole.Button, new() { Name = "Remove", Exact = true }).ClickAsync();
        await Expect(removeSheet).ToBeHiddenAsync();
        await Expect(postings).ToHaveCountAsync(1);
        await Expect(postings.First).ToContainTextAsync(OtherSite);
        await Expect(page.GetByRole(AriaRole.Button, new() { Name = $"Remove the {OtherSite} posting" })).ToHaveCountAsync(0);

        // A new title gives the offer a new slug; its old URL still leads to it.
        await page.Locator(".detail-actions").GetByRole(AriaRole.Link, new() { Name = "Edit" }).ClickAsync();
        await page.GetByLabel("Title", new() { Exact = true }).FillAsync(NewTitle);
        await page.GetByRole(AriaRole.Button, new() { Name = "Save changes" }).ClickAsync();
        await Expect(page).ToHaveURLAsync(new Regex(RenamedPathPattern));
        var renamedUrl = page.Url;
        await Expect(page.GetByRole(AriaRole.Heading, new() { Name = NewTitle, Level = 1 })).ToBeVisibleAsync();
        await page.GotoAsync(offerUrl);
        await Expect(page).ToHaveURLAsync(renamedUrl);

        await page.GotoAsync(CompaniesPath);
        await page.GetByRole(AriaRole.Link, new() { Name = CompanyName }).First.ClickAsync();
        await Expect(page).ToHaveURLAsync(new Regex($"/{CompaniesPath}/{CompanySlug}$"));
        await Expect(page.GetByRole(AriaRole.Link, new() { Name = "Offers 1" })).ToHaveAttributeAsync("aria-current", "page");
        await page.GetByRole(AriaRole.Link, new() { Name = NewTitle }).ClickAsync();
        await Expect(page).ToHaveURLAsync(renamedUrl);
    }
}
