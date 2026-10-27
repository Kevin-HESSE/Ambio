using Ambio.Application.Dashboard;
using Ambio.Application.Dashboard.Dtos;
using Ambio.Domain.JobApplications;
using Ambio.Domain.Settings;
using Ambio.Infrastructure.Tests.Fixtures;
using Ambio.Infrastructure.Tests.Stories;

using Bogus;

using NSubstitute;

namespace Ambio.Infrastructure.Tests.Dashboard;

public class DashboardServiceTests : IAsyncLifetime
{
    private const int ActivityCount = 5;

    private static readonly DateTimeOffset Now = new(2026, 9, 28, 10, 0, 0, TimeSpan.Zero);
    private static readonly DateOnly Today = new(2026, 9, 28);

    private SqliteServiceProvider _services = null!;
    private Faker<SpontaneousApplication> _generator = null!;

    public async ValueTask InitializeAsync()
    {
        _services = await SqliteServiceProvider.CreateAsync();
        _services.TimeProvider.GetUtcNow().Returns(Now);
        var story = new SpontaneousApplicationStory();
        await story.SaveAsync(_services);
        _generator = SpontaneousApplicationFactory.GetGenerator(story.Company.Id);
    }

    public ValueTask DisposeAsync() => _services.DisposeAsync();

    [Fact]
    public async Task GetAsync_WithoutApplications_ReturnsZeroForEveryStatusAndTheDefaultDelay()
    {
        var dashboard = await GetAsync();

        Assert.Equal(Today, dashboard.Today);
        Assert.Equal(Enum.GetValues<ApplicationStatus>(), dashboard.CountsByStatus.Keys.Order());
        Assert.All(dashboard.CountsByStatus.Values, count => Assert.Equal(0, count));
        Assert.Equal(UserSettings.DefaultFollowUpAfterDays, dashboard.FollowUpAfterDays);
        Assert.Empty(dashboard.FollowUps);
        Assert.Empty(dashboard.RecentActivity);
    }

    [Fact]
    public async Task GetAsync_WithApplications_CountsTheActiveOnesByStatus()
    {
        var (draft, sent, otherSent, archived) = (_generator.Generate(), Sent(Today), Sent(Today), Sent(Today));
        archived.Archive(Now.UtcDateTime);
        await AddAtAsync(Now, draft, sent, otherSent, archived);

        var dashboard = await GetAsync();

        Assert.Equal((1, 2, 0, 3), (
            dashboard.CountsByStatus[ApplicationStatus.Draft],
            dashboard.CountsByStatus[ApplicationStatus.Applied],
            dashboard.CountsByStatus[ApplicationStatus.Interview],
            dashboard.Total));
    }

    [Fact]
    public async Task GetAsync_WithDefaultDelay_FollowsUpTheApplicationsStillAppliedSinceTheDelayOldestFirst()
    {
        var lastSendingDay = Today.AddDays(-UserSettings.DefaultFollowUpAfterDays);
        var (sentOnLastDay, sentLongAgo, sentTheDayAfter, answered, archived, draft) = (
            Sent(lastSendingDay), Sent(lastSendingDay.AddDays(-20)), Sent(lastSendingDay.AddDays(1)), Sent(lastSendingDay.AddDays(-20)),
            Sent(lastSendingDay.AddDays(-20)), _generator.Generate());
        answered.ChangeStatus(ApplicationStatus.Interview, Now.UtcDateTime);
        archived.Archive(Now.UtcDateTime);
        await AddAtAsync(Now, sentOnLastDay, sentLongAgo, sentTheDayAfter, answered, archived, draft);

        var dashboard = await GetAsync();

        Assert.Equal([sentLongAgo.Id, sentOnLastDay.Id], dashboard.FollowUps.Select(a => a.Id));
    }

