using Ambio.Application.Companies.Dtos;
using Ambio.Application.JobApplications;
using Ambio.Application.JobApplications.Dtos;
using Ambio.Domain.JobApplications;
using Ambio.Web.Components.JobApplications;
using Ambio.Web.Components.JobApplications.Pages;
using Ambio.Web.Components.JobApplications.Shared;
using Ambio.Web.Components.Offers;
using Ambio.Web.Tests.Fixtures;

using Bunit;
using Bunit.TestDoubles;

using Microsoft.Extensions.DependencyInjection;

using NSubstitute;

namespace Ambio.Web.Tests.Components.JobApplications.Pages;

public class JobApplicationListTests : BunitContext
{
    private const string NotSent = "Not sent";

    private const string Search = "data engineer";

    private static readonly DateOnly SentOn = new(2026, 9, 3);

    private readonly IJobApplicationService _jobApplicationService = Substitute.For<IJobApplicationService>();
    private readonly JobApplicationSummaryDto[] _applications;
    private readonly CompanySummaryDto[] _companies = [.. CompanySummaryFactory.GetGenerator().Generate(2)];

    public JobApplicationListTests()
    {
        Services.AddSingleton(_jobApplicationService);
        var factory = JobApplicationSummaryFactory.GetGenerator();
        _applications =
        [
            factory.Generate(JobApplicationSummaryFactory.IsDraftRuleSet),
            factory.Generate() with { SentOn = SentOn },
        ];
        _jobApplicationService.ListCompaniesAsync(Arg.Any<CancellationToken>()).Returns(_companies);
    }

