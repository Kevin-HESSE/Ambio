using Ambio.Application.Common;
using Ambio.Application.CoverLetters;
using Ambio.Application.JobApplications;
using Ambio.Application.JobApplications.Dtos;
using Ambio.Domain.JobApplications;
using Ambio.Web.Components.Companies;
using Ambio.Web.Components.CoverLetters.Shared;
using Ambio.Web.Components.JobApplications;
using Ambio.Web.Components.JobApplications.Pages;
using Ambio.Web.Components.JobApplications.Shared;
using Ambio.Web.Components.Layout;
using Ambio.Web.Components.Offers;
using Ambio.Web.Components.Offers.Shared;
using Ambio.Web.Components.Shared;
using Ambio.Web.Tests.Fixtures;

using Bunit;
using Bunit.TestDoubles;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

using NSubstitute;

namespace Ambio.Web.Tests.Components.JobApplications.Pages;

public class JobApplicationDetailsTests : BunitContext
{
    private const string NotSentYet = "Not sent yet";
    private const string NotSpecified = "Not specified";
    private const string NoNotes = "No notes yet.";
    private const string ChannelTerm = "Channel";

    private const string Comment = "First call with the HR team";

    private static readonly DateOnly SentOn = new(2026, 9, 3);
    private static readonly DateTime CreatedAt = new(2026, 9, 1, 9, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime SentAt = new(2026, 9, 3, 9, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime InterviewAt = new(2026, 9, 12, 9, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime ArchivedAt = new(2026, 9, 27, 9, 0, 0, DateTimeKind.Utc);

    private readonly IJobApplicationService _jobApplicationService = Substitute.For<IJobApplicationService>();
    private readonly ICoverLetterService _coverLetterService = Substitute.For<ICoverLetterService>();
    private readonly ToastService _toasts = new();

    public JobApplicationDetailsTests()
    {
        Services.AddSingleton(_jobApplicationService);
        Services.AddSingleton(_coverLetterService);
        Services.AddSingleton(_toasts);
        // The page is static; its change status sheet is interactive once the circuit is up.
        SetRendererInfo(new RendererInfo("Static", isInteractive: false));
    }

    [Fact]
    public void Render_WithOfferApplication_ShowsTheOfferInTheHeader()
    {
        var application = OfferApplicationDetailsFactory.GetGenerator().Generate();
        GivenApplication(application);

        var cut = RenderDetails(Key(application));

        Assert.Equal(application.Title, cut.Find("h1").TextContent);
        Assert.Equal(CompanyRoutes.Details(application.CompanySlug), cut.Find(".application-company").GetAttribute("href"));
        Assert.Equal(application.Location, cut.Find(".application-location").TextContent);
        Assert.Equal(
            [application.ContractType.ToLabel(), application.RemotePolicy.ToLabel(), application.Salary],
            cut.FindAll(".detail-tags .tag").Select(tag => tag.TextContent.Trim()));
        Assert.Equal(application.Status.ToLabel(), cut.Find(".status-panel-status .status-badge").TextContent.Trim());
    }

    [Fact]
    public void Render_WithSentOfferApplication_ShowsTheWholeOverview()
    {
        var application = OfferApplicationDetailsFactory.GetGenerator().Generate() with { SentOn = SentOn };
        GivenApplication(application);

        var cut = RenderDetails(Key(application));

        Assert.Equal(
            [
                ("Offer", $"{application.Title} · {application.CompanyName}"),
                ("Applied via", application.AppliedViaSite!),
                ("Sent on", "3 Sep 2026"),
                ("Short ID", application.ShortId),
                ("Created", application.CreatedAt.ToDisplayDate()),
            ],
            Overview(cut).Where(row => row.Term != ChannelTerm));
        Assert.Equal(
            (application.Channel.ToLabel(), application.ChannelDetail),
            (cut.Find(".application-channel .tag").TextContent.Trim(), cut.Find(".application-channel-detail").TextContent));
        Assert.Equal(OfferRoutes.Details(application.JobOfferShortId, application.JobOfferSlug), cut.Find(".application-offer").GetAttribute("href"));
        Assert.NotNull(cut.Find($".application-via a[href='{application.AppliedViaUrl}'][target='_blank']"));
        Assert.Equal(application.Notes, cut.Find(".application-notes-text").TextContent);
    }

    [Fact]
    public void Render_WithDraft_SaysItIsNotSentWithoutPostingNorNotes()
    {
        var application = OfferApplicationDetailsFactory.GetGenerator().Generate(OfferApplicationDetailsFactory.IsDraftRuleSet);
        GivenApplication(application);

        var cut = RenderDetails(Key(application));

        var overview = Overview(cut).ToDictionary();
        Assert.Equal((NotSpecified, NotSentYet), (overview["Applied via"], overview["Sent on"]));
        Assert.Equal(NoNotes, cut.Find(".application-notes-empty").TextContent);
    }

    [Fact]
    public void Render_WithSpontaneousApplication_ShowsItsCompanyAndTag()
    {
        var application = SpontaneousApplicationDetailsFactory.GetGenerator().Generate();
        GivenApplication(application);

        var cut = RenderDetails(Key(application));

        Assert.Equal(application.Title, cut.Find("h1").TextContent);
        Assert.Equal(application.CompanyLocation, cut.Find(".application-location").TextContent);
        Assert.Equal(ApplicationKind.Spontaneous.ToLabel(), cut.Find(".detail-tags .tag-accent").TextContent.Trim());
        Assert.Equal(
            ["Company", "Channel", "Sent on", "Short ID", "Created"],
            Overview(cut).Select(row => row.Term));
        Assert.Equal(
            CompanyRoutes.Details(application.CompanySlug),
            cut.Find(".application-overview-list a").GetAttribute("href"));
    }

    [Fact]
    public void Render_WithStatusChanges_ListsThemNewestFirstThenTheCreationAsDraft()
    {
        var application = OfferApplicationDetailsFactory.GetGenerator().Generate() with
        {
            Status = ApplicationStatus.Interview,
            CreatedAt = CreatedAt,
            StatusHistory =
            [
                new StatusChangeDto(ApplicationStatus.Applied, ApplicationStatus.Interview, InterviewAt, Comment),
                new StatusChangeDto(ApplicationStatus.Draft, ApplicationStatus.Applied, SentAt, null),
            ],
        };
        GivenApplication(application);

        var cut = RenderDetails(Key(application));

        Assert.Equal(
            [
                $"{ApplicationStatus.Interview.ToLabel()} 12 Sep 2026 {Comment}",
                $"{ApplicationStatus.Applied.ToLabel()} 3 Sep 2026",
                $"{ApplicationStatus.Draft.ToLabel()} 1 Sep 2026",
            ],
            cut.FindAll(".status-timeline-item").Select(TimelineItem));
        Assert.Equal("since 12 Sep", Normalize(cut.Find(".status-panel-since").TextContent));
    }

    [Fact]
    public void Render_WithoutStatusChange_IsADraftSinceItsCreation()
    {
        var application = OfferApplicationDetailsFactory.GetGenerator().Generate(OfferApplicationDetailsFactory.IsDraftRuleSet) with { CreatedAt = CreatedAt };
        GivenApplication(application);

        var cut = RenderDetails(Key(application));

        Assert.Equal($"{ApplicationStatus.Draft.ToLabel()} 1 Sep 2026", TimelineItem(cut.Find(".status-timeline-item")));
        Assert.Equal("since 1 Sep", Normalize(cut.Find(".status-panel-since").TextContent));
    }

    [Theory]
    [InlineData(ApplicationStatus.Draft, true)]
    [InlineData(ApplicationStatus.Rejected, false)]
    public void Render_WithStatus_OffersToChangeItUnlessItIsFinal(ApplicationStatus status, bool canChange)
    {
        var application = OfferApplicationDetailsFactory.GetGenerator().Generate() with { Status = status };
        GivenApplication(application);

        var cut = RenderDetails(Key(application));

        Assert.Equal(canChange, cut.FindAll(".status-panel .change-status-open").Count == 1);
    }

    [Fact]
    public void Render_WithApplication_LinksTheEditButtonsToTheEditPage()
    {
        var application = OfferApplicationDetailsFactory.GetGenerator().Generate();
        GivenApplication(application);

        var cut = RenderDetails(Key(application));

        var editHref = JobApplicationRoutes.Edit(application.ShortId, application.Slug);
        Assert.Equal(editHref, cut.Find(".detail-actions a").GetAttribute("href"));
        Assert.Equal(editHref, cut.Find("a.application-notes-edit").GetAttribute("href"));
    }

    [Fact]
    public void Render_WithCanonicalKey_StaysOnThePage()
    {
        var application = OfferApplicationDetailsFactory.GetGenerator().Generate();
        GivenApplication(application);
        var navigation = Services.GetRequiredService<BunitNavigationManager>();

        RenderDetails(Key(application));

        Assert.Empty(navigation.History);
    }

    [Theory]
    [InlineData("-old-title")]
    [InlineData("")]
    public void Render_WithAnotherSlug_RedirectsToTheCanonicalUrl(string slugPart)
    {
        var application = OfferApplicationDetailsFactory.GetGenerator().Generate();
        GivenApplication(application);
        var navigation = Services.GetRequiredService<BunitNavigationManager>();

        RenderDetails($"{application.ShortId}{slugPart}");

        Assert.EndsWith($"/{JobApplicationRoutes.Details(application.ShortId, application.Slug)}", navigation.Uri);
    }

    [Fact]
    public void Render_WithoutTab_ShowsTheOverviewAndCountsTheLetters()
    {
        var application = OfferApplicationDetailsFactory.GetGenerator().Generate();
        GivenApplication(application);
        _coverLetterService.ListAsync(application.Id, Arg.Any<CancellationToken>()).Returns(CoverLetterFactory.GetGenerator().Generate(2));

        var cut = RenderDetails(Key(application));

        Assert.Equal(
            JobApplicationRoutes.Details(application.ShortId, application.Slug, JobApplicationRoutes.OverviewTab),
            cut.Find("nav.tabs a.tab.active").GetAttribute("href"));
        var letters = cut.Find($"nav.tabs a.tab[href='{JobApplicationRoutes.Details(application.ShortId, application.Slug, JobApplicationRoutes.LettersTab)}']");
        Assert.Equal("2", letters.QuerySelector(".tab-count")!.TextContent);
        Assert.NotNull(cut.Find(".application-overview"));
        Assert.Empty(cut.FindComponents<CoverLetterEditor>());
        Assert.DoesNotContain("desktop-only", cut.Find(".application-detail-history").ClassList);
    }

    [Fact]
    public void Render_OnLettersTab_ShowsTheLettersAndKeepsTheHistoryForDesktop()
    {
        var application = OfferApplicationDetailsFactory.GetGenerator().Generate();
        GivenApplication(application);
        var letters = CoverLetterFactory.GetGenerator().Generate(2);
        _coverLetterService.ListAsync(application.Id, Arg.Any<CancellationToken>()).Returns(letters);
        NavigateToTab(application, JobApplicationRoutes.LettersTab);

        var cut = RenderDetails(Key(application));

        var editor = cut.FindComponent<CoverLetterEditor>();
        Assert.Equal((application.Id, letters), (editor.Instance.ApplicationId, editor.Instance.Letters));
        Assert.Empty(cut.FindAll(".application-overview"));
        Assert.Contains("desktop-only", cut.Find(".application-detail-history").ClassList);
    }

    [Fact]
    public void Render_WithAnotherSlugOnLettersTab_RedirectsToTheCanonicalUrlOnTheSameTab()
    {
        var application = OfferApplicationDetailsFactory.GetGenerator().Generate();
        GivenApplication(application);
        var navigation = Services.GetRequiredService<BunitNavigationManager>();
        navigation.NavigateTo(JobApplicationRoutes.Details(application.ShortId, "old-title", JobApplicationRoutes.LettersTab));

        RenderDetails($"{application.ShortId}-old-title");

        Assert.EndsWith($"/{JobApplicationRoutes.Details(application.ShortId, application.Slug, JobApplicationRoutes.LettersTab)}", navigation.Uri);
    }

    [Fact]
    public void Render_WithUnknownShortId_RequestsNotFound()
    {
        var notFound = false;
        Services.GetRequiredService<NavigationManager>().OnNotFound += (_, _) => notFound = true;

        var cut = RenderDetails("999999-backend-developer");

        Assert.True(notFound);
        Assert.Empty(cut.FindAll("h1"));
    }

    [Fact]
    public void Render_ActiveApplication_OffersToArchiveItFromTheMenuWithoutBanner()
    {
        var application = OfferApplicationDetailsFactory.GetGenerator().Generate();
        GivenApplication(application);

        var cut = RenderDetails(Key(application));

        Assert.Equal(
            [("Archive", ArchiveForms.ArchiveFormId), ("Archive", ArchiveForms.ArchiveFormId)],
            cut.FindAll(".menu-item").Select(item => (item.TextContent.Trim(), item.GetAttribute("form"))));
        Assert.Empty(cut.FindAll(".archived-banner, .detail-tags .tag-archived"));
    }

    [Fact]
    public void Render_ArchivedApplication_ShowsTheBannerTheTagAndRestoreInsteadOfChangingItsStatus()
    {
        var application = OfferApplicationDetailsFactory.GetGenerator().Generate() with { Status = ApplicationStatus.Applied, ArchivedAt = ArchivedAt };
        GivenApplication(application);

        var cut = RenderDetails(Key(application));

        Assert.Contains("Archived on 27 Sep 2026.", cut.Find(".archived-banner").TextContent);
        Assert.NotNull(cut.Find(".detail-tags .tag-archived"));
        Assert.All(cut.FindAll(".menu-item"), item => Assert.Equal(("Restore", ArchiveForms.RestoreFormId), (item.TextContent.Trim(), item.GetAttribute("form"))));
        Assert.Equal(ArchiveForms.RestoreFormId, cut.Find(".status-panel .status-panel-restore").GetAttribute("form"));
        Assert.Empty(cut.FindAll(".change-status-open"));
    }

    [Fact]
    public void SubmitArchive_ArchivesShowsTheUndoToastAndReloadsTheApplication()
    {
        var application = OfferApplicationDetailsFactory.GetGenerator().Generate();
        _jobApplicationService.GetAsync(application.ShortId, Arg.Any<CancellationToken>()).Returns(application, application with { ArchivedAt = ArchivedAt });
        _jobApplicationService.ArchiveAsync(application.Id, Arg.Any<CancellationToken>()).Returns(ServiceResult.Success());
        var cut = RenderDetails(Key(application));

        cut.Find($"#{ArchiveForms.ArchiveFormId}").Submit();

        _jobApplicationService.Received().ArchiveAsync(application.Id, Arg.Any<CancellationToken>());
        Assert.Equal(ArchiveForms.UndoToast("Application archived"), _toasts.Current);
        Assert.NotNull(cut.Find(".archived-banner"));
    }

    [Fact]
    public void SubmitRestore_RestoresAndReloadsTheApplicationWithoutToast()
    {
        var application = OfferApplicationDetailsFactory.GetGenerator().Generate() with { ArchivedAt = ArchivedAt };
        _jobApplicationService.GetAsync(application.ShortId, Arg.Any<CancellationToken>()).Returns(application, application with { ArchivedAt = null });
        _jobApplicationService.RestoreAsync(application.Id, Arg.Any<CancellationToken>()).Returns(ServiceResult.Success());
        var cut = RenderDetails(Key(application));

        cut.Find($"#{ArchiveForms.RestoreFormId}").Submit();

        _jobApplicationService.Received().RestoreAsync(application.Id, Arg.Any<CancellationToken>());
        Assert.Null(_toasts.Current);
        Assert.Empty(cut.FindAll(".archived-banner"));
    }

    [Fact]
    public void SubmitArchive_ApplicationGone_RequestsNotFound()
    {
        var application = OfferApplicationDetailsFactory.GetGenerator().Generate();
        GivenApplication(application);
        _jobApplicationService.ArchiveAsync(application.Id, Arg.Any<CancellationToken>()).Returns(ServiceResult.Failure(JobApplicationErrors.NotFound));
        var notFound = false;
        Services.GetRequiredService<NavigationManager>().OnNotFound += (_, _) => notFound = true;
        var cut = RenderDetails(Key(application));

        cut.Find($"#{ArchiveForms.ArchiveFormId}").Submit();

        Assert.True(notFound);
        Assert.Null(_toasts.Current);
    }

    private void GivenApplication(JobApplicationDetailsDto application) =>
        _jobApplicationService.GetAsync(application.ShortId, Arg.Any<CancellationToken>()).Returns(application);

    private void NavigateToTab(JobApplicationDetailsDto application, string tab) =>
        Services.GetRequiredService<BunitNavigationManager>().NavigateTo(JobApplicationRoutes.Details(application.ShortId, application.Slug, tab));

    private static string Key(JobApplicationDetailsDto application) => ShortIdKey.Of(application.ShortId, application.Slug);

    private static IEnumerable<(string Term, string Description)> Overview(IRenderedComponent<JobApplicationDetails> cut) =>
        cut.FindAll(".application-overview-list > div").Select(row => (
            row.QuerySelector("dt")!.TextContent,
            Normalize(row.QuerySelector("dd")!.TextContent)));

    // The badge, the date and the comment of a history entry.
    private static string TimelineItem(AngleSharp.Dom.IElement item) =>
        string.Join(' ', item.Children.Select(part => Normalize(part.TextContent)));

    // The text of an element, its whitespace collapsed.
    private static string Normalize(string text) => string.Join(' ', text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    private IRenderedComponent<JobApplicationDetails> RenderDetails(string key) =>
        Render<JobApplicationDetails>(parameters => parameters.Add(page => page.Key, key));
}
