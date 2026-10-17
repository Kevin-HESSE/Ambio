using Ambio.Application.Common;
using Ambio.Application.Companies;
using Ambio.Application.Companies.Dtos;
using Ambio.Application.JobApplications;
using Ambio.Application.JobApplications.Dtos;
using Ambio.Application.Offers;
using Ambio.Application.Offers.Dtos;
using Ambio.Domain.JobApplications;
using Ambio.Web.Components.JobApplications;
using Ambio.Web.Components.JobApplications.Shared;
using Ambio.Web.Tests.Fixtures;

using Bunit;
using Bunit.TestDoubles;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

using NSubstitute;

namespace Ambio.Web.Tests.Components.JobApplications.Shared;

public class NewJobApplicationFormTests : BunitContext
{
    private const string OfferError = "Choose the offer you apply to.";
    private const string SentOnError = "Enter the day you sent it.";
    private const string SentOnText = "2026-09-03";
    private const string CompanyError = "Choose the company you apply to.";
    private const string PositionError = "Enter the position you apply for.";
    private const string NewCompanyName = "Lumen Health";
    private const string TargetPosition = "Backend Developer";
    private const string Notes = "Sent with the English CV.";

    private static readonly DateOnly SentOn = new(2026, 9, 3);
    private static readonly JobApplicationLinkDto Created = new("482913", "backend-developer-acme");

    private readonly IJobOfferService _jobOfferService = Substitute.For<IJobOfferService>();
    private readonly IJobApplicationService _jobApplicationService = Substitute.For<IJobApplicationService>();
    private readonly ICompanyService _companyService = Substitute.For<ICompanyService>();
    private readonly CompanySummaryDto _company = CompanySummaryFactory.GetGenerator().Generate();
    private readonly JobOfferDetailsDto _offer = JobOfferDetailsFactory.GetGenerator().Generate();

    public NewJobApplicationFormTests()
    {
        Services.AddSingleton(_jobOfferService);
        Services.AddSingleton(_jobApplicationService);
        Services.AddSingleton(_companyService);
        _companyService.SearchAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns([_company]);
        SetRendererInfo(new RendererInfo("Server", isInteractive: true));
        _jobApplicationService.CreateAsync(Arg.Any<NewOfferApplicationInput>(), Arg.Any<CancellationToken>())
            .Returns(ServiceResult<JobApplicationLinkDto>.Success(Created));
        _jobApplicationService.CreateAsync(Arg.Any<NewSpontaneousApplicationInput>(), Arg.Any<CancellationToken>())
            .Returns(Created);
    }

    [Fact]
    public void Render_BeforeTheCircuit_DisablesTheButtons()
    {
        SetRendererInfo(new RendererInfo("Static", isInteractive: false));

        var cut = RenderForm(_offer);

        Assert.True(cut.Find("button[type='submit']").HasAttribute("disabled"));
        Assert.True(cut.Find(".application-offer-change").HasAttribute("disabled"));
        Assert.All(cut.FindAll("input[name='application-kind']"), radio => Assert.True(radio.HasAttribute("disabled")));
    }

    [Fact]
    public void Render_WithoutOffer_ShowsTheSearch()
    {
        var cut = RenderForm(offer: null);

        Assert.NotNull(cut.Find("input#application-offer[type='search']"));
        Assert.Equal(["Not specified"], cut.FindAll("#application-via option").Select(option => option.TextContent));
    }

    [Fact]
    public void Render_WithOffer_ShowsItAndItsPostingsToApplyVia()
    {
        var cut = RenderForm(_offer);

        Assert.Equal($"{_offer.Title} · {_offer.CompanyName}", cut.Find(".application-offer-name").TextContent);
        Assert.Equal(
            ["", .. _offer.Postings.Select(posting => posting.Id.ToString())],
            cut.FindAll("#application-via option").Select(option => option.GetAttribute("value")));
        Assert.Empty(cut.FindAll("#application-via option[selected]"));
    }

    [Fact]
    public void Render_WithOfferSeenOnOneSite_PreselectsItsPosting()
    {
        var offer = JobOfferDetailsFactory.GetGenerator(numberOfPosting: 1).Generate();

        var cut = RenderForm(offer);

        Assert.Equal(offer.Postings[0].Id.ToString(), cut.Find("#application-via").GetAttribute("value"));
    }

