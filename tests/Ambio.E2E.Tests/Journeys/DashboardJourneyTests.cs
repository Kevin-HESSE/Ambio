using System.Text.RegularExpressions;

using Ambio.E2E.Tests.Fixtures;

using Microsoft.Playwright;

using static Microsoft.Playwright.Assertions;

namespace Ambio.E2E.Tests.Journeys;

[Trait("Category", "E2E")]
public class DashboardJourneyTests : IClassFixture<AmbioAppFixture>
{
    private const string DashboardPath = "";
    private const string ApplicationsPath = "applications";
    private const string NewApplicationPath = "applications/new";
    private const string SentCompany = "Brightwave";
    private const string SentPosition = "Développeur Backend";
    private const string DraftCompany = "Kelvio";
    private const string DraftPosition = "Data Engineer";
    // Long before any day the tests run: always past the follow-up delay.
    private const string SentOn = "2026-09-03";

    private readonly AmbioAppFixture _app;

    public DashboardJourneyTests(AmbioAppFixture app)
    {
        _app = app;
    }

    [Fact]
    public async Task User_FollowsTheApplicationsOnTheDashboardThenOpensAStatusFromItsCounter()
    {
        var page = await _app.NewLoggedInPageAsync(new() { ViewportSize = new() { Width = 1440, Height = 900 } });

        // Without application, the dashboard says what it is for.
        await page.GotoAsync(DashboardPath);
        await Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Nothing to track yet" })).ToBeVisibleAsync();

        await page.CreateSpontaneousApplicationAsync(SentCompany, SentPosition, SentOn);
        await page.CreateSpontaneousApplicationAsync(DraftCompany, DraftPosition, sentOn: null);

        await page.GotoAsync(DashboardPath);
        var pipeline = page.GetByRole(AriaRole.Region, new() { Name = "Pipeline" });
        await Expect(pipeline).ToContainTextAsync("2 active applications");
        await Expect(pipeline.GetByRole(AriaRole.Link, new() { Name = "Draft" })).ToContainTextAsync("1");

        // Only the sent application waits for an answer.
        var followUp = page.GetByRole(AriaRole.Region, new() { Name = "Follow up" });
        await Expect(followUp.GetByRole(AriaRole.Link)).ToHaveCountAsync(1);
        await Expect(followUp).ToContainTextAsync(SentPosition);

        var activity = page.GetByRole(AriaRole.Region, new() { Name = "Recent activity" });
        await Expect(activity).ToContainTextAsync($"{DraftPosition} · {DraftCompany} was created as a draft");
        await Expect(activity).ToContainTextAsync($"{SentPosition} · {SentCompany} was sent");

        await pipeline.GetByRole(AriaRole.Link, new() { Name = "Applied" }).ClickAsync();
        await Expect(page).ToHaveURLAsync(new Regex($"/{ApplicationsPath}\\?status=applied$"));
        await Expect(page.Locator("tbody tr")).ToHaveCountAsync(1);
        await Expect(page.Locator("tbody tr")).ToContainTextAsync(SentPosition);

        // On a phone, the floating button opens a new application.
        var phone = await _app.NewLoggedInPageAsync(new() { ViewportSize = new() { Width = 390, Height = 844 }, IsMobile = true, HasTouch = true });
        await phone.GotoAsync(DashboardPath);
        await phone.GetByRole(AriaRole.Link, new() { Name = "New application" }).ClickAsync();
        await Expect(phone).ToHaveURLAsync(new Regex($"/{NewApplicationPath}$"));
    }
}
