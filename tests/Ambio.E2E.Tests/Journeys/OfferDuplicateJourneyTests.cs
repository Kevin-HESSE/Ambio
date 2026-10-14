using System.Text.RegularExpressions;

using Ambio.E2E.Tests.Fixtures;

using Microsoft.Playwright;

using static Microsoft.Playwright.Assertions;

namespace Ambio.E2E.Tests.Journeys;

[Trait("Category", "E2E")]
public class OfferDuplicateJourneyTests : IClassFixture<AmbioAppFixture>
{
    private readonly AmbioAppFixture _app;

    public OfferDuplicateJourneyTests(AmbioAppFixture app)
    {
        _app = app;
    }

    private const string NewOfferPath = "offers/new";
    private const string OfferPathPattern = "/offers/[0-9a-f-]{36}$";
    private const string CompanyName = "Brightwave";
    private const string Title = "Full-stack Engineer";
    private const string SimilarTitle = "Full stack engineer";
    private const string Site = "LinkedIn";
    private const string Url = "https://www.linkedin.com/jobs/view/4102938475";
    private const string OtherSite = "Indeed";
    private const string OtherUrl = "https://fr.indeed.com/viewjob?jk=4a1f9e0c2b7d6e35";
    private const string ThirdUrl = "https://www.welcometothejungle.com/fr/companies/brightwave/jobs/full-stack";

    [Fact]
    public async Task User_AddsThePostingToTheSimilarOfferThenCreatesAnotherOneAnyway()
    {
        var page = await _app.NewLoggedInPageAsync();

        await CreateOfferAsync(page, CompanyName, Title, Site, Url);
        await Expect(page).ToHaveURLAsync(new Regex(OfferPathPattern));
        var offerUrl = page.Url;

        await CreateOfferAsync(page, CompanyName.ToLowerInvariant(), SimilarTitle, OtherSite, OtherUrl);
        var banner = page.GetByRole(AriaRole.Status).Filter(new() { HasText = "This offer may already exist" });
        await Expect(banner.GetByRole(AriaRole.Link, new() { Name = $"{Title} · {CompanyName}" })).ToBeVisibleAsync();
        await banner.GetByRole(AriaRole.Button, new() { Name = "Add this posting to this offer" }).ClickAsync();
        await Expect(page).ToHaveURLAsync(offerUrl);
        var postings = page.Locator(".offer-posting");
        await Expect(postings).ToHaveCountAsync(2);
        await Expect(postings.Nth(1)).ToContainTextAsync(OtherSite);

        await CreateOfferAsync(page, CompanyName, SimilarTitle, OtherSite, ThirdUrl);
        await banner.GetByRole(AriaRole.Button, new() { Name = "Create a new offer anyway" }).ClickAsync();
        await Expect(page).ToHaveURLAsync(new Regex(OfferPathPattern));
        Assert.NotEqual(offerUrl, page.Url);
        await Expect(page.GetByRole(AriaRole.Heading, new() { Name = SimilarTitle, Level = 1 })).ToBeVisibleAsync();
        await Expect(postings).ToHaveCountAsync(1);
    }

    private static async Task CreateOfferAsync(IPage page, string companyName, string title, string site, string url)
    {
        await page.GotoAsync(NewOfferPath);
        await page.GetByLabel("Company", new() { Exact = true }).FillAsync(companyName);
        await page.GetByLabel("Title", new() { Exact = true }).FillAsync(title);
        await page.GetByLabel("Site", new() { Exact = true }).FillAsync(site);
        await page.GetByLabel("Link", new() { Exact = true }).FillAsync(url);
        await page.GetByRole(AriaRole.Button, new() { Name = "Create offer" }).ClickAsync();
    }
}
