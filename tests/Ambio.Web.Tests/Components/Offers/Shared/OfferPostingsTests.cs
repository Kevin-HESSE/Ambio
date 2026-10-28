using Ambio.Application.Common;
using Ambio.Application.Offers;
using Ambio.Application.Offers.Dtos;
using Ambio.Web.Components.Offers.Shared;
using Ambio.Web.Tests.Fixtures;

using Bogus;

using Bunit;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

using NSubstitute;

namespace Ambio.Web.Tests.Components.Offers.Shared;

public class OfferPostingsTests : BunitContext
{
    private const string CloseSheet = "ambio.sheet.close";
    private const string Site = "LinkedIn";
    private const string Url = "https://www.linkedin.com/jobs/view/4102938475";
    private const string SiteError = "Enter the site where you saw the offer.";
    private const string UrlError = "Enter the link to the offer.";

    private readonly IJobOfferService _jobOfferService = Substitute.For<IJobOfferService>();
    private readonly Faker<JobOfferDetailsDto> _jobOfferDetailsFactory;
    private readonly Faker<JobOfferPostingDto> _jobOfferPostingFactory;

    public OfferPostingsTests()
    {
        Services.AddSingleton(_jobOfferService);
        SetRendererInfo(new RendererInfo("Server", isInteractive: true));
        JSInterop.SetupVoid(CloseSheet, _ => true).SetVoidResult();
        _jobOfferDetailsFactory = JobOfferDetailsFactory.GetGenerator();
        _jobOfferPostingFactory = JobOfferPostingFactory.GetGenerator();
    }

    [Fact]
    public void Render_BeforeTheCircuit_DisablesTheButtons()
    {
        SetRendererInfo(new RendererInfo("Static", isInteractive: false));
        var offer = _jobOfferDetailsFactory.Generate();

        var cut = RenderPostings(offer);

        Assert.True(cut.Find(".offer-postings-add").HasAttribute("disabled"));
        Assert.All(cut.FindAll(".offer-posting-remove"), button => Assert.True(button.HasAttribute("disabled")));
    }

    [Fact]
    public void Render_Interactive_AddOpensTheAddPostingSheet()
    {
        var offer = _jobOfferDetailsFactory.Generate();

        var cut = RenderPostings(offer);

        var add = cut.Find(".offer-postings-add");
        Assert.False(add.HasAttribute("disabled"));
        Assert.Equal(OfferPostings.AddSheetId, add.GetAttribute("data-sheet-open"));
        Assert.Equal("Add a posting", cut.Find($"#{OfferPostings.AddSheetId} h2").TextContent);
    }

    [Fact]
    public void Render_WithSiteField_SuggestsCommonSites()
    {
        var cut = RenderPostings(_jobOfferDetailsFactory.Generate());

        Assert.Equal("posting-sites", cut.Find("#posting-site").GetAttribute("list"));
        Assert.Equal(SiteSuggestions.CommonSites, cut.FindAll("#posting-sites option").Select(option => option.GetAttribute("value")));
    }

    [Fact]
    public void Submit_WithSiteAndLink_AddsThePostingReloadsTheListAndClosesTheSheet()
    {
        var offer = _jobOfferDetailsFactory.Generate();
        var added = _jobOfferPostingFactory.Generate() with { Site = Site, Url = Url };
        _jobOfferService.AddPostingAsync(offer.Id, Arg.Any<JobOfferPostingInput>(), Arg.Any<CancellationToken>())
            .Returns(ServiceResult.Success());
        _jobOfferService.GetAsync(offer.ShortId, Arg.Any<CancellationToken>())
            .Returns(offer with { Postings = [.. offer.Postings, added] });
        var cut = RenderPostings(offer);

        cut.Find("#posting-site").Change(Site);
        cut.Find("#posting-url").Change(Url);
        cut.Find($"#{OfferPostings.AddSheetId} form").Submit();

        _jobOfferService.Received(1).AddPostingAsync(
            offer.Id,
            Arg.Is<JobOfferPostingInput>(input => input.Site == Site && input.Url == Url),
            Arg.Any<CancellationToken>());
        Assert.Equal(3, cut.FindAll(".offer-posting").Count);
        Assert.Equal("3", cut.Find(".offer-postings-count").TextContent);
        Assert.Equal("", cut.Find("#posting-url").GetAttribute("value"));
        Assert.Equal(OfferPostings.AddSheetId, JSInterop.VerifyInvoke(CloseSheet).Arguments[0]);
    }

    [Fact]
    public void Submit_Empty_ShowsErrorsWithoutAdding()
    {
        var cut = RenderPostings(_jobOfferDetailsFactory.Generate());

        cut.Find($"#{OfferPostings.AddSheetId} form").Submit();

        Assert.Equal([SiteError, UrlError], cut.FindAll(".validation-message").Select(message => message.TextContent));
        AssertNotAdded();
        JSInterop.VerifyNotInvoke(CloseSheet);
    }

    [Fact]
    public void Submit_WithLinkTheOfferHas_ShowsTheErrorUnderTheLinkAndKeepsTheSheetOpen()
    {
        var offer = _jobOfferDetailsFactory.Generate();
        _jobOfferService.AddPostingAsync(offer.Id, Arg.Any<JobOfferPostingInput>(), Arg.Any<CancellationToken>())
            .Returns(ServiceResult.Failure(JobOfferErrors.PostingAlreadyExists));
        var cut = RenderPostings(offer);

        cut.Find("#posting-site").Change(Site);
        cut.Find("#posting-url").Change(offer.Postings[0].Url);
        cut.Find($"#{OfferPostings.AddSheetId} form").Submit();

        Assert.Equal(JobOfferErrors.PostingAlreadyExists, cut.Find("#posting-url + .validation-message").TextContent);
        JSInterop.VerifyNotInvoke(CloseSheet);
    }

