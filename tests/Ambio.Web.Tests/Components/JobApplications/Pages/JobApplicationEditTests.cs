using Ambio.Application.Common;
using Ambio.Application.JobApplications;
using Ambio.Application.JobApplications.Dtos;
using Ambio.Application.Offers;
using Ambio.Application.Offers.Dtos;
using Ambio.Domain.JobApplications;
using Ambio.Web.Components.JobApplications;
using Ambio.Web.Components.JobApplications.Pages;
using Ambio.Web.Components.Shared;
using Ambio.Web.Tests.Fixtures;

using Bunit;
using Bunit.TestDoubles;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

using NSubstitute;

namespace Ambio.Web.Tests.Components.JobApplications.Pages;

public class JobApplicationEditTests : BunitContext
{
    private const string NewPosition = "Platform Engineer";
    private const string NewNotes = "Called back on Monday.";
    private const string NewSentOnText = "2026-09-02";
    private const string PositionError = "Enter the position you apply for.";

    private static readonly DateOnly NewSentOn = new(2026, 9, 2);
    private static readonly JobApplicationLinkDto Updated = new("482913", "platform-engineer-acme");

    private readonly IJobApplicationService _jobApplicationService = Substitute.For<IJobApplicationService>();
    private readonly IJobOfferService _jobOfferService = Substitute.For<IJobOfferService>();
    private readonly JobOfferDetailsDto _offer;
    private readonly OfferApplicationDetailsDto _application;

    public JobApplicationEditTests()
    {
        Services.AddSingleton(_jobApplicationService);
        Services.AddSingleton(_jobOfferService);
        _offer = JobOfferDetailsFactory.GetGenerator().Generate();
        _application = OfferApplicationDetailsFactory.GetGenerator().Generate() with
        {
            JobOfferId = _offer.Id,
            JobOfferShortId = _offer.ShortId,
            AppliedViaPostingId = _offer.Postings[0].Id,
        };
        _jobOfferService.GetAsync(_offer.ShortId, Arg.Any<CancellationToken>()).Returns(_offer);
        GivenApplication(_application);
        _jobApplicationService.UpdateAsync(Arg.Any<Guid>(), Arg.Any<OfferApplicationInput>(), Arg.Any<CancellationToken>())
            .Returns(ServiceResult<JobApplicationLinkDto>.Success(Updated));
        _jobApplicationService.UpdateAsync(Arg.Any<Guid>(), Arg.Any<SpontaneousApplicationInput>(), Arg.Any<CancellationToken>())
            .Returns(ServiceResult<JobApplicationLinkDto>.Success(Updated));
    }

    [Fact]
    public void Render_WithOfferApplication_PrefillsTheFormWithTheOfferPostings()
    {
        var cut = RenderEdit(Key(_application));

        Assert.Equal(_offer.Postings[0].Id.ToString(), cut.Find("#application-via").GetAttribute("value"));
        Assert.Equal(_offer.Postings.Count + 1, cut.FindAll("#application-via option").Count);
        Assert.Equal(_application.Channel.ToString(), cut.Find("#application-channel").GetAttribute("value"));
        Assert.Equal(_application.ChannelDetail, cut.Find("#application-channel-detail").GetAttribute("value"));
        Assert.Equal(_application.SentOn!.Value.ToString("yyyy-MM-dd"), cut.Find("#application-sent-on").GetAttribute("value"));
        Assert.Equal(_application.Notes, cut.Find("#application-notes").GetAttribute("value"));
        Assert.Empty(cut.FindAll("#application-position"));
        Assert.Contains(_application.CompanyName, cut.Find(".page-header").TextContent);
    }

    [Fact]
    public void Render_WithDraft_HasNoSendingDay()
    {
        var draft = OfferApplicationDetailsFactory.GetGenerator().Generate(OfferApplicationDetailsFactory.IsDraftRuleSet);
        GivenApplication(draft);

        var cut = RenderEdit(Key(draft));

        Assert.Empty(cut.FindAll("#application-sent-on"));
        Assert.Empty(cut.FindAll("#application-already-sent"));
    }

    [Fact]
    public void Render_WithSpontaneousApplication_PrefillsTheTargetPositionWithoutPosting()
    {
        var application = SpontaneousApplicationDetailsFactory.GetGenerator().Generate();
        GivenApplication(application);

        var cut = RenderEdit(Key(application));

        Assert.Equal(application.Title, cut.Find("#application-position").GetAttribute("value"));
        Assert.Empty(cut.FindAll("#application-via"));
    }

    [Fact]
    public void Render_WithAnotherSlug_RedirectsToTheCanonicalUrl()
    {
        var navigation = Services.GetRequiredService<BunitNavigationManager>();

        RenderEdit($"{_application.ShortId}-old-title");

        Assert.EndsWith($"/{JobApplicationRoutes.Edit(_application.ShortId, _application.Slug)}", navigation.Uri);
    }

