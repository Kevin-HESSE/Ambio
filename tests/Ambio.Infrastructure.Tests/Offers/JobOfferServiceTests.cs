using Ambio.Application.Offers;
using Ambio.Application.Offers.Dtos;
using Ambio.Domain.Companies;
using Ambio.Domain.Offers;
using Ambio.Infrastructure.Tests.Fixtures;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using NSubstitute;

namespace Ambio.Infrastructure.Tests.Offers;

public class JobOfferServiceTests : IAsyncLifetime
{
    private const string CompanyName = "Kelvio";
    private const string CompanySlug = "kelvio";
    private const string OtherCompanyName = "Brightwave";
    private const string Title = "Backend Developer";
    private const string Location = "Nantes";
    private const string Salary = "45–55 k€";
    private const string Description = "We are looking for a backend developer.";
    private const string Site = "LinkedIn";
    private const string Url = "https://www.linkedin.com/jobs/view/4102938475";
    private const string OtherUrl = "https://kelvio.io/careers/backend-developer";

    private static readonly DateTimeOffset Now = new(2026, 9, 28, 10, 0, 0, TimeSpan.Zero);

    private SqliteServiceProvider _services = null!;

    public async ValueTask InitializeAsync()
    {
        _services = await SqliteServiceProvider.CreateAsync();
        _services.TimeProvider.GetUtcNow().Returns(Now);
    }

    public ValueTask DisposeAsync() => _services.DisposeAsync();

    [Fact]
    public async Task ListAsync_WithoutOffers_ReturnsEmpty()
    {
        Assert.Empty(await ListAsync());
    }

    [Fact]
    public async Task ListAsync_WithOffer_ReturnsItsSummary()
    {
        var company = await AddCompanyAsync(CompanyName);
        var offer = JobOfferFactory.New(company.Id, Title);
        offer.ContractType = ContractType.FixedTerm;
        offer.RemotePolicy = RemotePolicy.OnSite;
        offer.AddPosting(Site, Url);
        offer.AddPosting("Company careers page", OtherUrl);
        await AddAsync(offer);

        var summary = Assert.Single(await ListAsync());

        Assert.Equal(
            new JobOfferSummaryDto(offer.Id, Title, CompanyName, CompanySlug, ContractType.FixedTerm, RemotePolicy.OnSite, 2, Now.UtcDateTime),
            summary);
    }

    [Fact]
    public async Task ListAsync_WithSeveralOffers_SortsNewestFirst()
    {
        var company = await AddCompanyAsync(CompanyName);
        await AddAtAsync(Now.AddDays(-2), JobOfferFactory.New(company.Id, "Mobile Developer"));
        await AddAtAsync(Now, JobOfferFactory.New(company.Id, "Data Engineer"));
        await AddAtAsync(Now.AddDays(-1), JobOfferFactory.New(company.Id, Title));

        var titles = (await ListAsync()).Select(o => o.Title);

        Assert.Equal(["Data Engineer", Title, "Mobile Developer"], titles);
    }

    [Fact]
    public async Task ListAsync_WithArchivedOffer_LeavesItOut()
    {
        var company = await AddCompanyAsync(CompanyName);
        var archived = JobOfferFactory.New(company.Id, "Mobile Developer");
        archived.ArchivedAt = DateTime.UtcNow;
        await AddAsync(JobOfferFactory.New(company.Id, Title), archived);

        var summary = Assert.Single(await ListAsync());

        Assert.Equal(Title, summary.Title);
    }

    [Fact]
    public async Task ListByCompanyAsync_WithOffersOfSeveralCompanies_ReturnsOnlyThoseOfTheCompany()
    {
        var company = await AddCompanyAsync(CompanyName);
        var otherCompany = await AddCompanyAsync(OtherCompanyName);
        await AddAsync(JobOfferFactory.New(company.Id, Title), JobOfferFactory.New(otherCompany.Id, "Mobile Developer"));

        var summary = Assert.Single(await ListByCompanyAsync(company.Id));

        Assert.Equal((Title, CompanyName), (summary.Title, summary.CompanyName));
    }

    [Fact]
    public async Task ListByCompanyAsync_WithArchivedOffer_LeavesItOut()
    {
        var company = await AddCompanyAsync(CompanyName);
        var archived = JobOfferFactory.New(company.Id, "Mobile Developer");
        archived.ArchivedAt = DateTime.UtcNow;
        await AddAsync(JobOfferFactory.New(company.Id, Title), archived);

        var summary = Assert.Single(await ListByCompanyAsync(company.Id));

        Assert.Equal(Title, summary.Title);
    }

