using Ambio.Application.Common;
using Ambio.Application.Companies;
using Ambio.Application.Companies.Dtos;
using Ambio.Web.Components.Companies;
using Ambio.Web.Components.Companies.Pages;
using Ambio.Web.Tests.Fixtures;

using Bunit;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

using NSubstitute;

namespace Ambio.Web.Tests.Components.Companies.Pages;

public class CompanyEditTests : BunitContext
{
    private const string NewName = "Brightwave Energy";
    private const string NewSlug = "brightwave-energy";
    private const string NewLocation = "Grenoble";
    private const string NameError = "Enter the name of the company.";

    private readonly ICompanyService _companyService = Substitute.For<ICompanyService>();
    private readonly CompanyDetailsDto _company;

    public CompanyEditTests()
    {
        Services.AddSingleton(_companyService);
        var companyDetailsFactory = CompanyDetailsFactory.GetGenerator();
        _company = companyDetailsFactory.Generate();
        _companyService.GetAsync(_company.Slug, Arg.Any<CancellationToken>()).Returns(_company);
    }

    [Fact]
    public void Render_WithCompany_PrefillsTheForm()
    {
        var cut = RenderEdit(_company.Slug);

        Assert.Equal(_company.Name, cut.Find("#company-name").GetAttribute("value"));
        Assert.Equal(_company.Kind.ToString(), cut.Find("#company-kind").GetAttribute("value"));
        Assert.Equal(_company.Size.ToString(), cut.Find("#company-size").GetAttribute("value"));
        Assert.Equal(_company.Location, cut.Find("#company-location").GetAttribute("value"));
        Assert.Equal(_company.Website, cut.Find("#company-website").GetAttribute("value"));
        Assert.Equal(_company.Description, cut.Find("#company-description").GetAttribute("value"));
    }

    [Fact]
    public void Render_WithCompany_CancelLinksToDetails()
    {
        var cut = RenderEdit(_company.Slug);

        Assert.Equal("Cancel", cut.Find($".action-bar a[href='{CompanyRoutes.Details(_company.Slug)}']").TextContent.Trim());
    }

    [Fact]
    public void Submit_WithChanges_UpdatesCompanyAndNavigatesToReturnedSlug()
    {
        _companyService.UpdateAsync(_company.Id, Arg.Any<CompanyInput>(), Arg.Any<CancellationToken>())
            .Returns(ServiceResult<string>.Success(NewSlug));
        var cut = RenderEdit(_company.Slug);

        cut.Find("#company-name").Change(NewName);
        cut.Find("#company-location").Change(NewLocation);
        cut.Find("form").Submit();

        _companyService.Received(1).UpdateAsync(
            _company.Id,
            Arg.Is<CompanyInput>(input =>
                input.Name == NewName && input.Location == NewLocation && input.Industry == _company.Industry),
            Arg.Any<CancellationToken>());
        Assert.EndsWith($"/{CompanyRoutes.Details(NewSlug)}", Services.GetRequiredService<NavigationManager>().Uri);
    }

    [Fact]
    public void Submit_WithoutName_ShowsErrorUnderNameWithoutUpdating()
    {
        var cut = RenderEdit(_company.Slug);

        cut.Find("#company-name").Change("");
        cut.Find("form").Submit();

        Assert.Equal(NameError, cut.Find("#company-name + .validation-message").TextContent);
        _companyService.DidNotReceive().UpdateAsync(Arg.Any<Guid>(), Arg.Any<CompanyInput>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public void Submit_WithExistingName_ShowsErrorUnderNameAndStaysOnPage()
    {
        _companyService.UpdateAsync(_company.Id, Arg.Any<CompanyInput>(), Arg.Any<CancellationToken>())
            .Returns(ServiceResult<string>.Failure(CompanyErrors.AlreadyExists));
        var cut = RenderEdit(_company.Slug);
        var navigation = Services.GetRequiredService<NavigationManager>();
        var startUri = navigation.Uri;

        cut.Find("#company-name").Change(NewName);
        cut.Find("form").Submit();

        Assert.Equal(CompanyErrors.AlreadyExists, cut.Find("#company-name + .validation-message").TextContent);
        Assert.Equal(startUri, navigation.Uri);
    }

    [Fact]
    public void Render_WithUnknownSlug_RequestsNotFound()
    {
        var notFound = ListenToNotFound();

        var cut = RenderEdit("kelvio");

        Assert.True(notFound());
        Assert.Empty(cut.FindAll("form"));
    }

    [Fact]
    public void Submit_WhenCompanyNoLongerExists_RequestsNotFound()
    {
        _companyService.UpdateAsync(_company.Id, Arg.Any<CompanyInput>(), Arg.Any<CancellationToken>())
            .Returns(ServiceResult<string>.Failure(CompanyErrors.NotFound));
        var notFound = ListenToNotFound();
        var cut = RenderEdit(_company.Slug);

        cut.Find("form").Submit();

        Assert.True(notFound());
    }

    private IRenderedComponent<CompanyEdit> RenderEdit(string slug) =>
        Render<CompanyEdit>(parameters => parameters.Add(page => page.Slug, slug));

    private Func<bool> ListenToNotFound()
    {
        var notFound = false;
        Services.GetRequiredService<NavigationManager>().OnNotFound += (_, _) => notFound = true;
        return () => notFound;
    }
}
