using Ambio.Application.JobApplications;
using Ambio.Application.JobApplications.Dtos;
using Ambio.Domain.JobApplications;
using Ambio.Web.Components.JobApplications;
using Ambio.Web.Components.JobApplications.Pages;
using Ambio.Web.Components.JobApplications.Shared;
using Ambio.Web.Components.Offers;
using Ambio.Web.Tests.Fixtures;

using Bunit;

using Microsoft.Extensions.DependencyInjection;

using NSubstitute;

namespace Ambio.Web.Tests.Components.JobApplications.Pages;

public class JobApplicationListTests : BunitContext
{
    private const string NotSent = "Not sent";

    private static readonly DateOnly SentOn = new(2026, 9, 3);

    private readonly IJobApplicationService _jobApplicationService = Substitute.For<IJobApplicationService>();
    private readonly JobApplicationSummaryDto[] _applications;

    public JobApplicationListTests()
    {
        Services.AddSingleton(_jobApplicationService);
        var factory = JobApplicationSummaryFactory.GetGenerator();
        _applications =
        [
            factory.Generate(JobApplicationSummaryFactory.IsDraftRuleSet),
            factory.Generate() with { SentOn = SentOn },
        ];
    }

    [Fact]
    public void Render_WithoutApplications_ShowsEmptyStateWithNewApplicationAndAddOfferLinks()
    {
        GivenApplications();

        var cut = Render<JobApplicationList>();

        Assert.NotNull(cut.Find($".empty a[href='{JobApplicationRoutes.New}']"));
        Assert.NotNull(cut.Find($".empty a[href='{OfferRoutes.New}']"));
        Assert.Empty(cut.FindAll("table"));
    }

    [Fact]
    public void Render_WithApplications_ShowsOneTableRowPerApplicationInServiceOrder()
    {
        GivenApplications(_applications);

        var cut = Render<JobApplicationList>();

        var rows = cut.FindAll("tbody tr").Select(row => (
            row.QuerySelector(".application-title")!.TextContent,
            row.QuerySelector(".application-company")!.TextContent,
            row.QuerySelector(".status-badge")!.TextContent.Trim(),
            row.QuerySelector(".tag")!.TextContent.Trim(),
            row.QuerySelector(".application-date")!.TextContent.Trim()));
        Assert.Equal(
            [
                (_applications[0].Title, _applications[0].CompanyName, _applications[0].Status.ToLabel(), _applications[0].Channel.ToLabel(), NotSent),
                (_applications[1].Title, _applications[1].CompanyName, _applications[1].Status.ToLabel(), _applications[1].Channel.ToLabel(), "3 Sep 2026"),
            ],
            rows);
        Assert.Empty(cut.FindAll(".empty"));
    }

    [Fact]
    public void Render_WithApplications_ShowsCardsWithCompanyDateAndStatus()
    {
        GivenApplications(_applications);

        var cut = Render<JobApplicationList>();

        var cards = cut.FindAll(".list-card").Select(card => (
            card.QuerySelector(".list-card-title")!.TextContent,
            card.QuerySelector(".list-card-subtitle")!.TextContent,
            card.QuerySelector(".list-card-aside")!.TextContent.Trim(),
            card.QuerySelector(".status-badge")!.TextContent.Trim()));
        Assert.Equal(
            [
                (_applications[0].Title, _applications[0].CompanyName, NotSent, _applications[0].Status.ToLabel()),
                (_applications[1].Title, _applications[1].CompanyName, "3 Sep", _applications[1].Status.ToLabel()),
            ],
            cards);
    }

    [Fact]
    public void Render_WithSpontaneousApplication_TagsItsRowAndCardOnly()
    {
        var spontaneous = JobApplicationSummaryFactory.GetGenerator().Generate(JobApplicationSummaryFactory.IsSpontaneousRuleSet);
        GivenApplications(_applications[1], spontaneous);

        var cut = Render<JobApplicationList>();

        Assert.Equal(
            [false, true],
            cut.FindAll("tbody tr").Select(row => row.QuerySelector(".tag-accent") is not null));
        Assert.Equal(
            [false, true],
            cut.FindAll(".list-card").Select(card => card.QuerySelector(".tag-accent") is not null));
        Assert.Equal(ApplicationKind.Spontaneous.ToLabel(), cut.Find("tbody .tag-accent").TextContent.Trim());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(2)]
    public void Render_WithOrWithoutApplications_ShowsTheNewApplicationFloatingButton(int count)
    {
        GivenApplications(_applications[..count]);

        var cut = Render<JobApplicationList>();

        var fab = cut.Find(".btn-fab");
        Assert.Equal(JobApplicationRoutes.New, fab.GetAttribute("href"));
        Assert.Equal("New application", fab.GetAttribute("aria-label"));
        Assert.Contains("mobile-only", fab.ClassList);
    }

    private void GivenApplications(params JobApplicationSummaryDto[] applications) =>
        _jobApplicationService.ListAsync(Arg.Any<CancellationToken>()).Returns(applications);
}
