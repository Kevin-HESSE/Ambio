using Ambio.Application.Common;
using Ambio.Application.Companies;
using Ambio.Application.Companies.Dtos;
using Ambio.Domain.Companies;
using Ambio.Infrastructure.Tests.Fixtures;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Ambio.Infrastructure.Tests.Companies;

public class CompanyServiceTests : IAsyncLifetime
{
    private const string CompanyName = "Brightwave";
    private const string CompanySlug = "brightwave";
    private const string OtherCompanyName = "Kelvio";
    private const string Industry = "Software";
    private const string Location = "Lyon";
    private const string Website = "https://brightwave.io";
    private const string LinkedInUrl = "https://www.linkedin.com/company/brightwave";
    private const string Description = "Payroll software for small companies.";

    private SqliteServiceProvider _services = null!;

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
        var company = CompanyFactory.New(CompanyName, CompanyKind.RecruitmentAgency);
        company.Industry = Industry;
        company.Location = Location;
        await AddAsync(company);

        var summary = Assert.Single(await ListAsync());

        Assert.Equal(new CompanySummaryDto(company.Id, CompanyName, CompanyKind.RecruitmentAgency, Industry, Location), summary);
    }

    [Fact]
    public async Task ListAsync_WithSeveralCompanies_SortsByNameIgnoringCase()
    {
        await AddAsync(CompanyFactory.New("kelvio"), CompanyFactory.New("Atelier Nord"), CompanyFactory.New("Brightwave"));

        var names = (await ListAsync()).Select(c => c.Name);

        Assert.Equal(["Atelier Nord", "Brightwave", "kelvio"], names);
    }

    [Fact]
    public async Task ListAsync_WithArchivedCompany_LeavesItOut()
    {
        var archived = CompanyFactory.New("Orbital Freight");
        archived.ArchivedAt = DateTime.UtcNow;
        await AddAsync(CompanyFactory.New(CompanyName), archived);

        var summary = Assert.Single(await ListAsync());

        Assert.Equal(CompanyName, summary.Name);
    }

    [Fact]
    public async Task GetAsync_WithUnknownId_ReturnsNull()
    {
        await AddAsync(CompanyFactory.New(CompanyName));

        Assert.Null(await GetAsync(Guid.CreateVersion7()));
    }

    [Fact]
    public async Task GetAsync_WithCompany_ReturnsEveryField()
    {
        var company = CompanyFactory.New(CompanyName, CompanyKind.ServiceCompany);
        company.Industry = Industry;
        company.Size = CompanySize.Medium;
        company.Location = Location;
        company.Website = Website;
        company.LinkedInUrl = LinkedInUrl;
        company.Description = Description;
        await AddAsync(company);

        var details = await GetAsync(company.Id);

        Assert.Equal(
            new CompanyDetailsDto(company.Id, CompanyName, CompanyKind.ServiceCompany, Industry, CompanySize.Medium, Location, Website, LinkedInUrl, Description),
            details);
    }

    [Fact]
    public async Task GetAsync_WithArchivedCompany_ReturnsIt()
    {
        var archived = CompanyFactory.New(CompanyName);
        archived.ArchivedAt = DateTime.UtcNow;
        await AddAsync(archived);

        var details = await GetAsync(archived.Id);

        Assert.Equal(CompanyName, details?.Name);
    }

    [Fact]
    public async Task CreateAsync_WithEveryField_SavesTheCompanyAndReturnsItsId()
    {
        var result = await CreateAsync(new CompanyInput
        {
            Name = CompanyName,
            Kind = CompanyKind.ServiceCompany,
            Industry = Industry,
            Size = CompanySize.Medium,
            Location = Location,
            Website = Website,
            LinkedInUrl = LinkedInUrl,
            Description = Description,
        });

        Assert.True(result.TryGetValue(out var id));
        var company = await FindAsync(id);
        Assert.Equal(CompanyName, company.Name);
        Assert.Equal(CompanyKind.ServiceCompany, company.Kind);
        Assert.Equal(Industry, company.Industry);
        Assert.Equal(CompanySize.Medium, company.Size);
        Assert.Equal(Location, company.Location);
        Assert.Equal(Website, company.Website);
        Assert.Equal(LinkedInUrl, company.LinkedInUrl);
        Assert.Equal(Description, company.Description);
        Assert.Null(company.ArchivedAt);
    }

    [Fact]
    public async Task CreateAsync_WithPaddedOrBlankFields_TrimsThemAndStoresBlankAsNull()
    {
        var result = await CreateAsync(new CompanyInput
        {
            Name = $"  {CompanyName}  ",
            Industry = "   ",
            Location = $" {Location} ",
            Website = "",
        });

        Assert.True(result.TryGetValue(out var id));
        var company = await FindAsync(id);
        Assert.Equal(CompanyName, company.Name);
        Assert.Null(company.Industry);
        Assert.Equal(Location, company.Location);
        Assert.Null(company.Website);
    }

    [Theory]
    [MemberData(nameof(SameNames))]
    public async Task CreateAsync_WithExistingNameIgnoringCase_ReturnsAlreadyExists(string name)
    {
        await AddAsync(CompanyFactory.New(CompanyName));

        var result = await CreateAsync(new CompanyInput { Name = name });

        Assert.False(result.IsSuccess);
        Assert.Equal(CompanyErrors.AlreadyExists, result.GetMessage);
        Assert.Single(await ListAsync());
    }

    [Fact]
    public async Task CreateAsync_WithNameOfArchivedCompany_ReturnsAlreadyExists()
    {
        var archived = CompanyFactory.New(CompanyName);
        archived.ArchivedAt = DateTime.UtcNow;
        await AddAsync(archived);

        var result = await CreateAsync(new CompanyInput { Name = CompanyName });

        Assert.Equal(CompanyErrors.AlreadyExists, result.GetMessage);
    }

    [Fact]
    public async Task CreateAsync_WithName_StoresItsSlug()
    {
        var result = await CreateAsync(new CompanyInput { Name = CompanyName });

        Assert.True(result.TryGetValue(out var id));
        Assert.Equal(CompanySlug, (await FindAsync(id)).Slug);
    }

    [Fact]
    public async Task CreateAsync_WithSlugOfOtherCompanies_AddsFirstFreeSuffix()
    {
        await AddAsync(CompanyFactory.New("Acme Inc."));

        var second = await CreateAsync(new CompanyInput { Name = "Acme, Inc" });
        var third = await CreateAsync(new CompanyInput { Name = "Acme Inc" });

        Assert.True(second.TryGetValue(out var secondId));
        Assert.True(third.TryGetValue(out var thirdId));
        Assert.Equal("acme-inc-2", (await FindAsync(secondId)).Slug);
        Assert.Equal("acme-inc-3", (await FindAsync(thirdId)).Slug);
    }

    [Fact]
    public async Task CreateAsync_WithReservedSlug_AddsSuffix()
    {
        var result = await CreateAsync(new CompanyInput { Name = "New" });

        Assert.True(result.TryGetValue(out var id));
        Assert.Equal("new-2", (await FindAsync(id)).Slug);
    }

    [Fact]
    public async Task CreateAsync_WithoutLetterOrDigit_UsesFallbackSlug()
    {
        var result = await CreateAsync(new CompanyInput { Name = "!!!" });

        Assert.True(result.TryGetValue(out var id));
        Assert.Equal(CompanyFactory.SlugFallback, (await FindAsync(id)).Slug);
    }

    [Fact]
    public async Task UpdateAsync_WithEveryField_SavesThem()
    {
        var company = CompanyFactory.New(OtherCompanyName);
        await AddAsync(company);

        var result = await UpdateAsync(company.Id, new CompanyInput
        {
            Name = CompanyName,
            Kind = CompanyKind.ServiceCompany,
            Industry = Industry,
            Size = CompanySize.Medium,
            Location = Location,
            Website = Website,
            LinkedInUrl = LinkedInUrl,
            Description = Description,
        });

        Assert.True(result.IsSuccess);
        var saved = await FindAsync(company.Id);
        Assert.Equal(CompanyName, saved.Name);
        Assert.Equal(CompanyKind.ServiceCompany, saved.Kind);
        Assert.Equal(Industry, saved.Industry);
        Assert.Equal(CompanySize.Medium, saved.Size);
        Assert.Equal(Location, saved.Location);
        Assert.Equal(Website, saved.Website);
        Assert.Equal(LinkedInUrl, saved.LinkedInUrl);
        Assert.Equal(Description, saved.Description);
    }

    [Fact]
    public async Task UpdateAsync_WithPaddedOrBlankFields_TrimsThemAndStoresBlankAsNull()
    {
        var company = CompanyFactory.New(CompanyName);
        company.Industry = Industry;
        company.Website = Website;
        await AddAsync(company);

        var result = await UpdateAsync(company.Id, new CompanyInput
        {
            Name = $"  {CompanyName}  ",
            Industry = "   ",
            Location = $" {Location} ",
            Website = "",
        });

        Assert.True(result.IsSuccess);
        var saved = await FindAsync(company.Id);
        Assert.Equal(CompanyName, saved.Name);
        Assert.Null(saved.Industry);
        Assert.Equal(Location, saved.Location);
        Assert.Null(saved.Website);
    }

    [Fact]
    public async Task UpdateAsync_WithOwnNameInOtherCase_RenamesIt()
    {
        var company = CompanyFactory.New("brightwave");
        await AddAsync(company);

        var result = await UpdateAsync(company.Id, new CompanyInput { Name = CompanyName });

        Assert.True(result.IsSuccess);
        Assert.Equal(CompanyName, (await FindAsync(company.Id)).Name);
    }

    [Theory]
    [MemberData(nameof(SameNames))]
    public async Task UpdateAsync_WithNameOfAnotherCompanyIgnoringCase_ReturnsAlreadyExists(string name)
    {
        var company = CompanyFactory.New(OtherCompanyName);
        await AddAsync(CompanyFactory.New(CompanyName), company);

        var result = await UpdateAsync(company.Id, new CompanyInput { Name = name });

        Assert.False(result.IsSuccess);
        Assert.Equal(CompanyErrors.AlreadyExists, result.GetMessage);
        Assert.Equal(OtherCompanyName, (await FindAsync(company.Id)).Name);
    }

    [Fact]
    public async Task UpdateAsync_WithNameOfArchivedCompany_ReturnsAlreadyExists()
    {
        var archived = CompanyFactory.New(CompanyName);
        archived.ArchivedAt = DateTime.UtcNow;
        var company = CompanyFactory.New(OtherCompanyName);
        await AddAsync(archived, company);

        var result = await UpdateAsync(company.Id, new CompanyInput { Name = CompanyName });

        Assert.Equal(CompanyErrors.AlreadyExists, result.GetMessage);
    }

    [Fact]
    public async Task UpdateAsync_WithUnknownId_ReturnsNotFound()
    {
        var result = await UpdateAsync(Guid.CreateVersion7(), new CompanyInput { Name = CompanyName });

        Assert.False(result.IsSuccess);
        Assert.Equal(CompanyErrors.NotFound, result.GetMessage);
    }

    [Fact]
    public async Task UpdateAsync_WithNewName_RecomputesSlug()
    {
        var company = CompanyFactory.New(OtherCompanyName);
        await AddAsync(company);

        await UpdateAsync(company.Id, new CompanyInput { Name = CompanyName });

        Assert.Equal(CompanySlug, (await FindAsync(company.Id)).Slug);
    }

    [Fact]
    public async Task UpdateAsync_WithNameGivingOwnSlug_KeepsIt()
    {
        var company = CompanyFactory.New("Acme Inc");
        await AddAsync(company);

        await UpdateAsync(company.Id, new CompanyInput { Name = "Acme Inc." });

        Assert.Equal("acme-inc", (await FindAsync(company.Id)).Slug);
    }

    [Fact]
    public async Task UpdateAsync_WithoutRenaming_KeepsSlug()
    {
        // acme-inc is free again, but only a rename moves the company to another URL.
        var company = CompanyFactory.New("Acme, Inc");
        company.Slug = "acme-inc-2";
        await AddAsync(company);

        await UpdateAsync(company.Id, new CompanyInput { Name = "Acme, Inc", Location = Location });

        Assert.Equal("acme-inc-2", (await FindAsync(company.Id)).Slug);
    }

    [Fact]
    public async Task UpdateAsync_WithSlugOfAnotherCompany_AddsSuffix()
    {
        var company = CompanyFactory.New(OtherCompanyName);
        await AddAsync(CompanyFactory.New("Acme Inc."), company);

        await UpdateAsync(company.Id, new CompanyInput { Name = "Acme, Inc" });

        Assert.Equal("acme-inc-2", (await FindAsync(company.Id)).Slug);
    }

    private async Task AddAsync(params Company[] companies)
    {
        await using var context = await _services.CreateDbContextAsync();
        context.Companies.AddRange(companies);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    private async Task<IReadOnlyList<CompanySummaryDto>> ListAsync()
    {
        await using var scope = _services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<ICompanyService>();
        return await service.ListAsync(TestContext.Current.CancellationToken);
    }

    private async Task<CompanyDetailsDto?> GetAsync(Guid id)
    {
        await using var scope = _services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<ICompanyService>();
        return await service.GetAsync(id, TestContext.Current.CancellationToken);
    }

    private async Task<ServiceResult<Guid>> CreateAsync(CompanyInput input)
    {
        await using var scope = _services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<ICompanyService>();
        return await service.CreateAsync(input, TestContext.Current.CancellationToken);
    }

    private async Task<ServiceResult> UpdateAsync(Guid id, CompanyInput input)
    {
        await using var scope = _services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<ICompanyService>();
        return await service.UpdateAsync(id, input, TestContext.Current.CancellationToken);
    }

    private async Task<Company> FindAsync(Guid id)
    {
        await using var context = await _services.CreateDbContextAsync();
        return await context.Companies.SingleAsync(c => c.Id == id, TestContext.Current.CancellationToken);
    }

    public static TheoryData<string> SameNames =>
    [
        CompanyName,
        "BRIGHTWAVE",
        "  brightwave  ",
    ];
}
