using Ambio.Application.Common;
using Ambio.Application.JobApplications;
using Ambio.Application.Offers;
using Ambio.Application.Offers.Dtos;
using Ambio.Web.Components.Companies;
using Ambio.Web.Components.JobApplications;
using Ambio.Web.Components.JobApplications.Shared;
using Ambio.Web.Components.Layout;
using Ambio.Web.Components.Offers;
using Ambio.Web.Components.Offers.Pages;
using Ambio.Web.Components.Offers.Shared;
using Ambio.Web.Components.Shared;
using Ambio.Web.Tests.Fixtures;

using Bogus;

using Bunit;
using Bunit.TestDoubles;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

using NSubstitute;

namespace Ambio.Web.Tests.Components.Offers.Pages;

public class OfferDetailsTests : BunitContext
{
    private static readonly DateTime Published = new(2026, 8, 28, 9, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime FirstSeen = new(2026, 8, 28, 9, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime LastSeen = new(2026, 9, 2, 9, 0, 0, DateTimeKind.Utc);

    private static readonly DateOnly SentOn = new(2026, 9, 3);

    private readonly IJobOfferService _jobOfferService = Substitute.For<IJobOfferService>();
    private readonly IJobApplicationService _jobApplicationService = Substitute.For<IJobApplicationService>();
    private readonly ToastService _toasts = new();
    private readonly Faker<JobOfferDetailsDto> _jobOfferDetailsFactory;

    public OfferDetailsTests()
    {
        Services.AddSingleton(_jobOfferService);
        Services.AddSingleton(_jobApplicationService);
        Services.AddSingleton(_toasts);
        _jobApplicationService.ListByOfferAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns([]);
        // The page is static; its postings card is interactive once the circuit is up.
        SetRendererInfo(new RendererInfo("Static", isInteractive: false));
        _jobOfferDetailsFactory = JobOfferDetailsFactory.GetGenerator();
    }

    [Fact]
    public void Render_WithOffer_ShowsTitleCompanyLocationAndPublicationDate()
    {
        var offer = _jobOfferDetailsFactory.Generate() with { RegisterAt = Published };
        GivenOffer(offer);

        var cut = RenderDetails(Key(offer));

        Assert.Equal(offer.Title, cut.Find("h1").TextContent);
        Assert.Equal(
            $"{offer.CompanyName} · {offer.Location} · Published 28 Aug 2026",
            cut.Find(".detail-subtitle").TextContent.Trim());
        Assert.Equal(offer.CompanyName, cut.Find($".detail-subtitle a[href='{CompanyRoutes.Details(offer.CompanySlug)}']").TextContent);
    }

    [Fact]
    public void Render_WithOffer_ShowsContractRemotePolicyAndSalaryTags()
    {
        var offer = _jobOfferDetailsFactory.Generate();
        GivenOffer(offer);

        var cut = RenderDetails(Key(offer));

        Assert.Equal(
            [offer.ContractType.ToLabel(), offer.RemotePolicy.ToLabel(), offer.Salary!],
            cut.FindAll(".detail-tags .tag").Select(tag => tag.TextContent.Trim()));
    }

    [Fact]
    public void Render_WithOnlyRequiredFields_LeavesOutLocationSalaryAndAdText()
    {
        var offer = _jobOfferDetailsFactory.Generate(JobOfferDetailsFactory.RequiredOnlyRuleSet);
        GivenOffer(offer);

        var cut = RenderDetails(Key(offer));

        Assert.Empty(cut.FindAll(".offer-location"));
        Assert.Equal(2, cut.FindAll(".detail-tags .tag").Count);
        Assert.Equal("No copy of the ad yet.", cut.Find(".offer-description-empty").TextContent);
    }

    [Fact]
    public void Render_WithDescription_ShowsIt()
    {
        var offer = _jobOfferDetailsFactory.Generate();
        GivenOffer(offer);

        var cut = RenderDetails(Key(offer));

        Assert.Equal(offer.Description, cut.Find(".offer-description").TextContent);
    }

    [Fact]
    public void Render_WithPostings_ListsThemWithSiteLinkAndDaySeen()
    {
        var offer = _jobOfferDetailsFactory.Generate();
        offer = offer with
        {
            Postings = [offer.Postings[0] with { SeenAt = FirstSeen }, offer.Postings[1] with { SeenAt = LastSeen }],
        };
        GivenOffer(offer);

        var cut = RenderDetails(Key(offer));

        var postings = cut.FindAll(".offer-posting").Select(posting => (
            posting.QuerySelector(".offer-posting-site")!.TextContent,
            posting.QuerySelector("a[target='_blank']")!.GetAttribute("href"),
            posting.QuerySelector(".offer-posting-date")!.TextContent));
        Assert.Equal(
            [
                (offer.Postings[0].Site, offer.Postings[0].Url, "28 Aug"),
                (offer.Postings[1].Site, offer.Postings[1].Url, "2 Sep"),
            ],
            postings);
        Assert.Equal("2", cut.Find(".offer-postings-count").TextContent);
    }

    [Fact]
    public void Render_WithOffer_BreadcrumbLinksToList()
    {
        var offer = _jobOfferDetailsFactory.Generate();
        GivenOffer(offer);

        var cut = RenderDetails(Key(offer));

        Assert.Equal("Offers", cut.Find($".detail-breadcrumb a[href='{OfferRoutes.List}']").TextContent);
    }

    [Fact]
    public void Render_WithOffer_LinksToEditPage()
    {
        var offer = _jobOfferDetailsFactory.Generate();
        GivenOffer(offer);

        var cut = RenderDetails(Key(offer));

        Assert.Equal("Edit", cut.Find($".detail-actions a[href='{OfferRoutes.Edit(offer.ShortId, offer.Slug)}']").TextContent.Trim());
    }

    [Theory]
    [InlineData("-old-title")]
    [InlineData("")]
    public void Render_WithAnotherSlug_RedirectsToTheCanonicalUrl(string slugPart)
    {
        var offer = _jobOfferDetailsFactory.Generate();
        GivenOffer(offer);
        var navigation = Services.GetRequiredService<BunitNavigationManager>();

        RenderDetails($"{offer.ShortId}{slugPart}");

        Assert.EndsWith($"/{OfferRoutes.Details(offer.ShortId, offer.Slug)}", navigation.Uri);
    }

    [Fact]
    public void Render_WithUnknownShortId_RequestsNotFound()
    {
        var notFound = false;
        Services.GetRequiredService<NavigationManager>().OnNotFound += (_, _) => notFound = true;

        var cut = RenderDetails("999999-backend-developer-kelvio");

        Assert.True(notFound);
        Assert.Empty(cut.FindAll("h1"));
    }

    [Fact]
    public void Render_WithApplications_ListsThemWithTheirStatusAndSending()
    {
        var offer = _jobOfferDetailsFactory.Generate();
        var factory = JobApplicationSummaryFactory.GetGenerator();
        var sent = factory.Generate() with { SentOn = SentOn };
        var draft = factory.Generate(JobApplicationSummaryFactory.IsDraftRuleSet);
        GivenOffer(offer);
        _jobApplicationService.ListByOfferAsync(offer.Id, Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns([sent, draft]);

        var cut = RenderDetails(Key(offer));

        Assert.Equal("2", cut.Find(".offer-applications-count").TextContent);
        var items = cut.FindAll(".offer-applications .application-item").Select(item => (
            item.QuerySelector(".status-badge")!.TextContent.Trim(),
            item.QuerySelector(".application-item-meta")!.TextContent.Trim()));
        Assert.Equal(
            [
                (sent.Status.ToLabel(), $"Sent 3 Sep 2026 via {sent.AppliedVia}"),
                (draft.Status.ToLabel(), "Not sent yet"),
            ],
            items);
        Assert.Empty(cut.FindAll(".offer-applications .application-item-title"));
        Assert.Equal(
            [JobApplicationRoutes.Details(sent.ShortId, sent.Slug), JobApplicationRoutes.Details(draft.ShortId, draft.Slug)],
            cut.FindAll(".offer-applications a.application-item").Select(item => item.GetAttribute("href")));
    }

    [Fact]
    public void Render_WithoutApplications_SaysSoAndLinksToApply()
    {
        var offer = _jobOfferDetailsFactory.Generate();
        GivenOffer(offer);

        var cut = RenderDetails(Key(offer));

        Assert.Equal("0", cut.Find(".offer-applications-count").TextContent);
        Assert.Equal("No application yet.", cut.Find(".offer-applications-empty").TextContent);
        Assert.Equal(JobApplicationRoutes.NewForOffer(offer.ShortId), cut.Find("a.offer-applications-new").GetAttribute("href"));
    }

    [Fact]
    public void Render_ArchivedOffer_ShowsTheBannerAndListsItsArchivedApplicationsToo()
    {
        var offer = _jobOfferDetailsFactory.Generate() with { ArchivedAt = Published };
        GivenOffer(offer);

        var cut = RenderDetails(Key(offer));

        Assert.NotNull(cut.Find(".archived-banner"));
        Assert.All(cut.FindAll(".menu-item"), item => Assert.Equal(ArchiveForms.RestoreFormId, item.GetAttribute("form")));
        _jobApplicationService.Received().ListByOfferAsync(offer.Id, true, Arg.Any<CancellationToken>());
    }

    [Fact]
    public void Render_ActiveOffer_ListsItsActiveApplicationsOnly()
    {
        var offer = _jobOfferDetailsFactory.Generate();
        GivenOffer(offer);

        var cut = RenderDetails(Key(offer));

        Assert.Empty(cut.FindAll(".archived-banner"));
        _jobApplicationService.Received().ListByOfferAsync(offer.Id, false, Arg.Any<CancellationToken>());
    }

    [Fact]
    public void SubmitArchive_ArchivesShowsTheUndoToastAndListsTheArchivedApplications()
    {
        var offer = _jobOfferDetailsFactory.Generate();
        _jobOfferService.GetAsync(offer.ShortId, Arg.Any<CancellationToken>()).Returns(offer, offer with { ArchivedAt = Published });
        _jobOfferService.ArchiveAsync(offer.Id, Arg.Any<CancellationToken>()).Returns(ServiceResult.Success());
        var cut = RenderDetails(Key(offer));

        cut.Find($"#{ArchiveForms.ArchiveFormId}").Submit();

        Assert.Equal(ArchiveForms.UndoToast("Offer archived"), _toasts.Current);
        Assert.NotNull(cut.Find(".archived-banner"));
        _jobApplicationService.Received().ListByOfferAsync(offer.Id, true, Arg.Any<CancellationToken>());
    }

    private void GivenOffer(JobOfferDetailsDto offer) =>
        _jobOfferService.GetAsync(offer.ShortId, Arg.Any<CancellationToken>()).Returns(offer);

    private static string Key(JobOfferDetailsDto offer) => ShortIdKey.Of(offer.ShortId, offer.Slug);

    private IRenderedComponent<OfferDetails> RenderDetails(string key) =>
        Render<OfferDetails>(parameters => parameters.Add(page => page.Key, key));
}
