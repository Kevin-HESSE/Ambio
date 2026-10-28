using Ambio.Application.Dashboard;
using Ambio.Application.Dashboard.Dtos;
using Ambio.Domain.JobApplications;
using Ambio.Web.Components.JobApplications;
using Ambio.Web.Components.Offers;
using Ambio.Web.Components.Pages;
using Ambio.Web.Tests.Fixtures;

using Bunit;

using Microsoft.Extensions.DependencyInjection;

using NSubstitute;

namespace Ambio.Web.Tests.Components.Pages;

public class HomeTests : BunitContext
{
    private const string Today = "Thursday 3 September 2026";

    private readonly IDashboardService _dashboardService = Substitute.For<IDashboardService>();

    public HomeTests()
    {
        Services.AddSingleton(_dashboardService);
    }

    [Fact]
    public void Render_WithoutApplications_ShowsEmptyStateWithNewApplicationAndAddOfferLinks()
    {
        GivenDashboard(applied: 0);

        var cut = Render<Home>();

        Assert.NotNull(cut.Find($".empty a[href='{JobApplicationRoutes.New}']"));
        Assert.NotNull(cut.Find($".empty a[href='{OfferRoutes.New}']"));
        Assert.Empty(cut.FindAll(".pipeline-bar"));
    }

    [Fact]
    public void Render_WithApplications_ShowsTheThreeCardsUnderTheDay()
    {
        GivenDashboard(applied: 2);

        var cut = Render<Home>();

        Assert.Empty(cut.FindAll(".empty"));
        Assert.Equal(
            ["Pipeline", "Follow up", "Recent activity"],
            cut.FindAll("section.card h2").Select(title => title.FirstChild!.TextContent.Trim()));
        Assert.Equal(Today, cut.Find(".page-subtitle").TextContent);
    }

    [Fact]
    public void Render_AsksForTheRecentActivityCount()
    {
        GivenDashboard(applied: 1);

        Render<Home>();

        _dashboardService.Received(1).GetAsync(Home.RecentActivityCount, Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(2)]
    public void Render_WithOrWithoutApplications_ShowsTheNewApplicationFloatingButton(int applied)
    {
        GivenDashboard(applied);

        var cut = Render<Home>();

        var fab = cut.Find(".btn-fab");
        Assert.Equal(JobApplicationRoutes.New, fab.GetAttribute("href"));
        Assert.Equal("New application", fab.GetAttribute("aria-label"));
    }

    private void GivenDashboard(int applied)
    {
        var followUps = JobApplicationSummaryFactory.GetGenerator().Generate(applied);
        var dashboard = new DashboardDto(
            Today: new DateOnly(2026, 9, 3),
            CountsByStatus: Enum.GetValues<ApplicationStatus>().ToDictionary(s => s, s => s == ApplicationStatus.Applied ? applied : 0),
            FollowUpAfterDays: 7,
            FollowUps: followUps,
            RecentActivity: ActivityFactory.GetGenerator().Generate(applied));
        _dashboardService.GetAsync(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(dashboard);
    }
}