    [Fact]
    public async Task CreateAsync_WithEveryField_SavesTheOfferAndItsFirstPosting()
    {
        var company = await AddCompanyAsync(CompanyName);

        var id = await CreateAsync(new JobOfferInput
        {
            CompanyName = CompanyName,
            Title = Title,
            ContractType = ContractType.Freelance,
            RemotePolicy = RemotePolicy.Hybrid,
            Location = Location,
            Salary = Salary,
            Site = Site,
            Url = Url,
            Description = Description,
        });

        var offer = await FindAsync(id);
        Assert.Equal(
            (company.Id, Title, ContractType.Freelance, RemotePolicy.Hybrid, Location, Salary, Description, Now.UtcDateTime),
            (offer.CompanyId, offer.Title, offer.ContractType, offer.RemotePolicy, offer.Location, offer.Salary, offer.Description, offer.CreatedAt));
        Assert.Null(offer.ArchivedAt);
        var posting = Assert.Single(offer.Postings);
        Assert.Equal((Site, Url, Now.UtcDateTime), (posting.Site, posting.Url, posting.CreatedAt));
    }

    [Fact]
    public async Task CreateAsync_WithPaddedOrBlankFields_TrimsThemAndStoresBlankAsNull()
    {
        var id = await CreateAsync(NewInput() with { Title = $" {Title} ", Location = $" {Location} ", Salary = " " });

        var offer = await FindAsync(id);
        Assert.Equal((Title, Location, null), (offer.Title, offer.Location, offer.Salary));
    }

    [Theory]
    [MemberData(nameof(SameNames))]
    public async Task CreateAsync_WithNameOfExistingCompany_AddsTheOfferToIt(string name)
    {
        var company = await AddCompanyAsync(CompanyName, CompanyKind.RecruitmentAgency);

        var id = await CreateAsync(NewInput() with { CompanyName = name });

        Assert.Equal(company.Id, (await FindAsync(id)).CompanyId);
        Assert.Single(await ListCompaniesAsync());
    }

    [Fact]
    public async Task CreateAsync_WithNameOfArchivedCompany_AddsTheOfferToIt()
    {
        var archived = CompanyFactory.New(CompanyName);
        archived.ArchivedAt = DateTime.UtcNow;
        await AddCompaniesAsync(archived);

        var id = await CreateAsync(NewInput());

        Assert.Equal(archived.Id, (await FindAsync(id)).CompanyId);
    }

    [Fact]
    public async Task CreateAsync_WithNewCompanyName_CreatesTheCompanyAsEmployer()
    {
        await AddCompanyAsync(OtherCompanyName);

        var id = await CreateAsync(NewInput() with { CompanyName = $" {CompanyName} " });

        var offer = await FindAsync(id);
        Assert.Equal((CompanyName, CompanySlug, CompanyKind.Employer), (offer.Company.Name, offer.Company.Slug, offer.Company.Kind));
        Assert.Equal(2, (await ListCompaniesAsync()).Count);
    }

    [Fact]
    public async Task CreateAsync_WithNewCompanyWhoseSlugIsTaken_GivesItTheNextSuffix()
    {
        await AddCompanyAsync("Acme Inc.");

        var id = await CreateAsync(NewInput() with { CompanyName = "Acme, Inc" });

        Assert.Equal("acme-inc-2", (await FindAsync(id)).Company.Slug);
    }

    private static JobOfferInput NewInput() => new()
    {
        CompanyName = CompanyName,
        Title = Title,
        Site = Site,
        Url = Url,
    };

    private async Task<Company> AddCompanyAsync(string name, CompanyKind kind = CompanyKind.Employer)
    {
        var company = CompanyFactory.New(name, kind);
        await AddCompaniesAsync(company);
        return company;
    }

    private async Task AddCompaniesAsync(params Company[] companies)
    {
        await using var context = await _services.CreateDbContextAsync();
        context.Companies.AddRange(companies);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    private async Task AddAsync(params JobOffer[] offers)
    {
        await using var context = await _services.CreateDbContextAsync();
        context.JobOffers.AddRange(offers);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    private async Task AddAtAsync(DateTimeOffset createdAt, JobOffer offer)
    {
        _services.TimeProvider.GetUtcNow().Returns(createdAt);
        await AddAsync(offer);
    }

    private async Task<List<Company>> ListCompaniesAsync()
    {
        await using var context = await _services.CreateDbContextAsync();
        return await context.Companies.ToListAsync(TestContext.Current.CancellationToken);
    }

    private async Task<JobOffer> FindAsync(Guid id)
    {
        await using var context = await _services.CreateDbContextAsync();
        return await context.JobOffers
            .Include(o => o.Company)
            .Include(o => o.Postings)
            .SingleAsync(o => o.Id == id, TestContext.Current.CancellationToken);
    }

    private async Task<IReadOnlyList<JobOfferSummaryDto>> ListAsync()
    {
        await using var scope = _services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IJobOfferService>();
        return await service.ListAsync(TestContext.Current.CancellationToken);
    }

    private async Task<IReadOnlyList<JobOfferSummaryDto>> ListByCompanyAsync(Guid companyId)
    {
        await using var scope = _services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IJobOfferService>();
        return await service.ListByCompanyAsync(companyId, TestContext.Current.CancellationToken);
    }

    private async Task<Guid> CreateAsync(JobOfferInput input)
    {
        await using var scope = _services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IJobOfferService>();
        return await service.CreateAsync(input, TestContext.Current.CancellationToken);
    }

    public static TheoryData<string> SameNames =>
    [
        "Kelvio",
        "KELVIO",
        " kelvio ",
    ];
}