    [Fact]
    public void Render_WithoutApplications_ShowsEmptyStateWithNewApplicationAndAddOfferLinks()
    {
        GivenApplications();

        var cut = Render<JobApplicationList>();

        Assert.NotNull(cut.Find($".empty a[href='{JobApplicationRoutes.New}']"));
        Assert.NotNull(cut.Find($".empty a[href='{OfferRoutes.New}']"));
        Assert.Empty(cut.FindAll("table"));
        Assert.Empty(cut.FindAll("form"));
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
    public void Render_WithApplications_LinksEachRowAndCardToItsPage()
    {
        GivenApplications(_applications);

        var cut = Render<JobApplicationList>();

        var expected = _applications.Select(a => JobApplicationRoutes.Details(a.ShortId, a.Slug)).ToList();
        Assert.Equal(expected, cut.FindAll("tbody a.application-title").Select(link => link.GetAttribute("href")));
        Assert.Equal(expected, cut.FindAll("a.list-card").Select(card => card.GetAttribute("href")));
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

    [Fact]
    public void Render_WithQueryString_ListsTheFirstPageMatchingItsFilter()
    {
        var company = _companies[0];
        var filter = new JobApplicationFilter(ApplicationStatus.Interview, company.Slug, ApplicationKind.Spontaneous, Search);
        GivenApplications(_applications);
        NavigateTo(JobApplicationRoutes.Filtered(filter));

        Render<JobApplicationList>();

        _jobApplicationService.Received().ListAsync(filter, JobApplicationList.PageSize, Arg.Any<CancellationToken>());
    }

    [Fact]
    public void Render_WithShown_ListsThatManyApplications()
    {
        GivenApplications(_applications);
        NavigateTo(JobApplicationRoutes.Filtered(new JobApplicationFilter(), shown: 2 * JobApplicationList.PageSize));

        Render<JobApplicationList>();

        _jobApplicationService.Received().ListAsync(new JobApplicationFilter(), 2 * JobApplicationList.PageSize, Arg.Any<CancellationToken>());
    }

    [Fact]
    public void Render_WithInvalidValues_IgnoresThem()
    {
        GivenApplications(_applications);
        NavigateTo($"{JobApplicationRoutes.List}?status=2&kind=online&company=%20&q=%20&shown=3");

        Render<JobApplicationList>();

        _jobApplicationService.Received().ListAsync(new JobApplicationFilter(), JobApplicationList.PageSize, Arg.Any<CancellationToken>());
    }

    [Fact]
    public void Render_WithFilter_ShowsItsValuesInTheFilterBar()
    {
        var company = _companies[1];
        GivenApplications(_applications);
        NavigateTo(JobApplicationRoutes.Filtered(new JobApplicationFilter(ApplicationStatus.Offer, company.Slug, ApplicationKind.Offer, Search)));

        var cut = Render<JobApplicationList>();

        var bar = cut.Find("form.application-filters");
        Assert.Equal(Search, bar.QuerySelector($"input[name='{JobApplicationRoutes.SearchQuery}']")!.GetAttribute("value"));
        Assert.Equal(
            [ApplicationStatus.Offer.ToLabel(), company.Name, ApplicationKind.Offer.ToLabel()],
            bar.QuerySelectorAll("option[selected]").Select(option => option.TextContent));
    }

    [Fact]
    public void Render_WithCompanies_OffersThemInTheCompanyFilter()
    {
        GivenApplications(_applications);

        var cut = Render<JobApplicationList>();

        Assert.Equal(
            [string.Empty, .. _companies.Select(c => c.Slug)],
            cut.FindAll($"form.application-filters select[name='{JobApplicationRoutes.CompanyQuery}'] option").Select(option => option.GetAttribute("value")));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Render_WithOrWithoutFilter_ShowsClearFiltersOnlyWithOne(bool filtered)
    {
        GivenApplications(_applications);
        NavigateTo(JobApplicationRoutes.Filtered(new JobApplicationFilter(Search: filtered ? Search : null)));

        var cut = Render<JobApplicationList>();

        Assert.Equal(filtered, cut.FindAll($"form.application-filters a[href='{JobApplicationRoutes.List}']").Count == 1);
    }

    [Fact]
    public void Render_WithFilterMatchingNothing_KeepsTheFiltersAndOffersToClearThem()
    {
        GivenApplications();
        NavigateTo(JobApplicationRoutes.Filtered(new JobApplicationFilter(Search: Search)));

        var cut = Render<JobApplicationList>();

        Assert.NotNull(cut.Find("form.application-filters"));
        Assert.NotNull(cut.Find($".empty a[href='{JobApplicationRoutes.List}']"));
        Assert.Empty(cut.FindAll($".empty a[href='{JobApplicationRoutes.New}']"));
    }

    [Fact]
    public void Render_WithMoreApplicationsThanShown_LinksLoadMoreToTheNextPageOfTheSameFilter()
    {
        var filter = new JobApplicationFilter(Status: ApplicationStatus.Applied);
        GivenList(new JobApplicationListDto(_applications, Total: 30, CountsByStatus: new Dictionary<ApplicationStatus, int>()));
        NavigateTo(JobApplicationRoutes.Filtered(filter));

        var cut = Render<JobApplicationList>();

        var footers = cut.FindAll(".list-footer");
        Assert.Equal(2, footers.Count);
        Assert.All(footers, footer =>
        {
            Assert.Equal($"Showing {_applications.Length} of 30", footer.QuerySelector("span")!.TextContent);
            Assert.Equal(
                JobApplicationRoutes.Filtered(filter, _applications.Length + JobApplicationList.PageSize),
                footer.QuerySelector("a")!.GetAttribute("href"));
        });
    }

    [Fact]
    public void Render_WithEveryApplicationShown_HasNoLoadMore()
    {
        GivenApplications(_applications);

        var cut = Render<JobApplicationList>();

        Assert.Empty(cut.FindAll(".list-footer"));
    }

    private void GivenApplications(params JobApplicationSummaryDto[] applications) =>
        GivenList(new JobApplicationListDto(applications, applications.Length, new Dictionary<ApplicationStatus, int>()));

    private void GivenList(JobApplicationListDto list) =>
        _jobApplicationService.ListAsync(Arg.Any<JobApplicationFilter>(), Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(list);

    private void NavigateTo(string url) => Services.GetRequiredService<BunitNavigationManager>().NavigateTo(url);
}
