using Ambio.Application.JobApplications.Dtos;
using Ambio.Web.Components.Dashboard.Shared;
using Ambio.Web.Components.JobApplications;
using Ambio.Web.Tests.Fixtures;

using Bunit;

namespace Ambio.Web.Tests.Components.Dashboard.Shared;

public class FollowUpCardTests : BunitContext
{
    private const int Delay = 7;

    private static readonly DateOnly Today = new(2026, 9, 28);

    [Fact]
    public void Render_ShowsTheDelayAndTheCount()
    {
        var cut = RenderCard(Sent(24), Sent(7));

        Assert.Equal("Sent 7 days ago or more, no news since", cut.Find(".card-header p").TextContent);
        Assert.Equal("2", cut.Find(".count-badge").TextContent);
    }

    [Fact]
    public void Render_ShowsEachApplicationInOrderWithTheDaysWaitedAndALinkToIt()
    {
        var (oldest, newest) = (Sent(24), Sent(1));

        var cut = RenderCard(oldest, newest);

        Assert.Equal(
            [
                (oldest.Title, oldest.CompanyName, "24 days", JobApplicationRoutes.Details(oldest.ShortId, oldest.Slug)),
                (newest.Title, newest.CompanyName, "1 day", JobApplicationRoutes.Details(newest.ShortId, newest.Slug)),
            ],
            cut.FindAll(".list-card").Select(card => (
                card.QuerySelector(".list-card-title")!.TextContent,
                card.QuerySelector(".list-card-subtitle")!.TextContent,
                card.QuerySelector(".follow-up-age")!.TextContent.Trim(),
                card.GetAttribute("href"))));
    }

    [Fact]
    public void Render_WithSpontaneousApplication_TagsItsCardOnly()
    {
        var generated = JobApplicationSummaryFactory.GetGenerator().Generate(JobApplicationSummaryFactory.IsSpontaneousRuleSet);
        var spontaneous = generated with { SentOn = Today.AddDays(-Delay) };

        var cut = RenderCard(Sent(Delay), spontaneous);

        Assert.Equal([false, true], cut.FindAll(".list-card").Select(card => card.QuerySelector(".tag-accent") is not null));
    }

    [Fact]
    public void Render_WithoutApplications_SaysThereIsNothingToFollowUp()
    {
        var cut = RenderCard();

        Assert.NotNull(cut.Find(".follow-up-empty"));
        Assert.Empty(cut.FindAll(".list-card"));
    }

    private IRenderedComponent<FollowUpCard> RenderCard(params JobApplicationSummaryDto[] followUps) =>
        Render<FollowUpCard>(parameters => parameters
            .Add(p => p.FollowUps, followUps)
            .Add(p => p.FollowUpAfterDays, Delay)
            .Add(p => p.Today, Today));

    private static JobApplicationSummaryDto Sent(int daysAgo) =>
        JobApplicationSummaryFactory.GetGenerator().Generate() with { SentOn = Today.AddDays(-daysAgo) };
}
