using Ambio.Application.Common;
using Ambio.Application.Companies;
using Ambio.Application.Companies.Dtos;
using Ambio.Domain.Companies;
using Ambio.Infrastructure.Tests.Fixtures;

using Bogus;

using Microsoft.EntityFrameworkCore;

namespace Ambio.Infrastructure.Tests.Companies;

public class CompanyServiceTests : IAsyncLifetime
{
    private const string CompanyName = "Brightwave";
    private const string CompanySlug = "brightwave";

    private readonly Faker<Company> _companyFactory;
    private readonly Faker<CompanyInput> _companyInputFactory;
    private SqliteServiceProvider _services = null!;

    public CompanyServiceTests()
    {
        _companyFactory = CompanyFactory.GetGenerator();
        _companyInputFactory = CompanyInputFactory.GetGenerator();
    }

    public async ValueTask InitializeAsync() => _services = await SqliteServiceProvider.CreateAsync();

    public ValueTask DisposeAsync() => _services.DisposeAsync();

    [Fact]
    public async Task ListAsync_WithoutCompanies_ReturnsEmpty()
    {
        Assert.Empty(await ListAsync());
    }

    [Fact]
    public async Task ListAsync_WithCompany_ReturnsItsSummary()
    {
        var company = _companyFactory.Generate();
        await _services.AddAsync(company);

        var summary = Assert.Single(await ListAsync());

        Assert.Equal(new CompanySummaryDto(company.Id, company.Slug, company.Name, company.Kind, company.Industry, company.Location), summary);
    }

    [Fact]
    public async Task ListAsync_WithSeveralCompanies_SortsByNameIgnoringCase()
    {
        var firstCompany = _companyFactory.WithCustomName("kelvio").Generate();
        var secondCompany = _companyFactory.WithCustomName("Atelier Nord").Generate();
        var thirdCompany = _companyFactory.WithCustomName("Brightwave").Generate();
        await _services.AddAsync(firstCompany, secondCompany, thirdCompany);

        var names = (await ListAsync()).Select(c => c.Name);

        Assert.Equal(["Atelier Nord", "Brightwave", "kelvio"], names);
    }

    [Fact]
    public async Task ListAsync_WithArchivedCompany_LeavesItOut()
    {
        var company = _companyFactory.Generate();
        var archivedCompany = _companyFactory.Generate(CompanyFactory.IsArchivedRuleSet);

        await _services.AddAsync(company, archivedCompany);

        var summary = Assert.Single(await ListAsync());

        Assert.Equal(company.Id, summary.Id);
    }

    [Fact]
    public async Task SearchAsync_WithWordsInAnotherOrderWithoutAccents_ReturnsTheMatchingCompaniesByName()
    {
        var lumen = _companyFactory.WithCustomName("Lumen Santé").Generate();
        var another = _companyFactory.WithCustomName("Atelier Santé Lumen").Generate();
        var other = _companyFactory.WithCustomName("Brightwave").Generate();
        await _services.AddAsync(lumen, another, other);

        var names = (await SearchAsync("sante LUMEN")).Select(c => c.Name);

        Assert.Equal([another.Name, lumen.Name], names);
    }