    [Fact]
    public void Submit_AfterChangingTheLinkTheOfferHad_AddsThePosting()
    {
        var offer = _jobOfferDetailsFactory.Generate();
        _jobOfferService.AddPostingAsync(offer.Id, Arg.Is<JobOfferPostingInput>(input => input.Url == offer.Postings[0].Url), Arg.Any<CancellationToken>())
            .Returns(ServiceResult.Failure(JobOfferErrors.PostingAlreadyExists));
        _jobOfferService.AddPostingAsync(offer.Id, Arg.Is<JobOfferPostingInput>(input => input.Url == Url), Arg.Any<CancellationToken>())
            .Returns(ServiceResult.Success());
        _jobOfferService.GetAsync(offer.ShortId, Arg.Any<CancellationToken>()).Returns(offer);
        var cut = RenderPostings(offer);
        cut.Find("#posting-site").Change(Site);
        cut.Find("#posting-url").Change(offer.Postings[0].Url);
        cut.Find($"#{OfferPostings.AddSheetId} form").Submit();

        cut.Find("#posting-url").Change(Url);
        cut.Find($"#{OfferPostings.AddSheetId} form").Submit();

        // The form is the same object for both calls: only the last one closes the sheet.
        Assert.Equal(OfferPostings.AddSheetId, JSInterop.VerifyInvoke(CloseSheet).Arguments[0]);
        Assert.Empty(cut.FindAll(".validation-message"));
    }

    [Fact]
    public void Submit_WhenTheOfferNoLongerExists_RequestsNotFound()
    {
        var offer = _jobOfferDetailsFactory.Generate();
        _jobOfferService.AddPostingAsync(offer.Id, Arg.Any<JobOfferPostingInput>(), Arg.Any<CancellationToken>())
            .Returns(ServiceResult.Failure(JobOfferErrors.NotFound));
        var notFound = ListenToNotFound();
        var cut = RenderPostings(offer);

        cut.Find("#posting-site").Change(Site);
        cut.Find("#posting-url").Change(Url);
        cut.Find($"#{OfferPostings.AddSheetId} form").Submit();

        Assert.True(notFound());
    }

    [Fact]
    public void Render_WithOnePosting_HasNoRemoveButton()
    {
        var offer = JobOfferDetailsFactory.GetGenerator(numberOfPosting: 1).Generate();

        var cut = RenderPostings(offer);

        Assert.Empty(cut.FindAll(".offer-posting-remove"));
        Assert.Empty(cut.FindAll("dialog[id^='remove-posting-']"));
    }

    [Fact]
    public void Render_WithSeveralPostings_EachRemoveButtonOpensItsConfirmation()
    {
        var offer = _jobOfferDetailsFactory.Generate();

        var cut = RenderPostings(offer);

        var buttons = cut.FindAll(".offer-posting-remove");
        Assert.Equal(
            offer.Postings.Select(posting => (OfferPostings.RemoveSheetId(posting), $"Remove the {posting.Site} posting")),
            buttons.Select(button => (button.GetAttribute("data-sheet-open")!, button.GetAttribute("aria-label")!)));
        Assert.All(offer.Postings, posting => cut.Find($"#{OfferPostings.RemoveSheetId(posting)}"));
    }

    [Fact]
    public void Remove_Confirmed_RemovesThePostingReloadsTheListAndClosesTheSheet()
    {
        var offer = _jobOfferDetailsFactory.Generate();
        var removed = offer.Postings[0];
        _jobOfferService.RemovePostingAsync(offer.Id, removed.Id, Arg.Any<CancellationToken>())
            .Returns(ServiceResult.Success());
        _jobOfferService.GetAsync(offer.ShortId, Arg.Any<CancellationToken>())
            .Returns(offer with { Postings = [offer.Postings[1]] });
        var cut = RenderPostings(offer);

        cut.Find($"#{OfferPostings.RemoveSheetId(removed)} .btn-danger").Click();

        _jobOfferService.Received(1).RemovePostingAsync(offer.Id, removed.Id, Arg.Any<CancellationToken>());
        Assert.Equal(offer.Postings[1].Site, cut.Find(".offer-posting-site").TextContent);
        Assert.Empty(cut.FindAll(".offer-posting-remove"));
        Assert.Equal(OfferPostings.RemoveSheetId(removed), JSInterop.VerifyInvoke(CloseSheet).Arguments[0]);
    }

    private IRenderedComponent<OfferPostings> RenderPostings(JobOfferDetailsDto offer) =>
        Render<OfferPostings>(parameters => parameters
            .Add(card => card.OfferId, offer.Id)
            .Add(card => card.OfferShortId, offer.ShortId)
            .Add(card => card.Postings, offer.Postings));

    private void AssertNotAdded() =>
        _jobOfferService.DidNotReceive().AddPostingAsync(Arg.Any<Guid>(), Arg.Any<JobOfferPostingInput>(), Arg.Any<CancellationToken>());

    private Func<bool> ListenToNotFound()
    {
        var notFound = false;
        Services.GetRequiredService<NavigationManager>().OnNotFound += (_, _) => notFound = true;
        return () => notFound;
    }
}
