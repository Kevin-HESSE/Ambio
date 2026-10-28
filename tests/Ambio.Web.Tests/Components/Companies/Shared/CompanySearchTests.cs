using Ambio.Application.Companies;
using Ambio.Application.Companies.Dtos;
using Ambio.Web.Components.Companies.Shared;
using Ambio.Web.Tests.Fixtures;

using Bunit;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;

using NSubstitute;

namespace Ambio.Web.Tests.Components.Companies.Shared;

public class CompanySearchTests : BunitContext
{
    private const string Id = "application-company";
    private const string Text = "lumen";
    private const string NewName = "Lumen Health";

    private readonly ICompanyService _companyService = Substitute.For<ICompanyService>();
    private readonly CompanySummaryDto[] _companies;

    public CompanySearchTests()
    {
        Services.AddSingleton(_companyService);
        SetRendererInfo(new RendererInfo("Server", isInteractive: true));
        _companies = [.. CompanySummaryFactory.GetGenerator().Generate(2)];
        _companyService.SearchAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(_companies);
    }

    [Fact]
    public void Input_Text_ShowsTheMatchingCompaniesWithTheirKindThenTheCreateLine()
    {
        var cut = RenderSearch();

        cut.Find($"#{Id}").Input(Text);

        cut.WaitForAssertion(() => Assert.Equal(
            _companies.Select(company => company.Name),
            cut.FindAll(".company-search-name").Select(name => name.TextContent)));
        Assert.Equal(
            _companies.Select(company => $"{company.Kind.ToLabel()} · {company.Location}"),
            cut.FindAll(".company-search-meta").Select(meta => meta.TextContent.Trim()));
        Assert.Contains($"“{Text}”", cut.Find(".company-search-new").TextContent);
    }

    [Fact]
    public void Input_NameOfAResultInAnotherCase_HidesTheCreateLine()
    {
        var cut = RenderSearch();

        cut.Find($"#{Id}").Input($" {_companies[0].Name.ToUpperInvariant()} ");

        cut.WaitForElement(".company-search-name");
        Assert.Empty(cut.FindAll(".company-search-new"));
    }

    [Fact]
    public void Click_OnAResult_SelectsItsCompany()
    {
        CompanySummaryDto? selected = null;
        var cut = RenderSearch(onSelect: company => selected = company);
        cut.Find($"#{Id}").Input(Text);
        cut.WaitForElement(".company-search-name");

        cut.FindAll("button.search-field-result")[1].Click();

        // The pick goes through SearchField first.
        cut.WaitForAssertion(() => Assert.Equal(_companies[1], selected));
    }

    [Fact]
    public async Task Click_OnTheCreateLine_GivesTheTrimmedName()
    {
        _companyService.SearchAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns([]);
        string? created = null;
        var cut = RenderSearch(onCreate: name => created = name);
        cut.Find($"#{Id}").Input($"  {NewName} ");

        // Click() can return before the handler runs, while the renderer still ends the search; ClickAsync waits for it.
        await cut.WaitForElement(".company-search-new").ClickAsync(new MouseEventArgs());

        Assert.Equal(NewName, created);
    }

    private IRenderedComponent<CompanySearch> RenderSearch(
        Action<CompanySummaryDto>? onSelect = null, Action<string>? onCreate = null) =>
        Render<CompanySearch>(parameters => parameters
            .Add(p => p.Id, Id)
            .Add(p => p.OnSelect, onSelect ?? (_ => { }))
            .Add(p => p.OnCreate, onCreate ?? (_ => { })));
}
