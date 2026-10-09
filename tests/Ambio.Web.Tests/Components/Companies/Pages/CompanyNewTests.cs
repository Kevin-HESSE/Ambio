using Ambio.Application.Common;
using Ambio.Application.Companies;
using Ambio.Application.Companies.Dtos;
using Ambio.Domain.Companies;
using Ambio.Web.Components.Companies.Pages;

using Bunit;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

using NSubstitute;

namespace Ambio.Web.Tests.Components.Companies.Pages;

public class CompanyNewTests : BunitContext
{
    private const string CompanyName = "Brightwave";
    private const string CompaniesPath = "companies";
    private const string NameError = "Enter the name of the company.";
    private const string UrlError = "Enter the full link, starting with https://";

    private readonly ICompanyService _companyService = Substitute.For<ICompanyService>();

    public CompanyNewTests()
    {
        Services.AddSingleton(_companyService);
    }

    [Fact]
    public void Render_Default_OffersEveryKindAndSizeWithEmployerSelected()
    {
        var cut = Render<CompanyNew>();

        Assert.Equal(Enum.GetValues<CompanyKind>().Length, cut.FindAll("#company-kind option").Count);
        Assert.Equal(nameof(CompanyKind.Employer), cut.Find("#company-kind").GetAttribute("value"));
        Assert.Equal(Enum.GetValues<CompanySize>().Length + 1, cut.FindAll("#company-size option").Count);
    }

    [Fact]
    public void Render_Default_MarksOnlyNameAndTypeAsRequired()
    {
        var cut = Render<CompanyNew>();

        var requiredLabels = cut.FindAll("label.form-label")
            .Where(label => label.QuerySelector(".opt") is null)
            .Select(label => label.TextContent.Trim());
        Assert.Equal(["Name", "Type"], requiredLabels);
    }

    [Fact]
    public void Render_Default_CancelLinksToCompanies()
    {
        var cut = Render<CompanyNew>();

        Assert.Equal("Cancel", cut.Find($".action-bar a[href='{CompaniesPath}']").TextContent.Trim());
    }

    [Fact]
    public void Submit_WithValidInput_CreatesCompanyAndNavigatesToList()
    {
        _companyService.CreateAsync(Arg.Any<CreateCompanyInput>(), Arg.Any<CancellationToken>())
            .Returns(ServiceResult<Guid>.Success(Guid.CreateVersion7()));
        var cut = Render<CompanyNew>();

        cut.Find("#company-name").Change(CompanyName);
        cut.Find("#company-kind").Change(nameof(CompanyKind.RecruitmentAgency));
        cut.Find("form").Submit();

        _companyService.Received(1).CreateAsync(
            Arg.Is<CreateCompanyInput>(input => input.Name == CompanyName && input.Kind == CompanyKind.RecruitmentAgency),
            Arg.Any<CancellationToken>());
        Assert.EndsWith($"/{CompaniesPath}", Services.GetRequiredService<NavigationManager>().Uri);
    }

    [Fact]
    public void Submit_WithoutName_ShowsErrorUnderNameWithoutCreating()
    {
        var cut = Render<CompanyNew>();

        cut.Find("form").Submit();

        Assert.Equal(NameError, cut.Find("#company-name + .validation-message").TextContent);
        AssertNotCreated();
    }

    [Fact]
    public void Submit_WithWebsiteWithoutScheme_ShowsHowToFixIt()
    {
        var cut = Render<CompanyNew>();

        cut.Find("#company-name").Change(CompanyName);
        cut.Find("#company-website").Change("brightwave.io");
        cut.Find("form").Submit();

        Assert.Equal(UrlError, cut.Find("#company-website + .validation-message").TextContent);
        AssertNotCreated();
    }

    [Fact]
    public void Submit_WithExistingName_ShowsErrorUnderNameAndStaysOnPage()
    {
        _companyService.CreateAsync(Arg.Any<CreateCompanyInput>(), Arg.Any<CancellationToken>())
            .Returns(ServiceResult<Guid>.Failure(CompanyErrors.AlreadyExists));
        var cut = Render<CompanyNew>();
        var navigation = Services.GetRequiredService<NavigationManager>();
        var startUri = navigation.Uri;

        cut.Find("#company-name").Change(CompanyName);
        cut.Find("form").Submit();

        Assert.Equal(CompanyErrors.AlreadyExists, cut.Find("#company-name + .validation-message").TextContent);
        Assert.Equal(startUri, navigation.Uri);
    }

    private void AssertNotCreated() =>
        _companyService.DidNotReceive().CreateAsync(Arg.Any<CreateCompanyInput>(), Arg.Any<CancellationToken>());
}
