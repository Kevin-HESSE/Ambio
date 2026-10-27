using Ambio.Application.Dashboard.Dtos;
using Ambio.Domain.JobApplications;
using Ambio.Web.Components.Dashboard.Shared;
using Ambio.Web.Components.JobApplications;
using Ambio.Web.Tests.Fixtures;

using Bunit;

namespace Ambio.Web.Tests.Components.Dashboard.Shared;

public class ActivityFeedTests : BunitContext
{
    private static readonly DateTime OccurredAt = new(2026, 9, 22, 14, 0, 0, DateTimeKind.Utc);

    [Theory]
    [MemberData(nameof(Sentences))]
    public void Render_DescribesTheEventByTheStatusReached(ApplicationStatus status, string sentence)
    {
        var activity = ActivityFactory.GetGenerator().Generate() with { Status = status, OccurredAt = OccurredAt };

        var cut = RenderFeed(activity);

        var text = cut.Find(".activity-text");
        Assert.StartsWith($"{activity.Application.Title} · {activity.Application.CompanyName} {sentence}", NormalizeSpaces(text.TextContent));
        Assert.Equal("22 Sep", text.QuerySelector("time")!.TextContent);
        Assert.Contains($"status-{status.ToString().ToLowerInvariant()}", cut.Find(".activity-item").ClassList);
    }

    [Fact]
    public void Render_LinksEachTitleToItsApplicationInOrder()
    {
        var activity = ActivityFactory.GetGenerator().Generate(2);

        var cut = RenderFeed([.. activity]);

        Assert.Equal(
            activity.Select(a => JobApplicationRoutes.Details(a.Application.ShortId, a.Application.Slug)),
            cut.FindAll(".activity-text a").Select(link => link.GetAttribute("href")));
    }

    private IRenderedComponent<ActivityFeed> RenderFeed(params ActivityDto[] activity) =>
        Render<ActivityFeed>(parameters => parameters.Add(p => p.Activity, activity));

    private static string NormalizeSpaces(string text) => string.Join(' ', text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    public static TheoryData<ApplicationStatus, string> Sentences => new()
    {
        { ApplicationStatus.Draft, "was created as a draft" },
        { ApplicationStatus.Applied, "was sent" },
        { ApplicationStatus.Interview, "moved to Interview" },
        { ApplicationStatus.Offer, "moved to Offer received" },
    };
}
