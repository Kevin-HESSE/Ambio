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
        var company = NewCompany(CompanyName, CompanyKind.RecruitmentAgency);
        company.Industry = Industry;
        company.Location = Location;
        await AddAsync(company);

        var summary = Assert.Single(await ListAsync());

        Assert.Equal(new CompanySummaryDto(company.Id, CompanyName, CompanyKind.RecruitmentAgency, Industry, Location), summary);
    }

    [Fact]
    public async Task ListAsync_WithSeveralCompanies_SortsByNameIgnoringCase()
    {
        await AddAsync(NewCompany("kelvio"), NewCompany("Atelier Nord"), NewCompany("Brightwave"));

        var names = (await ListAsync()).Select(c => c.Name);

        Assert.Equal(["Atelier Nord", "Brightwave", "kelvio"], names);
    }

    [Fact]
    public async Task ListAsync_WithArchivedCompany_LeavesItOut()
    {
        var archived = NewCompany("Orbital Freight");
        archived.ArchivedAt = DateTime.UtcNow;
        await AddAsync(NewCompany(CompanyName), archived);

        var summary = Assert.Single(await ListAsync());

        Assert.Equal(CompanyName, summary.Name);
    }

    [Fact]
    public async Task CreateAsync_WithEveryField_SavesTheCompanyAndReturnsItsId()
    {
        var result = await CreateAsync(new CreateCompanyInput
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
        var result = await CreateAsync(new CreateCompanyInput
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
        await AddAsync(NewCompany(CompanyName));

        var result = await CreateAsync(new CreateCompanyInput { Name = name });

        Assert.False(result.IsSuccess);
        Assert.Equal(CompanyErrors.AlreadyExists, result.GetMessage);
        Assert.Single(await ListAsync());
    }

    [Fact]
    public async Task CreateAsync_WithNameOfArchivedCompany_ReturnsAlreadyExists()
    {
        var archived = NewCompany(CompanyName);
        archived.ArchivedAt = DateTime.UtcNow;
        await AddAsync(archived);

        var result = await CreateAsync(new CreateCompanyInput { Name = CompanyName });

        Assert.Equal(CompanyErrors.AlreadyExists, result.GetMessage);
    }

    private static Company NewCompany(string name, CompanyKind kind = CompanyKind.Employer) => new()
    {
        Name = name,
        Kind = kind,
    };

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

    private async Task<ServiceResult<Guid>> CreateAsync(CreateCompanyInput input)
    {
        await using var scope = _services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<ICompanyService>();
        return await service.CreateAsync(input, TestContext.Current.CancellationToken);
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
