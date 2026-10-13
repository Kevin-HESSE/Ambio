using Ambio.Application.Common;
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
    private static readonly DateTimeOffset Later = Now.AddDays(2);

    private readonly Faker<Company> _companyFactory;
    private readonly Faker<NewJobOfferInput> _newJobOfferInputFactory;
    private readonly Faker<JobOfferInput> _jobOfferInputFactory;
    private readonly Faker<JobOfferPostingInput> _jobOfferPostingInputFactory;
    private SqliteServiceProvider _services = null!;

    public JobOfferServiceTests()
    {
        _companyFactory = CompanyFactory.GetGenerator();
        _newJobOfferInputFactory = NewJobOfferInputFactory.GetGenerator();
        _jobOfferInputFactory = JobOfferInputFactory.GetGenerator();
        _jobOfferPostingInputFactory = JobOfferPostingInputFactory.GetGenerator();
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

    [Fact]
    public async Task GetAsync_WithUnknownId_ReturnsNull()
    {
        await new JobOfferStory().WithJobOffers(1, numberOfPosting: 1).SaveAsync(_services);

        Assert.Null(await GetAsync(Guid.CreateVersion7()));
    }

    [Fact]
    public async Task GetAsync_WithOffer_ReturnsEveryFieldAndItsPostings()
    {
        var story = new JobOfferStory().WithJobOffers(1, numberOfPosting: 1);
        await story.SaveAsync(_services);
        var company = story.Company;
        var offer = story.JobOffers[0];
        var posting = offer.Postings[0];

        var details = await GetAsync(offer.Id);

        Assert.NotNull(details);
        Assert.Equal(
            (offer.Id, offer.Title, company.Name, company.Slug, offer.ContractType, offer.RemotePolicy, offer.Location, offer.Salary, offer.Description, Now.UtcDateTime),
            (details.Id, details.Title, details.CompanyName, details.CompanySlug, details.ContractType, details.RemotePolicy, details.Location, details.Salary, details.Description, details.RegisterAt));
        Assert.Equal([new JobOfferPostingDto(posting.Id, posting.Site, posting.Url, Now.UtcDateTime)], details.Postings);
    }

    [Fact]
    public async Task GetAsync_WithPostingsSeenOnSeveralDays_SortsThemFromTheFirstSeen()
    {
        var offer = await AddOfferAsync(numberOfPosting: 1);
        _services.TimeProvider.GetUtcNow().Returns(Later);
        var input = _jobOfferPostingInputFactory.Generate();
        await AddPostingAsync(offer.Id, input);

        var details = await GetAsync(offer.Id);

        Assert.NotNull(details);
        Assert.Equal(
            [(offer.Postings[0].Url, Now.UtcDateTime), (input.Url, Later.UtcDateTime)],
            details.Postings.Select(p => (p.Url, p.SeenAt)));
    }

    [Fact]
    public async Task GetAsync_WithArchivedOffer_ReturnsIt()
    {
        var story = new JobOfferStory().WithJobOffers(1, numberOfPosting: 1, JobOfferFactory.IsArchivedRuleSet);
        await story.SaveAsync(_services);

        Assert.NotNull(await GetAsync(story.JobOffers[0].Id));
    }

    [Fact]
    public async Task UpdateAsync_WithEveryField_SavesThemAndKeepsTheCompanyAndThePostings()
    {
        var story = new JobOfferStory().WithJobOffers(1, numberOfPosting: 2);
        await story.SaveAsync(_services);
        var offer = story.JobOffers[0];
        var input = _jobOfferInputFactory.Generate();

        var result = await UpdateAsync(offer.Id, input);

        Assert.True(result.IsSuccess);
        var saved = await FindAsync(offer.Id);
        Assert.Equal(
            (story.Company.Id, input.Title, input.ContractType, input.RemotePolicy, input.Location, input.Salary, input.Description),
            (saved.CompanyId, saved.Title, saved.ContractType, saved.RemotePolicy, saved.Location, saved.Salary, saved.Description));
        Assert.Equal(offer.Postings.Select(p => p.Id).Order(), saved.Postings.Select(p => p.Id).Order());
    }

    [Fact]
    public async Task UpdateAsync_WithPaddedOrBlankFields_TrimsThemAndStoresBlankAsNull()
    {
        var offer = await AddOfferAsync(numberOfPosting: 1);
        var input = _jobOfferInputFactory.Generate();

        await UpdateAsync(offer.Id, input with { Title = $" {input.Title} ", Location = $" {input.Location} ", Salary = " ", Description = "" });

        var saved = await FindAsync(offer.Id);
        Assert.Equal((input.Title, input.Location, null, null), (saved.Title, saved.Location, saved.Salary, saved.Description));
    }

    [Fact]
    public async Task UpdateAsync_WithUnknownId_ReturnsNotFound()
    {
        var result = await UpdateAsync(Guid.CreateVersion7(), _jobOfferInputFactory.Generate());

        Assert.Equal((false, JobOfferErrors.NotFound), (result.IsSuccess, result.GetMessage));
    }

    [Fact]
    public async Task AddPostingAsync_WithNewUrl_SavesTheTrimmedPosting()
    {
        var offer = await AddOfferAsync(numberOfPosting: 1);
        _services.TimeProvider.GetUtcNow().Returns(Later);
        var input = _jobOfferPostingInputFactory.Generate();

        var result = await AddPostingAsync(offer.Id, input with { Site = $" {input.Site} ", Url = $" {input.Url}\t" });

        Assert.True(result.IsSuccess);
        var saved = await FindAsync(offer.Id);
        Assert.Equal(2, saved.Postings.Count);
        var posting = Assert.Single(saved.Postings, p => p.Url == input.Url);
        Assert.Equal((input.Site, Later.UtcDateTime), (posting.Site, posting.CreatedAt));
    }

    [Fact]
    public async Task AddPostingAsync_WithUrlOfAPostingOfTheOffer_ReturnsPostingAlreadyExists()
    {
        var offer = await AddOfferAsync(numberOfPosting: 1);
        var input = _jobOfferPostingInputFactory.Generate() with { Url = $" {offer.Postings[0].Url} " };

        var result = await AddPostingAsync(offer.Id, input);

        Assert.Equal((false, JobOfferErrors.PostingAlreadyExists), (result.IsSuccess, result.GetMessage));
        Assert.Single((await FindAsync(offer.Id)).Postings);
    }

    [Fact]
    public async Task AddPostingAsync_WithUnknownOffer_ReturnsNotFound()
    {
        var result = await AddPostingAsync(Guid.CreateVersion7(), _jobOfferPostingInputFactory.Generate());

        Assert.Equal((false, JobOfferErrors.NotFound), (result.IsSuccess, result.GetMessage));
    }

    [Fact]
    public async Task RemovePostingAsync_WithSeveralPostings_DeletesIt()
    {
        var offer = await AddOfferAsync(numberOfPosting: 2);
        var removed = offer.Postings[0];

        var result = await RemovePostingAsync(offer.Id, removed.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal(offer.Postings[1].Id, Assert.Single((await FindAsync(offer.Id)).Postings).Id);
        Assert.False(await _services.QueryAsync(context =>
            context.Set<JobOfferPosting>().AnyAsync(p => p.Id == removed.Id, TestContext.Current.CancellationToken)));
    }

    [Fact]
    public async Task RemovePostingAsync_LastPosting_ReturnsLastPostingAndKeepsIt()
    {
        var offer = await AddOfferAsync(numberOfPosting: 1);

        var result = await RemovePostingAsync(offer.Id, offer.Postings[0].Id);

        Assert.Equal((false, JobOfferErrors.LastPosting), (result.IsSuccess, result.GetMessage));
        Assert.Single((await FindAsync(offer.Id)).Postings);
    }

    [Fact]
    public async Task RemovePostingAsync_PostingOfAnotherOffer_ReturnsPostingNotFound()
    {
        var story = new JobOfferStory().WithJobOffers(2, numberOfPosting: 2);
        await story.SaveAsync(_services);
        var (offer, other) = (story.JobOffers[0], story.JobOffers[1]);

        var result = await RemovePostingAsync(offer.Id, other.Postings[0].Id);

        Assert.Equal((false, JobOfferErrors.PostingNotFound), (result.IsSuccess, result.GetMessage));
        Assert.Equal(2, (await FindAsync(other.Id)).Postings.Count);
    }

    [Fact]
    public async Task RemovePostingAsync_WithUnknownOffer_ReturnsNotFound()
    {
        var result = await RemovePostingAsync(Guid.CreateVersion7(), Guid.CreateVersion7());

        Assert.Equal((false, JobOfferErrors.NotFound), (result.IsSuccess, result.GetMessage));
    }

    private async Task<JobOffer> AddOfferAsync(int numberOfPosting)
    {
        var story = new JobOfferStory().WithJobOffers(1, numberOfPosting);
        await story.SaveAsync(_services);
        return story.JobOffers[0];
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

    private Task<JobOfferDetailsDto?> GetAsync(Guid id) =>
        _services.CallAsync((IJobOfferService service) => service.GetAsync(id, TestContext.Current.CancellationToken));

    private Task<ServiceResult> UpdateAsync(Guid id, JobOfferInput input) =>
        _services.CallAsync((IJobOfferService service) => service.UpdateAsync(id, input, TestContext.Current.CancellationToken));

    private Task<ServiceResult> AddPostingAsync(Guid offerId, JobOfferPostingInput input) =>
        _services.CallAsync((IJobOfferService service) => service.AddPostingAsync(offerId, input, TestContext.Current.CancellationToken));

    private Task<ServiceResult> RemovePostingAsync(Guid offerId, Guid postingId) =>
        _services.CallAsync((IJobOfferService service) => service.RemovePostingAsync(offerId, postingId, TestContext.Current.CancellationToken));

    public static TheoryData<string> SameNames =>
    [
        CompanyName,
        "KELVIO",
        " kelvio ",
    ];
}
