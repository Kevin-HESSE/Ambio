using Ambio.Application.JobApplications.Dtos;
using Ambio.Domain.JobApplications;
using Ambio.Web.Components.Dashboard.Shared;
using Ambio.Web.Components.JobApplications;

using Bunit;

namespace Ambio.Web.Tests.Components.Dashboard.Shared;

public class PipelineCardTests : BunitContext
{
    private static readonly Dictionary<ApplicationStatus, int> Counts = new()
    {
        [ApplicationStatus.Draft] = 2,
        [ApplicationStatus.Applied] = 9,
        [ApplicationStatus.Interview] = 0,
        [ApplicationStatus.Offer] = 1,
        [ApplicationStatus.Rejected] = 0,
        [ApplicationStatus.Withdrawn] = 0,
    };

    [Fact]
    public void Render_ShowsTheTotalOfActiveApplications()
    {
        var cut = RenderCard(Counts);

        Assert.Equal("12 active applications", cut.Find(".pipeline-total").TextContent);
    }

    [Fact]
    public void Render_WithOneApplication_SaysApplicationInTheSingular()
    {
        var cut = RenderCard(Enum.GetValues<ApplicationStatus>().ToDictionary(s => s, s => s == ApplicationStatus.Draft ? 1 : 0));

        Assert.Equal("1 active application", cut.Find(".pipeline-total").TextContent);
    }

    [Fact]
    public void Render_ShowsASegmentPerStatusWithApplications_AsWideAsItsCount()
    {
        var cut = RenderCard(Counts);

        Assert.Equal(
            [("pipeline-segment status-draft", "flex-grow: 2"), ("pipeline-segment status-applied", "flex-grow: 9"), ("pipeline-segment status-offer", "flex-grow: 1")],
            cut.FindAll(".pipeline-segment").Select(segment => (segment.ClassName, segment.GetAttribute("style"))));
    }

    [Fact]
    public void Render_LinksACounterPerStatusToTheListFilteredOnIt()
    {
        var cut = RenderCard(Counts);

        var counters = cut.FindAll("a.pipeline-stat").Select(counter => (
            counter.GetAttribute("href"),
            counter.QuerySelector(".pipeline-stat-value")!.TextContent));
        Assert.Equal(
            Enum.GetValues<ApplicationStatus>().Select(s => ((string?)JobApplicationRoutes.Filtered(new JobApplicationFilter(Status: s)), Counts[s].ToString())),
            counters);
    }

    private IRenderedComponent<PipelineCard> RenderCard(IReadOnlyDictionary<ApplicationStatus, int> counts) =>
        Render<PipelineCard>(parameters => parameters.Add(p => p.CountsByStatus, counts));
}