    [Fact]
    public void SelectResult_LoadsTheOfferWithItsPostings()
    {
        var summary = JobOfferSummaryFactory.GetGenerator().Generate() with { Id = _offer.Id };
        _jobOfferService.SearchAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns([summary]);
        _jobOfferService.GetAsync(_offer.Id, Arg.Any<CancellationToken>()).Returns(_offer);
        var cut = RenderForm(offer: null);

        cut.Find("#application-offer").Input(summary.Title);
        cut.WaitForElement("button.search-field-result").Click();

        Assert.Equal($"{_offer.Title} · {_offer.CompanyName}", cut.WaitForElement(".application-offer-name").TextContent);
        Assert.Equal(_offer.Postings.Count + 1, cut.FindAll("#application-via option").Count);
    }

    [Fact]
    public void Change_BringsTheSearchBack()
    {
        var cut = RenderForm(_offer);

        cut.Find(".application-offer-change").Click();

        Assert.NotNull(cut.Find("input#application-offer[type='search']"));
        Assert.Single(cut.FindAll("#application-via option"));
    }

    [Fact]
    public void AlreadySent_WhenChecked_ShowsTheSendingDay()
    {
        var cut = RenderForm(_offer);
        Assert.Empty(cut.FindAll("#application-sent-on"));

        cut.Find("#application-already-sent").Change(true);

        Assert.NotNull(cut.Find("#application-sent-on"));
    }

