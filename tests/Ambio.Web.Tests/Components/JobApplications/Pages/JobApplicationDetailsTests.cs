using Ambio.Application.JobApplications;
using Ambio.Application.JobApplications.Dtos;
using Ambio.Domain.JobApplications;
using Ambio.Web.Components.Companies;
using Ambio.Web.Components.JobApplications;
using Ambio.Web.Components.JobApplications.Pages;
using Ambio.Web.Components.JobApplications.Shared;
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

    private static readonly DateOnly SentOn = new(2026, 9, 3);

    private readonly IJobApplicationService _jobApplicationService = Substitute.For<IJobApplicationService>();

    public JobApplicationDetailsTests()
    {
        Services.AddSingleton(_jobApplicationService);
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
        Assert.Equal(application.Status.ToLabel(), cut.Find(".status-panel .status-badge").TextContent.Trim());
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
        Assert.Equal(OfferRoutes.Details(application.JobOfferId), cut.Find(".application-offer").GetAttribute("href"));
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
    public void Render_WithUnknownShortId_RequestsNotFound()
    {
        var notFound = false;
        Services.GetRequiredService<NavigationManager>().OnNotFound += (_, _) => notFound = true;

        var cut = RenderDetails("999999-backend-developer");

        Assert.True(notFound);
        Assert.Empty(cut.FindAll("h1"));
    }

    private void GivenApplication(JobApplicationDetailsDto application) =>
        _jobApplicationService.GetAsync(application.ShortId, Arg.Any<CancellationToken>()).Returns(application);

    private static string Key(JobApplicationDetailsDto application) => JobApplicationRoutes.Key(application.ShortId, application.Slug);

    private static IEnumerable<(string Term, string Description)> Overview(IRenderedComponent<JobApplicationDetails> cut) =>
        cut.FindAll(".application-overview-list > div").Select(row => (
            row.QuerySelector("dt")!.TextContent,
            string.Join(' ', row.QuerySelector("dd")!.TextContent.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries))));

    private IRenderedComponent<JobApplicationDetails> RenderDetails(string key) =>
        Render<JobApplicationDetails>(parameters => parameters.Add(page => page.Key, key));
}
