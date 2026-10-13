using Ambio.Application.Offers;
using Ambio.Application.Offers.Dtos;
using Ambio.Domain.Companies;
using Ambio.Domain.Offers;
using Ambio.Infrastructure.Tests.Fixtures;
using Ambio.Infrastructure.Tests.Stories;

using Bogus;

using Microsoft.EntityFrameworkCore;

using NSubstitute;

namespace Ambio.Infrastructure.Tests.Offers;

public class JobOfferServiceTests : IAsyncLifetime
{
    private const string CompanyName = "Kelvio";
    private const string CompanySlug = "kelvio";

    private static readonly DateTimeOffset Now = new(2026, 9, 28, 10, 0, 0, TimeSpan.Zero);

    private readonly Faker<Company> _companyFactory;
    private readonly Faker<NewJobOfferInput> _newJobOfferInputFactory;
    private SqliteServiceProvider _services = null!;

    public JobOfferServiceTests()
    {
        _companyFactory = CompanyFactory.GetGenerator();
        _newJobOfferInputFactory = NewJobOfferInputFactory.GetGenerator();
    }

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
        var story = new JobOfferStory().WithJobOffers(1, numberOfPosting: 2);
        await story.SaveAsync(_services);
        var company = story.Company;
        var offer = story.JobOffers[0];

        var summary = Assert.Single(await ListAsync());