    [Fact]
    public void Submit_OfferApplication_UpdatesItAndOpensItsPage()
    {
        var cut = RenderEdit(Key(_application));

        cut.Find("#application-via").Change("");
        cut.Find("#application-channel").Change(nameof(Channel.Referral));
        cut.Find("#application-sent-on").Change(NewSentOnText);
        cut.Find("#application-notes").Change(NewNotes);
        cut.Find("form").Submit();

        _jobApplicationService.Received(1).UpdateAsync(
            _application.Id,
            Arg.Is<OfferApplicationInput>(input =>
                input.AppliedViaPostingId == null
                && input.Channel == Channel.Referral
                && input.SentOn == NewSentOn
                && input.Notes == NewNotes
                && input.ChannelDetail == _application.ChannelDetail),
            Arg.Any<CancellationToken>());
        Assert.EndsWith($"/{JobApplicationRoutes.Details(Updated.ShortId, Updated.Slug)}", Services.GetRequiredService<NavigationManager>().Uri);
    }

    [Fact]
    public void Submit_SpontaneousApplication_UpdatesItsTargetPosition()
    {
        var application = SpontaneousApplicationDetailsFactory.GetGenerator().Generate();
        GivenApplication(application);
        var cut = RenderEdit(Key(application));

        cut.Find("#application-position").Change(NewPosition);
        cut.Find("form").Submit();

        _jobApplicationService.Received(1).UpdateAsync(
            application.Id,
            Arg.Is<SpontaneousApplicationInput>(input => input.TargetPosition == NewPosition),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public void Submit_SentWithoutSendingDay_ShowsTheErrorWithoutUpdating()
    {
        var cut = RenderEdit(Key(_application));

        cut.Find("#application-sent-on").Change("");
        cut.Find("form").Submit();

        Assert.Equal(JobApplicationInput.SentOnRequired, cut.Find("#application-sent-on + .validation-message").TextContent);
        _jobApplicationService.DidNotReceive().UpdateAsync(Arg.Any<Guid>(), Arg.Any<OfferApplicationInput>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public void Submit_SpontaneousWithoutPosition_ShowsTheErrorWithoutUpdating()
    {
        var application = SpontaneousApplicationDetailsFactory.GetGenerator().Generate();
        GivenApplication(application);
        var cut = RenderEdit(Key(application));

        cut.Find("#application-position").Change("");
        cut.Find("form").Submit();

        Assert.Equal(PositionError, cut.Find("#application-position + .validation-message").TextContent);
        _jobApplicationService.DidNotReceive()
            .UpdateAsync(Arg.Any<Guid>(), Arg.Any<SpontaneousApplicationInput>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public void Submit_WhenThePostingIsRefused_ShowsTheErrorUnderAppliedVia()
    {
        _jobApplicationService.UpdateAsync(_application.Id, Arg.Any<OfferApplicationInput>(), Arg.Any<CancellationToken>())
            .Returns(ServiceResult<JobApplicationLinkDto>.Failure(JobApplicationErrors.PostingNotFound));
        var cut = RenderEdit(Key(_application));

        cut.Find("form").Submit();

        Assert.Equal(JobApplicationErrors.PostingNotFound, cut.Find("#application-via + .validation-message").TextContent);
    }

    [Fact]
    public void Render_WithUnknownShortId_RequestsNotFound()
    {
        var notFound = ListenToNotFound();

        var cut = RenderEdit("999999-backend-developer");

        Assert.True(notFound());
        Assert.Empty(cut.FindAll("form"));
    }

    [Fact]
    public void Submit_WhenTheApplicationNoLongerExists_RequestsNotFound()
    {
        _jobApplicationService.UpdateAsync(_application.Id, Arg.Any<OfferApplicationInput>(), Arg.Any<CancellationToken>())
            .Returns(ServiceResult<JobApplicationLinkDto>.Failure(JobApplicationErrors.NotFound));
        var notFound = ListenToNotFound();
        var cut = RenderEdit(Key(_application));

        cut.Find("form").Submit();

        Assert.True(notFound());
    }

    private void GivenApplication(JobApplicationDetailsDto application) =>
        _jobApplicationService.GetAsync(application.ShortId, Arg.Any<CancellationToken>()).Returns(application);

    private static string Key(JobApplicationDetailsDto application) => ShortIdKey.Of(application.ShortId, application.Slug);

    private IRenderedComponent<JobApplicationEdit> RenderEdit(string key) =>
        Render<JobApplicationEdit>(parameters => parameters.Add(page => page.Key, key));

    private Func<bool> ListenToNotFound()
    {
        var notFound = false;
        Services.GetRequiredService<NavigationManager>().OnNotFound += (_, _) => notFound = true;
        return () => notFound;
    }
}
