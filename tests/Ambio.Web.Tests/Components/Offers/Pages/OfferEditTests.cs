using Ambio.Application.Common;
using Ambio.Application.Offers;
using Ambio.Application.Offers.Dtos;
using Ambio.Domain.Offers;
using Ambio.Web.Components.Offers;
using Ambio.Web.Components.Offers.Pages;
using Ambio.Web.Tests.Fixtures;

using Bunit;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

using NSubstitute;

namespace Ambio.Web.Tests.Components.Offers.Pages;

public class OfferEditTests : BunitContext
{
    private const string NewTitle = "Platform Engineer";
    private const string NewSalary = "TJM 550";
    private const string TitleError = "Enter the title of the offer.";

    private readonly IJobOfferService _jobOfferService = Substitute.For<IJobOfferService>();
    private readonly JobOfferDetailsDto _offer;

    public OfferEditTests()
    {
        Services.AddSingleton(_jobOfferService);
        _offer = JobOfferDetailsFactory.GetGenerator().Generate();
        _jobOfferService.GetAsync(_offer.Id, Arg.Any<CancellationToken>()).Returns(_offer);
    }

    [Fact]
    public void Render_WithOffer_PrefillsTheFormAndNamesTheCompany()
    {
        var cut = RenderEdit(_offer.Id);

        Assert.Equal(_offer.Title, cut.Find("#offer-title").GetAttribute("value"));
        Assert.Equal(_offer.ContractType.ToString(), cut.Find("#offer-contract").GetAttribute("value"));
        Assert.Equal(_offer.RemotePolicy.ToString(), cut.Find("#offer-remote").GetAttribute("value"));
        Assert.Equal(_offer.Location, cut.Find("#offer-location").GetAttribute("value"));
        Assert.Equal(_offer.Salary, cut.Find("#offer-salary").GetAttribute("value"));
        Assert.Equal(_offer.Description, cut.Find("#offer-description").GetAttribute("value"));
        Assert.Contains(_offer.CompanyName, cut.Find(".page-header").TextContent);
    }

    [Fact]
    public void Render_WithOffer_HasNoCompanyNorPostingField()
    {
        var cut = RenderEdit(_offer.Id);

        Assert.Empty(cut.FindAll("#offer-company"));
        Assert.Empty(cut.FindAll("#offer-site"));
        Assert.Empty(cut.FindAll("#offer-url"));
    }

    [Fact]
    public void Render_WithOffer_CancelLinksToTheOfferPage()
    {
        var cut = RenderEdit(_offer.Id);

        Assert.Equal("Cancel", cut.Find($".action-bar a[href='{OfferRoutes.Details(_offer.Id)}']").TextContent.Trim());
    }

    [Fact]
    public void Submit_WithChanges_UpdatesTheOfferAndNavigatesToItsPage()
    {
        _jobOfferService.UpdateAsync(_offer.Id, Arg.Any<JobOfferInput>(), Arg.Any<CancellationToken>())
            .Returns(ServiceResult.Success());
        var cut = RenderEdit(_offer.Id);

        cut.Find("#offer-title").Change(NewTitle);
        cut.Find("#offer-contract").Change(nameof(ContractType.Freelance));
        cut.Find("#offer-salary").Change(NewSalary);
        cut.Find("form").Submit();

        _jobOfferService.Received(1).UpdateAsync(
            _offer.Id,
            Arg.Is<JobOfferInput>(input =>
                input.Title == NewTitle
                && input.ContractType == ContractType.Freelance
                && input.Salary == NewSalary
                && input.Location == _offer.Location),
            Arg.Any<CancellationToken>());
        Assert.EndsWith($"/{OfferRoutes.Details(_offer.Id)}", Services.GetRequiredService<NavigationManager>().Uri);
    }

    [Fact]
    public void Submit_WithoutTitle_ShowsErrorUnderTitleWithoutUpdating()
    {
        var cut = RenderEdit(_offer.Id);

        cut.Find("#offer-title").Change("");
        cut.Find("form").Submit();

        Assert.Equal(TitleError, cut.Find("#offer-title + .validation-message").TextContent);
        _jobOfferService.DidNotReceive().UpdateAsync(Arg.Any<Guid>(), Arg.Any<JobOfferInput>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public void Render_WithUnknownId_RequestsNotFound()
    {
        var notFound = ListenToNotFound();

        var cut = RenderEdit(Guid.CreateVersion7());

        Assert.True(notFound());
        Assert.Empty(cut.FindAll("form"));
    }

    [Fact]
    public void Submit_WhenTheOfferNoLongerExists_RequestsNotFound()
    {
        _jobOfferService.UpdateAsync(_offer.Id, Arg.Any<JobOfferInput>(), Arg.Any<CancellationToken>())
            .Returns(ServiceResult.Failure(JobOfferErrors.NotFound));
        var notFound = ListenToNotFound();
        var cut = RenderEdit(_offer.Id);

        cut.Find("form").Submit();

        Assert.True(notFound());
    }

    private IRenderedComponent<OfferEdit> RenderEdit(Guid id) =>
        Render<OfferEdit>(parameters => parameters.Add(page => page.Id, id));

    private Func<bool> ListenToNotFound()
    {
        var notFound = false;
        Services.GetRequiredService<NavigationManager>().OnNotFound += (_, _) => notFound = true;
        return () => notFound;
    }
}
