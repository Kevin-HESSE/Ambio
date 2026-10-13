using Ambio.Application.Common.Attributes;
using Ambio.Application.Companies;
using Ambio.Application.Companies.Dtos;
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

public class OfferNewTests : BunitContext
{
    private const string CompanyName = "Kelvio";
    private const string Title = "Backend Developer";
    private const string Site = "LinkedIn";
    private const string Url = "https://www.linkedin.com/jobs/view/4102938475";
    private const string Description = "We are looking for a backend developer.";

    private readonly IJobOfferService _jobOfferService = Substitute.For<IJobOfferService>();
    private readonly ICompanyService _companyService = Substitute.For<ICompanyService>();
    private readonly IReadOnlyList<CompanySummaryDto> _companies;

    public OfferNewTests()
    {
        Services.AddSingleton(_jobOfferService);
        Services.AddSingleton(_companyService);
        var companySummaryFactory = CompanySummaryFactory.GetGenerator();
        _companies = companySummaryFactory.Generate(2);
        _companyService.ListAsync(Arg.Any<CancellationToken>()).Returns(_companies);
    }

    [Fact]
    public void Render_Default_MarksOnlyOptionalFieldsAsOptional()
    {
        var cut = Render<OfferNew>();

        var requiredLabels = cut.FindAll("label.form-label")
            .Where(label => label.QuerySelector(".opt") is null)
            .Select(label => label.TextContent.Trim());
        Assert.Equal(["Company", "Title", "Contract", "Remote policy", "Site", "Link"], requiredLabels);
    }

    [Fact]
    public void Render_Default_SuggestsExistingCompanies()
    {
        var cut = Render<OfferNew>();

        Assert.Equal("offer-companies", cut.Find("#offer-company").GetAttribute("list"));
        Assert.Equal(_companies.Select(company => company.Name), cut.FindAll("#offer-companies option").Select(option => option.GetAttribute("value")));
        Assert.Equal("offer-company-hint", cut.Find("#offer-company").GetAttribute("aria-describedby"));
    }

    [Fact]
    public void Render_Default_SuggestsCommonSites()
    {
        var cut = Render<OfferNew>();

        Assert.Equal("offer-sites", cut.Find("#offer-site").GetAttribute("list"));
        Assert.Equal(OfferNew.CommonSites, cut.FindAll("#offer-sites option").Select(option => option.GetAttribute("value")));
    }

    [Fact]
    public void Render_Default_OffersEveryContractAndRemotePolicyWithDefaultsSelected()
    {
        var cut = Render<OfferNew>();

        Assert.Equal(Enum.GetValues<ContractType>().Length, cut.FindAll("#offer-contract option").Count);
        Assert.Equal(nameof(ContractType.Permanent), cut.Find("#offer-contract").GetAttribute("value"));
        Assert.Equal(Enum.GetValues<RemotePolicy>().Length, cut.FindAll("#offer-remote option").Count);
        Assert.Equal(nameof(RemotePolicy.Unknown), cut.Find("#offer-remote").GetAttribute("value"));
    }

    [Fact]
    public void Render_Default_CancelLinksToOffers()
    {
        var cut = Render<OfferNew>();

        Assert.Equal("Cancel", cut.Find($".action-bar a[href='{OfferRoutes.List}']").TextContent.Trim());
    }

    [Fact]
    public void Submit_WithValidInput_CreatesOfferAndNavigatesToOffers()
    {
        var cut = Render<OfferNew>();

        cut.Find("#offer-company").Change(CompanyName);
        cut.Find("#offer-title").Change(Title);
        cut.Find("#offer-contract").Change(nameof(ContractType.Freelance));
        cut.Find("#offer-remote").Change(nameof(RemotePolicy.FullRemote));
        cut.Find("#offer-site").Change(Site);
        cut.Find("#offer-url").Change(Url);
        cut.Find("#offer-description").Change(Description);
        cut.Find("form").Submit();

        _jobOfferService.Received(1).CreateAsync(
            Arg.Is<NewJobOfferInput>(input =>
                input.CompanyName == CompanyName
                && input.Title == Title
                && input.ContractType == ContractType.Freelance
                && input.RemotePolicy == RemotePolicy.FullRemote
                && input.Site == Site
                && input.Url == Url
                && input.Description == Description),
            Arg.Any<CancellationToken>());
        Assert.EndsWith($"/{OfferRoutes.List}", Services.GetRequiredService<NavigationManager>().Uri);
    }

    [Fact]
    public void Submit_Empty_ShowsErrorsUnderRequiredFieldsWithoutCreating()
    {
        var cut = Render<OfferNew>();

        cut.Find("form").Submit();

        var errors = cut.FindAll(".validation-message").Select(message => message.TextContent);
        Assert.Equal(
            [
                "Enter the name of the company.",
                "Enter the title of the offer.",
                "Enter the site where you saw the offer.",
                "Enter the link to the offer.",
            ],
            errors);
        AssertNotCreated();
    }

    [Fact]
    public void Submit_WithLinkWithoutScheme_ShowsHowToFixIt()
    {
        var cut = Render<OfferNew>();

        cut.Find("#offer-company").Change(CompanyName);
        cut.Find("#offer-title").Change(Title);
        cut.Find("#offer-site").Change(Site);
        cut.Find("#offer-url").Change("linkedin.com/jobs/view/4102938475");
        cut.Find("form").Submit();

        Assert.Equal(new HttpUrlAttribute().FormatErrorMessage("Url"), cut.Find("#offer-url + .validation-message").TextContent);
        AssertNotCreated();
    }

    private void AssertNotCreated() =>
        _jobOfferService.DidNotReceive().CreateAsync(Arg.Any<NewJobOfferInput>(), Arg.Any<CancellationToken>());
}