    [Fact]
    public async Task SearchAsync_WithArchivedCompany_LeavesItOut()
    {
        var company = _companyFactory.Generate();
        var archivedCompany = _companyFactory.WithCustomName($"{company.Name} Archived").Generate(CompanyFactory.IsArchivedRuleSet);
        await _services.AddAsync(company, archivedCompany);

        var summary = Assert.Single(await SearchAsync(company.Name));

        Assert.Equal(company.Id, summary.Id);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public async Task SearchAsync_WithBlankText_ReturnsEmpty(string text)
    {
        await _services.AddAsync(_companyFactory.Generate());

        Assert.Empty(await SearchAsync(text));
    }

    [Fact]
    public async Task GetAsync_WithUnknownSlug_ReturnsNull()
    {
        var company = _companyFactory.Generate();
        await _services.AddAsync(company);

        Assert.Null(await GetAsync("kelvio"));
    }

    [Fact]
    public async Task GetAsync_WithCompany_ReturnsEveryField()
    {
        var company = _companyFactory.Generate();
        await _services.AddAsync(company);

        var details = await GetAsync(company.Slug);

        Assert.Equal(
            new CompanyDetailsDto(company.Id, company.Slug, company.Name, company.Kind, company.Industry, company.Size, company.Location, company.Website, company.LinkedInUrl, company.Description),
            details);
    }

    [Fact]
    public async Task GetAsync_WithArchivedCompany_ReturnsIt()
    {
        var archivedCompany = _companyFactory.Generate(CompanyFactory.IsArchivedRuleSet);

        await _services.AddAsync(archivedCompany);

        var details = await GetAsync(archivedCompany.Slug);

        Assert.Equal(archivedCompany.Id, details?.Id);
    }

    [Fact]
    public async Task CreateAsync_WithEveryField_SavesTheCompanyAndReturnsItsSlug()
    {
        var input = _companyInputFactory.Generate();

        var result = await CreateAsync(input);

        Assert.True(result.TryGetValue(out var slug));
        var company = await FindAsync(slug);
        Assert.Equal(input.Name, company.Name);
        Assert.Equal(input.Kind, company.Kind);
        Assert.Equal(input.Industry, company.Industry);
        Assert.Equal(input.Size, company.Size);
        Assert.Equal(input.Location, company.Location);
        Assert.Equal(input.Website, company.Website);
        Assert.Equal(input.LinkedInUrl, company.LinkedInUrl);
        Assert.Equal(input.Description, company.Description);
        Assert.Null(company.ArchivedAt);
    }

    [Fact]
    public async Task CreateAsync_WithPaddedOrBlankFields_TrimsThemAndStoresBlankAsNull()
    {
        var input = _companyInputFactory.Generate();

        var result = await CreateAsync(input with
        {
            Name = $"  {input.Name}  ",
            Industry = "   ",
            Location = $" {input.Location} ",
            Website = "",
        });

        Assert.True(result.TryGetValue(out var slug));
        var company = await FindAsync(slug);
        Assert.Equal(input.Name, company.Name);
        Assert.Null(company.Industry);
        Assert.Equal(input.Location, company.Location);
        Assert.Null(company.Website);
    }

    [Theory]
    [MemberData(nameof(SameNames))]
    public async Task CreateAsync_WithExistingNameIgnoringCase_ReturnsAlreadyExists(string name)
    {
        var company = _companyFactory
            .WithCustomName(CompanyName)
            .Generate();
        var companyInput = _companyInputFactory
            .WithCustomName(name)
            .Generate();

        await _services.AddAsync(company);

        var result = await CreateAsync(companyInput);

        Assert.False(result.IsSuccess);
        Assert.Equal(CompanyErrors.AlreadyExists, result.GetMessage);
        Assert.Single(await ListAsync());
    }

    [Fact]
    public async Task CreateAsync_WithNameOfArchivedCompany_ReturnsAlreadyExists()
    {
        var archivedCompany = _companyFactory.Generate(CompanyFactory.IsArchivedRuleSet);
        var duplicateCompanyInput = _companyInputFactory
            .WithCustomName(archivedCompany.Name)
            .Generate();

        await _services.AddAsync(archivedCompany);

        var result = await CreateAsync(duplicateCompanyInput);

        Assert.Equal(CompanyErrors.AlreadyExists, result.GetMessage);
    }

    [Fact]
    public async Task CreateAsync_WithName_ReturnsSlugOfName()
    {
        var companyInput = _companyInputFactory
            .WithCustomName(CompanyName)
            .Generate();
        var result = await CreateAsync(companyInput);

        Assert.True(result.TryGetValue(out var slug));
        Assert.Equal(CompanySlug, slug);
    }

    [Fact]
    public async Task CreateAsync_WithSlugOfOtherCompanies_AddsFirstFreeSuffix()
    {
        var baseCompany = _companyFactory
            .WithCustomName("Acme Inc.")
            .Generate();
        await _services.AddAsync(baseCompany);

        var second = await CreateAsync(
            _companyInputFactory.WithCustomName("Acme, Inc").Generate());
        var third = await CreateAsync(
            _companyInputFactory.WithCustomName("Acme Inc").Generate());

        Assert.True(second.TryGetValue(out var secondSlug));
        Assert.True(third.TryGetValue(out var thirdSlug));
        Assert.Equal("acme-inc-2", secondSlug);
        Assert.Equal("acme-inc-3", thirdSlug);
    }

    [Fact]
    public async Task CreateAsync_WithReservedSlug_AddsSuffix()
    {
        var result = await CreateAsync(_companyInputFactory.WithCustomName("New").Generate());

        Assert.True(result.TryGetValue(out var slug));
        Assert.Equal("new-2", slug);
    }

    [Fact]
    public async Task CreateAsync_WithoutLetterOrDigit_UsesFallbackSlug()
    {
        var result = await CreateAsync(_companyInputFactory.WithCustomName("!!!").Generate());

        Assert.True(result.TryGetValue(out var slug));
        Assert.Equal(CompanyFactory.SlugFallback, slug);
    }

    [Fact]
    public async Task UpdateAsync_WithEveryField_SavesThem()
    {
        var company = _companyFactory.Generate();
        await _services.AddAsync(company);
        var input = _companyInputFactory.Generate();

        var result = await UpdateAsync(company.Id, input);

        Assert.True(result.IsSuccess);
        var saved = await FindAsync(company.Id);
        Assert.Equal(input.Name, saved.Name);
        Assert.Equal(input.Kind, saved.Kind);
        Assert.Equal(input.Industry, saved.Industry);
        Assert.Equal(input.Size, saved.Size);
        Assert.Equal(input.Location, saved.Location);
        Assert.Equal(input.Website, saved.Website);
        Assert.Equal(input.LinkedInUrl, saved.LinkedInUrl);
        Assert.Equal(input.Description, saved.Description);
    }

    [Fact]
    public async Task UpdateAsync_WithPaddedOrBlankFields_TrimsThemAndStoresBlankAsNull()
    {
        var company = _companyFactory.Generate();
        await _services.AddAsync(company);
        var input = _companyInputFactory.Generate();

        var result = await UpdateAsync(company.Id, input with
        {
            Name = $"  {input.Name}  ",
            Industry = "   ",
            Location = $" {input.Location} ",
            Website = "",
        });

        Assert.True(result.IsSuccess);
        var saved = await FindAsync(company.Id);
        Assert.Equal(input.Name, saved.Name);
        Assert.Null(saved.Industry);
        Assert.Equal(input.Location, saved.Location);
        Assert.Null(saved.Website);
    }

    [Fact]
    public async Task UpdateAsync_WithOwnNameInOtherCase_RenamesIt()
    {
        var company = _companyFactory.WithCustomName("brightwave").Generate();
        await _services.AddAsync(company);

        var result = await UpdateAsync(company.Id, _companyInputFactory.WithCustomName(CompanyName).Generate());

        Assert.True(result.IsSuccess);
        Assert.Equal(CompanyName, (await FindAsync(company.Id)).Name);
    }

    [Theory]
    [MemberData(nameof(SameNames))]
    public async Task UpdateAsync_WithNameOfAnotherCompanyIgnoringCase_ReturnsAlreadyExists(string name)
    {
        var company = _companyFactory.Generate();
        await _services.AddAsync(_companyFactory.WithCustomName(CompanyName).Generate(), company);

        var result = await UpdateAsync(company.Id, _companyInputFactory.WithCustomName(name).Generate());

        Assert.False(result.IsSuccess);
        Assert.Equal(CompanyErrors.AlreadyExists, result.GetMessage);
        Assert.Equal(company.Name, (await FindAsync(company.Id)).Name);
    }

    [Fact]
    public async Task UpdateAsync_WithNameOfArchivedCompany_ReturnsAlreadyExists()
    {
        var archived = _companyFactory
            .Generate(CompanyFactory.IsArchivedRuleSet);
        var company = _companyFactory.Generate();
        await _services.AddAsync(archived, company);

        var result = await UpdateAsync(company.Id, _companyInputFactory.WithCustomName(archived.Name).Generate());

        Assert.Equal(CompanyErrors.AlreadyExists, result.GetMessage);
    }

    [Fact]
    public async Task UpdateAsync_WithUnknownId_ReturnsNotFound()
    {
        var result = await UpdateAsync(Guid.CreateVersion7(), _companyInputFactory.Generate());

        Assert.False(result.IsSuccess);
        Assert.Equal(CompanyErrors.NotFound, result.GetMessage);
    }

    [Fact]
    public async Task UpdateAsync_WithNewName_RecomputesSlugAndReturnsIt()
    {
        var company = _companyFactory.Generate();
        await _services.AddAsync(company);

        var result = await UpdateAsync(company.Id, _companyInputFactory.WithCustomName(CompanyName).Generate());

        Assert.True(result.TryGetValue(out var slug));
        Assert.Equal(CompanySlug, slug);
        Assert.Equal(CompanySlug, (await FindAsync(company.Id)).Slug);
    }

    [Fact]
    public async Task UpdateAsync_WithNewName_LeavesOldSlugUnknown()
    {
        var company = _companyFactory.Generate();
        await _services.AddAsync(company);

        await UpdateAsync(company.Id, _companyInputFactory.Generate());

        Assert.Null(await GetAsync(company.Slug));
    }

    [Fact]
    public async Task UpdateAsync_WithNameGivingOwnSlug_KeepsIt()
    {
        var company = _companyFactory.WithCustomName("Acme Inc").Generate();
        await _services.AddAsync(company);

        await UpdateAsync(company.Id, _companyInputFactory.WithCustomName("Acme Inc.").Generate());

        Assert.Equal("acme-inc", (await FindAsync(company.Id)).Slug);
    }

    [Fact]
    public async Task UpdateAsync_WithoutRenaming_KeepsSlug()
    {
        // acme-inc is free again, but only a rename moves the company to another URL.
        const string defaultName = "Acme, Inc";
        const string expectedSlug = "acme-inc-2";

        var company = _companyFactory.WithCustomName(defaultName).Generate();
        company.Slug = expectedSlug;
        await _services.AddAsync(company);

        var result = await UpdateAsync(company.Id, _companyInputFactory.WithCustomName(defaultName).Generate());

        Assert.True(result.TryGetValue(out var slug));
        Assert.Equal(expectedSlug, slug);
        Assert.Equal(expectedSlug, (await FindAsync(company.Id)).Slug);
    }

    [Fact]
    public async Task UpdateAsync_WithSlugOfAnotherCompany_AddsSuffix()
    {
        var company = _companyFactory.Generate();
        await _services.AddAsync(_companyFactory.WithCustomName("Acme Inc.").Generate(), company);

        await UpdateAsync(company.Id, _companyInputFactory.WithCustomName("Acme, Inc").Generate());

        Assert.Equal("acme-inc-2", (await FindAsync(company.Id)).Slug);
    }

    private Task<IReadOnlyList<CompanySummaryDto>> ListAsync() =>
        _services.CallAsync((ICompanyService service) => service.ListAsync(TestContext.Current.CancellationToken));

    private Task<IReadOnlyList<CompanySummaryDto>> SearchAsync(string text) =>
        _services.CallAsync((ICompanyService service) => service.SearchAsync(text, TestContext.Current.CancellationToken));

    private Task<CompanyDetailsDto?> GetAsync(string slug) =>
        _services.CallAsync((ICompanyService service) => service.GetAsync(slug, TestContext.Current.CancellationToken));

    private Task<ServiceResult<string>> CreateAsync(CompanyInput input) =>
        _services.CallAsync((ICompanyService service) => service.CreateAsync(input, TestContext.Current.CancellationToken));

    private Task<ServiceResult<string>> UpdateAsync(Guid id, CompanyInput input) =>
        _services.CallAsync((ICompanyService service) => service.UpdateAsync(id, input, TestContext.Current.CancellationToken));

    private Task<Company> FindAsync(Guid id) =>
        _services.QueryAsync(context => context.Companies.SingleAsync(c => c.Id == id, TestContext.Current.CancellationToken));

    private Task<Company> FindAsync(string slug) =>
        _services.QueryAsync(context => context.Companies.SingleAsync(c => c.Slug == slug, TestContext.Current.CancellationToken));

    public static TheoryData<string> SameNames =>
    [
        CompanyName,
        "BRIGHTWAVE",
        "  brightwave  ",
    ];
}