        Assert.Equal(
            new JobOfferSummaryDto(offer.Id, offer.Title, company.Name, company.Slug, offer.ContractType, offer.RemotePolicy, 2, Now.UtcDateTime),
            summary);
    }

    [Fact]
    public async Task ListAsync_WithSeveralOffers_SortsNewestFirst()
    {
        var story = new JobOfferStory().WithJobOffers(3);
        var (oldest, newest, middle) =
            (story.JobOffers[0], story.JobOffers[1], story.JobOffers[2]);
        await _services.AddAsync(story.Company);
        await AddAtAsync(Now.AddDays(-2), oldest);
        await AddAtAsync(Now, newest);
        await AddAtAsync(Now.AddDays(-1), middle);

        var ids = (await ListAsync()).Select(o => o.Id);

        Assert.Equal([newest.Id, middle.Id, oldest.Id], ids);
    }

    [Fact]
    public async Task ListAsync_WithArchivedOffer_LeavesItOut()
    {
        var story = new JobOfferStory()
            .WithJobOffers(1)
            .WithJobOffers(1, ruleSet: JobOfferFactory.IsArchivedRuleSet);
        await story.SaveAsync(_services);

        var summary = Assert.Single(await ListAsync());

        Assert.Equal(story.JobOffers[0].Id, summary.Id);
    }

    [Fact]
    public async Task ListByCompanyAsync_WithOffersOfSeveralCompanies_ReturnsOnlyThoseOfTheCompany()
    {
        var story = new JobOfferStory().WithJobOffers(1);
        await story.SaveAsync(_services);
        await new JobOfferStory().WithJobOffers(1).SaveAsync(_services);

        var summary = Assert.Single(await ListByCompanyAsync(story.Company.Id));

        Assert.Equal((story.JobOffers[0].Id, story.Company.Name), (summary.Id, summary.CompanyName));
    }

    [Fact]
    public async Task ListByCompanyAsync_WithArchivedOffer_LeavesItOut()
    {
        var story = new JobOfferStory().WithJobOffers(1).WithJobOffers(1, ruleSet: JobOfferFactory.IsArchivedRuleSet);
        await story.SaveAsync(_services);

        var summary = Assert.Single(await ListByCompanyAsync(story.Company.Id));

        Assert.Equal(story.JobOffers[0].Id, summary.Id);
    }

    [Fact]
    public async Task CreateAsync_WithEveryField_SavesTheOfferAndItsFirstPosting()
    {
        var company = await AddCompanyAsync();
        var input = _newJobOfferInputFactory.Generate() with { CompanyName = company.Name };

        var id = await CreateAsync(input);

        var offer = await FindAsync(id);
        Assert.Equal(
            (company.Id, input.Title, input.ContractType, input.RemotePolicy, input.Location, input.Salary, input.Description, Now.UtcDateTime),
            (offer.CompanyId, offer.Title, offer.ContractType, offer.RemotePolicy, offer.Location, offer.Salary, offer.Description, offer.CreatedAt));
        Assert.Null(offer.ArchivedAt);
        var posting = Assert.Single(offer.Postings);
        Assert.Equal((input.Site, input.Url, Now.UtcDateTime), (posting.Site, posting.Url, posting.CreatedAt));
    }

    [Fact]
    public async Task CreateAsync_WithPaddedOrBlankFields_TrimsThemAndStoresBlankAsNull()
    {
        var input = _newJobOfferInputFactory.Generate();

        var id = await CreateAsync(input with { Title = $" {input.Title} ", Location = $" {input.Location} ", Salary = " " });

        var offer = await FindAsync(id);
        Assert.Equal((input.Title, input.Location, null), (offer.Title, offer.Location, offer.Salary));
    }

    [Theory]
    [MemberData(nameof(SameNames))]
    public async Task CreateAsync_WithNameOfExistingCompany_AddsTheOfferToIt(string name)
    {
        var company = _companyFactory.WithCustomName(CompanyName).Generate();
        await _services.AddAsync(company);

        var id = await CreateAsync(_newJobOfferInputFactory.WithCustomCompanyName(name).Generate());

        Assert.Equal(company.Id, (await FindAsync(id)).CompanyId);
        Assert.Single(await ListCompaniesAsync());
    }

    [Fact]
    public async Task CreateAsync_WithNameOfArchivedCompany_AddsTheOfferToIt()
    {
        var archived = _companyFactory.Generate(CompanyFactory.IsArchivedRuleSet);
        await _services.AddAsync(archived);

        var id = await CreateAsync(_newJobOfferInputFactory.WithCustomCompanyName(archived.Name).Generate());

        Assert.Equal(archived.Id, (await FindAsync(id)).CompanyId);
    }

    [Fact]
    public async Task CreateAsync_WithNewCompanyName_CreatesTheCompanyAsEmployer()
    {
        await AddCompanyAsync();

        var id = await CreateAsync(_newJobOfferInputFactory.WithCustomCompanyName($" {CompanyName} ").Generate());

        var offer = await FindAsync(id);
        Assert.Equal((CompanyName, CompanySlug, CompanyKind.Employer), (offer.Company.Name, offer.Company.Slug, offer.Company.Kind));
        Assert.Equal(2, (await ListCompaniesAsync()).Count);
    }

    [Fact]
    public async Task CreateAsync_WithNewCompanyWhoseSlugIsTaken_GivesItTheNextSuffix()
    {
        await _services.AddAsync(_companyFactory.WithCustomName("Acme Inc.").Generate());

        var id = await CreateAsync(_newJobOfferInputFactory.WithCustomCompanyName("Acme, Inc").Generate());

        Assert.Equal("acme-inc-2", (await FindAsync(id)).Company.Slug);
    }

    private async Task<Company> AddCompanyAsync()
    {
        var company = _companyFactory.Generate();
        await _services.AddAsync(company);
        return company;
    }

    private async Task AddAtAsync(DateTimeOffset createdAt, JobOffer offer)
    {
        _services.TimeProvider.GetUtcNow().Returns(createdAt);
        await _services.AddAsync(offer);
    }

    private Task<List<Company>> ListCompaniesAsync() =>
        _services.QueryAsync(context => context.Companies.ToListAsync(TestContext.Current.CancellationToken));

    private Task<JobOffer> FindAsync(Guid id) =>
        _services.QueryAsync(context => context.JobOffers
            .Include(o => o.Company)
            .Include(o => o.Postings)
            .SingleAsync(o => o.Id == id, TestContext.Current.CancellationToken));

    private Task<IReadOnlyList<JobOfferSummaryDto>> ListAsync() =>
        _services.CallAsync((IJobOfferService service) => service.ListAsync(TestContext.Current.CancellationToken));

    private Task<IReadOnlyList<JobOfferSummaryDto>> ListByCompanyAsync(Guid companyId) =>
        _services.CallAsync((IJobOfferService service) => service.ListByCompanyAsync(companyId, TestContext.Current.CancellationToken));

    private Task<Guid> CreateAsync(NewJobOfferInput input) =>
        _services.CallAsync((IJobOfferService service) => service.CreateAsync(input, TestContext.Current.CancellationToken));

    public static TheoryData<string> SameNames =>
    [
        CompanyName,
        "KELVIO",
        " kelvio ",
    ];
}
