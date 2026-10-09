using Ambio.Application.Companies;
using Ambio.Application.Companies.Dtos;
using Ambio.Domain.Companies;
using Ambio.Infrastructure.Tests.Fixtures;

using Microsoft.Extensions.DependencyInjection;

namespace Ambio.Infrastructure.Tests.Companies;

public class CompanyServiceTests : IAsyncLifetime
{
    private const string CompanyName = "Brightwave";
    private const string Industry = "Software";
    private const string Location = "Lyon";

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
}