    [Fact]
    public async Task GetAsync_WithSavedDelay_FollowsUpAfterIt()
    {
        var settings = UserSettingsFactory.GetGenerator().Generate();
        await _services.AddAsync(settings);
        var lastSendingDay = Today.AddDays(-settings.FollowUpAfterDays);
        var (sentOnLastDay, sentTheDayAfter) = (Sent(lastSendingDay), Sent(lastSendingDay.AddDays(1)));
        await AddAtAsync(Now, sentOnLastDay, sentTheDayAfter);

        var dashboard = await GetAsync();

        Assert.Equal(settings.FollowUpAfterDays, dashboard.FollowUpAfterDays);
        Assert.Equal([sentOnLastDay.Id], dashboard.FollowUps.Select(a => a.Id));
    }

    [Fact]
    public async Task GetAsync_WithCreationsAndChanges_ListsTheActivityNewestFirst()
    {
        // A draft, sent the next day; then an application created as already sent, which is only its sending.
        var sentLater = _generator.Generate();
        sentLater.ChangeStatus(ApplicationStatus.Applied, Now.AddDays(-2).UtcDateTime, appliedOn: Today.AddDays(-2));
        await AddAtAsync(Now.AddDays(-3), sentLater);
        var createdAsSent = Sent(Today.AddDays(-1), Now.AddDays(-1));
        await AddAtAsync(Now.AddDays(-1), createdAsSent);

        var dashboard = await GetAsync();

        Assert.Equal(
            [
                (createdAsSent.Id, ApplicationStatus.Applied, Now.AddDays(-1).UtcDateTime),
                (sentLater.Id, ApplicationStatus.Applied, Now.AddDays(-2).UtcDateTime),
                (sentLater.Id, ApplicationStatus.Draft, Now.AddDays(-3).UtcDateTime),
            ],
            dashboard.RecentActivity.Select(e => (e.Application.Id, e.Status, e.OccurredAt)));
    }

    [Fact]
    public async Task GetAsync_WithMoreEventsThanAsked_KeepsTheLatestActiveOnes()
    {
        var (archivedOfToday, ofYesterday, ofLastWeek, ofLastMonth) =
            (_generator.Generate(), _generator.Generate(), _generator.Generate(), _generator.Generate());
        archivedOfToday.Archive(Now.UtcDateTime);
        await AddAtAsync(Now, archivedOfToday);
        await AddAtAsync(Now.AddDays(-1), ofYesterday);
        await AddAtAsync(Now.AddDays(-7), ofLastWeek);
        await AddAtAsync(Now.AddDays(-30), ofLastMonth);

        var dashboard = await GetAsync(activityCount: 2);

        Assert.Equal([ofYesterday.Id, ofLastWeek.Id], dashboard.RecentActivity.Select(e => e.Application.Id));
    }

    [Fact]
    public async Task GetAsync_WithSpontaneousApplication_DescribesItsEventWithTheTargetPosition()
    {
        var application = _generator.Generate();
        await AddAtAsync(Now, application);

        var activity = Assert.Single((await GetAsync()).RecentActivity);

        Assert.Equal((application.ShortId, application.TargetPosition), (activity.Application.ShortId, activity.Application.Title));
    }

    // An application sent on sentOn, its sending recorded at sentAt (now by default), before it is saved.
    private SpontaneousApplication Sent(DateOnly sentOn, DateTimeOffset? sentAt = null)
    {
        var application = _generator.Generate();
        application.ChangeStatus(ApplicationStatus.Applied, (sentAt ?? Now).UtcDateTime, appliedOn: sentOn);
        return application;
    }

    private async Task AddAtAsync(DateTimeOffset createdAt, params JobApplication[] applications)
    {
        _services.TimeProvider.GetUtcNow().Returns(createdAt);
        await _services.AddAsync(applications);
        _services.TimeProvider.GetUtcNow().Returns(Now);
    }

    private Task<DashboardDto> GetAsync(int activityCount = ActivityCount) =>
        _services.CallAsync((IDashboardService service) => service.GetAsync(activityCount, TestContext.Current.CancellationToken));
}