    [Fact]
    public void Submit_WithoutOffer_ShowsTheErrorWithoutCreating()
    {
        var cut = RenderForm(offer: null);

        cut.Find("form").Submit();

        Assert.Equal([OfferError], cut.FindAll(".validation-message").Select(message => message.TextContent));
        _jobApplicationService.DidNotReceive().CreateAsync(Arg.Any<NewOfferApplicationInput>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public void Submit_AlreadySentWithoutDay_ShowsTheErrorWithoutCreating()
    {
        var cut = RenderForm(_offer);
        cut.Find("#application-already-sent").Change(true);

        cut.Find("form").Submit();

        Assert.Equal(SentOnError, cut.Find("#application-sent-on + .validation-message").TextContent);
        _jobApplicationService.DidNotReceive().CreateAsync(Arg.Any<NewOfferApplicationInput>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public void Submit_Valid_CreatesTheApplicationAndOpensItsPage()
    {
        var posting = _offer.Postings[1];
        var cut = RenderForm(_offer);

        cut.Find("#application-via").Change(posting.Id.ToString());
        cut.Find("#application-channel").Change(nameof(Channel.Email));
        cut.Find("#application-already-sent").Change(true);
        cut.Find("#application-sent-on").Change(SentOnText);
        cut.Find("form").Submit();

        _jobApplicationService.Received(1).CreateAsync(
            Arg.Is<NewOfferApplicationInput>(input =>
                input.JobOfferId == _offer.Id
                && input.AppliedViaPostingId == posting.Id
                && input.Channel == Channel.Email
                && input.AlreadySent
                && input.SentOn == SentOn),
            Arg.Any<CancellationToken>());
        Assert.EndsWith(
            $"/{JobApplicationRoutes.Details(Created.ShortId, Created.Slug)}",
            Services.GetRequiredService<BunitNavigationManager>().Uri);
    }

    [Fact]
    public void Submit_WhenThePostingIsRefused_ShowsTheErrorUnderAppliedVia()
    {
        _jobApplicationService.CreateAsync(Arg.Any<NewOfferApplicationInput>(), Arg.Any<CancellationToken>())
            .Returns(ServiceResult<JobApplicationLinkDto>.Failure(JobApplicationErrors.PostingNotFound));
        var cut = RenderForm(_offer);

        cut.Find("form").Submit();

        Assert.Equal(JobApplicationErrors.PostingNotFound, cut.Find("#application-via + .validation-message").TextContent);
    }

    [Fact]
    public void Spontaneous_WhenChosen_ShowsTheCompanySearchAndThePositionInsteadOfTheOffer()
    {
        var cut = RenderForm(_offer);

        ChooseSpontaneous(cut);

        Assert.NotNull(cut.Find("input#application-company[type='search']"));
        Assert.NotNull(cut.Find("#application-position"));
        Assert.Empty(cut.FindAll("#application-offer"));
        Assert.Empty(cut.FindAll("#application-via"));
    }

    [Fact]
    public void Spontaneous_WhenChosen_KeepsTheValuesOfTheApplicationSection()
    {
        var cut = RenderForm(_offer);
        cut.Find("#application-channel").Change(nameof(Channel.Email));
        cut.Find("#application-already-sent").Change(true);
        cut.Find("#application-sent-on").Change(SentOnText);
        cut.Find("#application-notes").Change(Notes);

        ChooseSpontaneous(cut);

        Assert.Equal(
            (nameof(Channel.Email), SentOnText, Notes),
            (cut.Find("#application-channel").GetAttribute("value"), cut.Find("#application-sent-on").GetAttribute("value"),
                cut.Find("#application-notes").GetAttribute("value")));
    }

    [Fact]
    public void Render_WithCompanyName_StartsSpontaneousWithIt()
    {
        var cut = RenderForm(offer: null, companyName: _company.Name);

        Assert.True(cut.Find($"#application-kind-{nameof(ApplicationKind.Spontaneous)}").HasAttribute("checked"));
        Assert.Equal(_company.Name, cut.Find(".application-company-name").TextContent.Trim());
    }

    [Fact]
    public void ChangeCompany_BringsTheCompanySearchBack()
    {
        var cut = RenderForm(offer: null, companyName: _company.Name);

        cut.Find(".application-company-change").Click();

        Assert.NotNull(cut.Find("input#application-company[type='search']"));
    }

    [Fact]
    public void Submit_SpontaneousWithoutCompanyNorPosition_ShowsBothErrorsWithoutCreating()
    {
        var cut = RenderForm(offer: null);
        ChooseSpontaneous(cut);

        cut.Find("form").Submit();

        Assert.Equal([CompanyError, PositionError], cut.FindAll(".validation-message").Select(message => message.TextContent));
        _jobApplicationService.DidNotReceive().CreateAsync(Arg.Any<NewSpontaneousApplicationInput>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public void Submit_SpontaneousToAFoundCompany_CreatesItAndOpensItsPage()
    {
        var cut = RenderForm(offer: null);
        ChooseSpontaneous(cut);
        cut.Find("#application-company").Input(_company.Name);
        cut.WaitForElement("button.search-field-result").Click();
        cut.WaitForElement(".application-company-name");

        cut.Find("#application-position").Change(TargetPosition);
        cut.Find("form").Submit();

        _jobApplicationService.Received(1).CreateAsync(
            Arg.Is<NewSpontaneousApplicationInput>(input =>
                input.CompanyName == _company.Name && input.TargetPosition == TargetPosition),
            Arg.Any<CancellationToken>());
        Assert.EndsWith(
            $"/{JobApplicationRoutes.Details(Created.ShortId, Created.Slug)}",
            Services.GetRequiredService<BunitNavigationManager>().Uri);
    }

    [Fact]
    public void Submit_SpontaneousToANewCompany_CreatesItWithTheTypedName()
    {
        _companyService.SearchAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns([]);
        var cut = RenderForm(offer: null);
        ChooseSpontaneous(cut);
        cut.Find("#application-company").Input(NewCompanyName);
        cut.WaitForElement(".company-search-new").Click();

        Assert.Contains("new company", cut.WaitForElement(".application-company-name").TextContent);
        cut.Find("#application-position").Change(TargetPosition);
        cut.Find("form").Submit();

        _jobApplicationService.Received(1).CreateAsync(
            Arg.Is<NewSpontaneousApplicationInput>(input => input.CompanyName == NewCompanyName),
            Arg.Any<CancellationToken>());
    }

    private static void ChooseSpontaneous(IRenderedComponent<NewJobApplicationForm> cut) =>
        cut.Find($"#application-kind-{nameof(ApplicationKind.Spontaneous)}").Change(true);

    private IRenderedComponent<NewJobApplicationForm> RenderForm(JobOfferDetailsDto? offer, string? companyName = null) =>
        Render<NewJobApplicationForm>(parameters => parameters
            .Add(p => p.Offer, offer)
            .Add(p => p.CompanyName, companyName));
}
