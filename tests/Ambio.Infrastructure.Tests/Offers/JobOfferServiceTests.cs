using Ambio.Application.Common;
using Ambio.Application.Companies;
using Ambio.Application.Offers;
using Ambio.Application.Offers.Dtos;
using Ambio.Domain.Companies;
using Ambio.Domain.JobApplications;
using Ambio.Domain.Offers;
using Ambio.Infrastructure.Common;
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
    private const string Title = "Full-stack Engineer";
    private const string OfferSlug = "full-stack-engineer-kelvio";
    private const string SimilarTitle = "full stack engineer";
    private const string DifferentTitle = "Frontend Developer";
    private const string AccentedTitle = "Développeur backend";
    private const string UnaccentedSearch = "developpeur kelvio";
    private const string AlphabetKey = $"{ShortIdOptions.Section}:{nameof(ShortIdOptions.Alphabet)}";
    private const string SizeKey = $"{ShortIdOptions.Section}:{nameof(ShortIdOptions.Size)}";
    private const string ShortIdPattern = "^[0-9]{6}$";

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
            new JobOfferSummaryDto(offer.Id, offer.ShortId, offer.Title, company.Name, company.Slug, offer.ContractType, offer.RemotePolicy, 2, Now.UtcDateTime, IsArchived: false),
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
    public async Task ListAsync_IncludingArchived_ReturnsEveryOfferAndFlagsTheArchivedOne()
    {
        var story = new JobOfferStory();
        var generator = JobOfferFactory.GetGenerator(story.Company.Id);
        var (offer, archivedOffer) = (generator.Generate(), generator.Generate(JobOfferFactory.IsArchivedRuleSet));
        await story.SaveAsync(_services);
        await AddAtAsync(Now.AddDays(-1), archivedOffer);
        await AddAtAsync(Now, offer);

        var list = await ListAsync(includeArchived: true);

        Assert.Equal([(offer.Id, false), (archivedOffer.Id, true)], list.Offers.Select(o => (o.Id, o.IsArchived)));
    }

    [Fact]
    public async Task ListAsync_WithArchivedOffer_SaysSoWithoutListingIt()
    {
        await new JobOfferStory().WithJobOffers(1, ruleSet: JobOfferFactory.IsArchivedRuleSet).SaveAsync(_services);

        var list = await ListAsync(includeArchived: false);

        Assert.Equal((0, true), (list.Offers.Count, list.HasArchived));
    }

    [Fact]
    public async Task ListAsync_WithActiveOffersOnly_SaysNoneIsArchived()
    {
        await new JobOfferStory().WithJobOffers(1).SaveAsync(_services);

        Assert.False((await ListAsync(includeArchived: true)).HasArchived);
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
    public async Task ListByCompanyAsync_IncludingArchived_ReturnsTheArchivedOfferToo()
    {
        var story = new JobOfferStory().WithJobOffers(1).WithJobOffers(1, ruleSet: JobOfferFactory.IsArchivedRuleSet);
        await story.SaveAsync(_services);

        var offers = await ListByCompanyAsync(story.Company.Id, includeArchived: true);

        Assert.Equal(
            story.JobOffers.Select(o => (o.Id, o.IsArchived)).OrderBy(o => o.Id),
            offers.Select(o => (o.Id, o.IsArchived)).OrderBy(o => o.Id));
    }

    [Fact]
    public async Task SearchAsync_WithWordsOfTitleAndCompany_ReturnsTheOfferSummary()
    {
        var story = new JobOfferStory().WithJobOffers(1, numberOfPosting: 1);
        var (company, offer) = (story.Company, story.JobOffers[0]);
        company.Name = CompanyName;
        offer.Title = AccentedTitle;
        await story.SaveAsync(_services);

        var summary = Assert.Single(await SearchAsync(UnaccentedSearch));

        Assert.Equal(
            new JobOfferSummaryDto(offer.Id, offer.ShortId, AccentedTitle, CompanyName, company.Slug, offer.ContractType, offer.RemotePolicy, 1, Now.UtcDateTime, IsArchived: false),
            summary);
    }

    [Fact]
    public async Task SearchAsync_WithOtherTitleOrArchivedOffer_LeavesThemOut()
    {
        var story = new JobOfferStory()
            .WithJobOffers(2)
            .WithJobOffers(1, ruleSet: JobOfferFactory.IsArchivedRuleSet);
        story.JobOffers[0].Title = Title;
        story.JobOffers[1].Title = DifferentTitle;
        story.JobOffers[2].Title = Title;
        await story.SaveAsync(_services);

        var summary = Assert.Single(await SearchAsync(Title));

        Assert.Equal(story.JobOffers[0].Id, summary.Id);
    }

    [Fact]
    public async Task SearchAsync_WithSeveralMatches_SortsNewestFirst()
    {
        var story = new JobOfferStory().WithJobOffers(2);
        var (older, newer) = (story.JobOffers[0], story.JobOffers[1]);
        older.Title = Title;
        newer.Title = Title;
        await _services.AddAsync(story.Company);
        await AddAtAsync(Now.AddDays(-1), older);
        await AddAtAsync(Now, newer);

        var ids = (await SearchAsync(Title)).Select(o => o.Id);

        Assert.Equal([newer.Id, older.Id], ids);
    }

    [Fact]
    public async Task SearchAsync_WithBlankText_ReturnsEmpty()
    {
        await new JobOfferStory().WithJobOffers(1).SaveAsync(_services);

        Assert.Empty(await SearchAsync(" "));
    }

    [Fact]
    public async Task FindSimilarAsync_WithSimilarTitle_ReturnsTheOfferSummary()
    {
        var story = new JobOfferStory().WithJobOffers(1, numberOfPosting: 2);
        var (company, offer) = (story.Company, story.JobOffers[0]);
        offer.Title = Title;
        await story.SaveAsync(_services);

        var summary = Assert.Single(await FindSimilarAsync(company.Name, SimilarTitle));

        Assert.Equal(
            new JobOfferSummaryDto(offer.Id, offer.ShortId, Title, company.Name, company.Slug, offer.ContractType, offer.RemotePolicy, 2, Now.UtcDateTime, IsArchived: false),
            summary);
    }

    [Fact]
    public async Task FindSimilarAsync_WithCompanyNameInAnotherCase_ReturnsTheOffer()
    {
        var story = new JobOfferStory().WithJobOffers(1);
        story.JobOffers[0].Title = Title;
        await story.SaveAsync(_services);

        var summary = Assert.Single(await FindSimilarAsync($" {story.Company.Name.ToUpperInvariant()} ", Title));

        Assert.Equal(story.JobOffers[0].Id, summary.Id);
    }

    [Fact]
    public async Task FindSimilarAsync_WithDifferentTitleArchivedOfferOrOtherCompany_LeavesThemOut()
    {
        var story = new JobOfferStory()
            .WithJobOffers(2)
            .WithJobOffers(1, ruleSet: JobOfferFactory.IsArchivedRuleSet);
        var (similar, different, archived) = (story.JobOffers[0], story.JobOffers[1], story.JobOffers[2]);
        (similar.Title, different.Title, archived.Title) = (Title, DifferentTitle, Title);
        await story.SaveAsync(_services);
        var otherStory = new JobOfferStory().WithJobOffers(1);
        otherStory.JobOffers[0].Title = Title;
        await otherStory.SaveAsync(_services);

        var summary = Assert.Single(await FindSimilarAsync(story.Company.Name, SimilarTitle));

        Assert.Equal(similar.Id, summary.Id);
    }

    [Fact]
    public async Task FindSimilarAsync_WithUnknownCompany_ReturnsEmpty()
    {
        var story = new JobOfferStory().WithJobOffers(1);
        story.JobOffers[0].Title = Title;
        await story.SaveAsync(_services);

        Assert.Empty(await FindSimilarAsync(CompanyName, Title));
    }

    [Fact]
    public async Task FindSimilarAsync_WithSeveralSimilarOffers_SortsNewestFirst()
    {
        var story = new JobOfferStory().WithJobOffers(2);
        var (oldest, newest) = (story.JobOffers[0], story.JobOffers[1]);
        (oldest.Title, newest.Title) = (Title, SimilarTitle);
        await _services.AddAsync(story.Company);
        await AddAtAsync(Now.AddDays(-1), oldest);
        await AddAtAsync(Now, newest);

        var ids = (await FindSimilarAsync(story.Company.Name, Title)).Select(o => o.Id);

        Assert.Equal([newest.Id, oldest.Id], ids);
    }

    [Fact]
    public async Task CreateAsync_WithEveryField_SavesTheOfferAndItsFirstPosting()
    {
        var company = await AddCompanyAsync();
        var input = _newJobOfferInputFactory.Generate() with { CompanyName = company.Name };

        var link = await CreateAsync(input);

        var offer = await FindAsync(link.ShortId);
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

        var link = await CreateAsync(input with { Title = $" {input.Title} ", Location = $" {input.Location} ", Salary = " " });

        var offer = await FindAsync(link.ShortId);
        Assert.Equal((input.Title, input.Location, null), (offer.Title, offer.Location, offer.Salary));
    }

    [Theory]
    [MemberData(nameof(SameNames))]
    public async Task CreateAsync_WithNameOfExistingCompany_AddsTheOfferToIt(string name)
    {
        var company = _companyFactory.WithCustomName(CompanyName).Generate();
        await _services.AddAsync(company);

        var link = await CreateAsync(_newJobOfferInputFactory.WithCustomCompanyName(name).Generate());

        Assert.Equal(company.Id, (await FindAsync(link.ShortId)).CompanyId);
        Assert.Single(await ListCompaniesAsync());
    }

    [Fact]
    public async Task CreateAsync_WithNameOfArchivedCompany_AddsTheOfferToIt()
    {
        var archived = _companyFactory.Generate(CompanyFactory.IsArchivedRuleSet);
        await _services.AddAsync(archived);

        var link = await CreateAsync(_newJobOfferInputFactory.WithCustomCompanyName(archived.Name).Generate());

        Assert.Equal(archived.Id, (await FindAsync(link.ShortId)).CompanyId);
    }

    [Fact]
    public async Task CreateAsync_WithNewCompanyName_CreatesTheCompanyAsEmployer()
    {
        await AddCompanyAsync();

        var link = await CreateAsync(_newJobOfferInputFactory.WithCustomCompanyName($" {CompanyName} ").Generate());

        var offer = await FindAsync(link.ShortId);
        Assert.Equal((CompanyName, CompanySlug, CompanyKind.Employer), (offer.Company.Name, offer.Company.Slug, offer.Company.Kind));
        Assert.Equal(2, (await ListCompaniesAsync()).Count);
    }

    [Fact]
    public async Task CreateAsync_WithNewCompanyWhoseSlugIsTaken_GivesItTheNextSuffix()
    {
        await _services.AddAsync(_companyFactory.WithCustomName("Acme Inc.").Generate());

        var link = await CreateAsync(_newJobOfferInputFactory.WithCustomCompanyName("Acme, Inc").Generate());

        Assert.Equal("acme-inc-2", (await FindAsync(link.ShortId)).Company.Slug);
    }

    [Fact]
    public async Task CreateAsync_SavesTheOfferWithANumericShortId()
    {
        var link = await CreateAsync(_newJobOfferInputFactory.Generate());

        Assert.Matches(ShortIdPattern, (await FindAsync(link.ShortId)).ShortId);
    }

    [Fact]
    public async Task CreateAsync_WithNameOfExistingCompanyInAnotherCase_ReturnsItsShortIdAndSlug()
    {
        var company = _companyFactory.WithCustomName(CompanyName).Generate();
        await _services.AddAsync(company);

        var link = await CreateAsync(_newJobOfferInputFactory.WithCustomCompanyName("KELVIO").Generate() with { Title = Title });

        Assert.Equal(new JobOfferLinkDto((await FindAsync(company.Id, Title)).ShortId, OfferSlug), link);
    }

    [Fact]
    public async Task CreateAsync_WhenTheDrawnShortIdIsTaken_DrawsAnother()
    {
        await using var services = await CreateServicesAsync(alphabet: "01", size: 1);
        var story = new JobOfferStory();
        await services.AddAsync(story.Company);
        await services.AddAsync(JobOfferFactory.GetGenerator(story.Company.Id).RuleFor(o => o.ShortId, "0").Generate());

        await services.CallAsync((IJobOfferService service) =>
            service.CreateAsync(_newJobOfferInputFactory.Generate(), TestContext.Current.CancellationToken));

        var shortIds = await services.QueryAsync(context =>
            context.JobOffers.Select(o => o.ShortId).OrderBy(id => id).ToListAsync(TestContext.Current.CancellationToken));
        Assert.Equal(["0", "1"], shortIds);
    }

    [Fact]
    public async Task GetAsync_WithUnknownShortId_ReturnsNull()
    {
        var story = new JobOfferStory().WithJobOffers(1, numberOfPosting: 1);
        await story.SaveAsync(_services);

        Assert.Null(await GetAsync($"{story.JobOffers[0].ShortId}0"));
    }

    [Fact]
    public async Task GetAsync_WithOffer_ReturnsEveryFieldAndItsPostings()
    {
        var story = new JobOfferStory().WithJobOffers(1, numberOfPosting: 1);
        await story.SaveAsync(_services);
        var company = story.Company;
        var offer = story.JobOffers[0];
        var posting = offer.Postings[0];

        var details = await GetAsync(offer.ShortId);

        Assert.NotNull(details);
        Assert.Equal(
            (offer.Id, offer.ShortId, offer.Title, company.Name, company.Slug, offer.ContractType, offer.RemotePolicy, offer.Location, offer.Salary, offer.Description, Now.UtcDateTime),
            (details.Id, details.ShortId, details.Title, details.CompanyName, details.CompanySlug, details.ContractType, details.RemotePolicy, details.Location, details.Salary, details.Description, details.RegisterAt));
        Assert.Equal([new JobOfferPostingDto(posting.Id, posting.Site, posting.Url, Now.UtcDateTime)], details.Postings);
    }

    [Fact]
    public async Task GetAsync_WithPostingsSeenOnSeveralDays_SortsThemFromTheFirstSeen()
    {
        var offer = await AddOfferAsync(numberOfPosting: 1);
        _services.TimeProvider.GetUtcNow().Returns(Later);
        var input = _jobOfferPostingInputFactory.Generate();
        await AddPostingAsync(offer.Id, input);

        var details = await GetAsync(offer.ShortId);

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

        var details = await GetAsync(story.JobOffers[0].ShortId);

        Assert.Equal((story.JobOffers[0].ArchivedAt, true), (details?.ArchivedAt, details?.IsArchived));
    }

    [Fact]
    public async Task UpdateAsync_WithEveryField_SavesThemAndKeepsTheCompanyAndThePostings()
    {
        var story = new JobOfferStory().WithJobOffers(1, numberOfPosting: 2);
        await story.SaveAsync(_services);
        var offer = story.JobOffers[0];
        var input = _jobOfferInputFactory.Generate();

        var result = await UpdateAsync(offer.Id, input);

        Assert.True(result.TryGetValue(out var link));
        Assert.Equal(new JobOfferLinkDto(offer.ShortId, JobOfferSlug.From(input.Title, story.Company.Name)), link);
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

    [Fact]
    public async Task ArchiveAsync_ActiveOffer_ArchivesItWithItsApplicationsOnTheSameDayButNotItsCompany()
    {
        var story = new OfferApplicationStory().WithApplications(2);
        await story.SaveAsync(_services);

        var result = await ArchiveAsync(story.JobOffer.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal(
            [Now.UtcDateTime, Now.UtcDateTime, Now.UtcDateTime, null],
            [
                (await FindAsync(story.JobOffer.Id)).ArchivedAt,
                (await FindApplicationAsync(story.Applications[0].Id)).ArchivedAt,
                (await FindApplicationAsync(story.Applications[1].Id)).ArchivedAt,
                (await FindAsync(story.JobOffer.Id)).Company.ArchivedAt,
            ]);
    }

    [Fact]
    public async Task ArchiveAsync_WithApplicationArchivedBefore_KeepsItsDay()
    {
        var story = new OfferApplicationStory().WithApplications(1, OfferApplicationFactory.IsArchivedRuleSet);
        await story.SaveAsync(_services);

        await ArchiveAsync(story.JobOffer.Id);

        Assert.Equal(story.Applications[0].ArchivedAt, (await FindApplicationAsync(story.Applications[0].Id)).ArchivedAt);
    }

    [Fact]
    public async Task ArchiveAsync_ArchivedOffer_KeepsItsDay()
    {
        var story = new JobOfferStory().WithJobOffers(1, ruleSet: JobOfferFactory.IsArchivedRuleSet);
        await story.SaveAsync(_services);

        var result = await ArchiveAsync(story.JobOffers[0].Id);

        Assert.True(result.IsSuccess);
        Assert.Equal(story.JobOffers[0].ArchivedAt, (await FindAsync(story.JobOffers[0].Id)).ArchivedAt);
    }

    [Fact]
    public async Task ArchiveAsync_WithUnknownId_ReturnsNotFound()
    {
        var result = await ArchiveAsync(Guid.CreateVersion7());

        Assert.Equal((false, JobOfferErrors.NotFound), (result.IsSuccess, result.GetMessage));
    }

    [Fact]
    public async Task RestoreAsync_ArchivedOffer_RestoresTheApplicationsArchivedWithItButNotThoseArchivedBefore()
    {
        var story = new OfferApplicationStory().WithApplications(1).WithApplications(1, OfferApplicationFactory.IsArchivedRuleSet);
        var (application, archivedApplication) = (story.Applications[0], story.Applications[1]);
        await story.SaveAsync(_services);
        await ArchiveAsync(story.JobOffer.Id);

        var result = await RestoreAsync(story.JobOffer.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal(
            [null, null, archivedApplication.ArchivedAt],
            [
                (await FindAsync(story.JobOffer.Id)).ArchivedAt,
                (await FindApplicationAsync(application.Id)).ArchivedAt,
                (await FindApplicationAsync(archivedApplication.Id)).ArchivedAt,
            ]);
    }

    [Fact]
    public async Task RestoreAsync_OfferOfArchivedCompany_RestoresTheCompanyAlone()
    {
        var story = new JobOfferStory().WithJobOffers(2);
        await story.SaveAsync(_services);
        await _services.CallAsync((ICompanyService service) => service.ArchiveAsync(story.Company.Id, TestContext.Current.CancellationToken));

        await RestoreAsync(story.JobOffers[0].Id);

        var (restored, other) = (await FindAsync(story.JobOffers[0].Id), await FindAsync(story.JobOffers[1].Id));
        Assert.Equal([null, null, Now.UtcDateTime], [restored.ArchivedAt, restored.Company.ArchivedAt, other.ArchivedAt]);
    }

    [Fact]
    public async Task RestoreAsync_ActiveOffer_LeavesItActive()
    {
        var offer = await AddOfferAsync(numberOfPosting: 1);

        var result = await RestoreAsync(offer.Id);

        Assert.True(result.IsSuccess);
        Assert.Null((await FindAsync(offer.Id)).ArchivedAt);
    }

    [Fact]
    public async Task RestoreAsync_WithUnknownId_ReturnsNotFound()
    {
        var result = await RestoreAsync(Guid.CreateVersion7());

        Assert.Equal((false, JobOfferErrors.NotFound), (result.IsSuccess, result.GetMessage));
    }

    private async Task<JobOffer> AddOfferAsync(int numberOfPosting)
    {
        var story = new JobOfferStory().WithJobOffers(1, numberOfPosting);
        await story.SaveAsync(_services);
        return story.JobOffers[0];
    }

    private static async Task<SqliteServiceProvider> CreateServicesAsync(string alphabet, int size)
    {
        var services = await SqliteServiceProvider.CreateAsync(new Dictionary<string, string?>
        {
            [AlphabetKey] = alphabet,
            [SizeKey] = size.ToString(),
        });
        services.TimeProvider.GetUtcNow().Returns(Now);
        return services;
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

    private Task<JobApplication> FindApplicationAsync(Guid id) =>
        _services.QueryAsync(context => context.JobApplications.SingleAsync(a => a.Id == id, TestContext.Current.CancellationToken));

    private Task<List<Company>> ListCompaniesAsync() =>
        _services.QueryAsync(context => context.Companies.ToListAsync(TestContext.Current.CancellationToken));

    private Task<JobOffer> FindAsync(Guid id) =>
        _services.QueryAsync(context => context.JobOffers
            .Include(o => o.Company)
            .Include(o => o.Postings)
            .SingleAsync(o => o.Id == id, TestContext.Current.CancellationToken));

    private Task<JobOffer> FindAsync(Guid companyId, string title) =>
        _services.QueryAsync(context => context.JobOffers
            .SingleAsync(o => o.CompanyId == companyId && o.Title == title, TestContext.Current.CancellationToken));

    private Task<JobOffer> FindAsync(string shortId) =>
        _services.QueryAsync(context => context.JobOffers
            .Include(o => o.Company)
            .Include(o => o.Postings)
            .SingleAsync(o => o.ShortId == shortId, TestContext.Current.CancellationToken));

    private async Task<IReadOnlyList<JobOfferSummaryDto>> ListAsync() =>
        (await ListAsync(includeArchived: false)).Offers;

    private Task<JobOfferListDto> ListAsync(bool includeArchived) =>
        _services.CallAsync((IJobOfferService service) => service.ListAsync(includeArchived, TestContext.Current.CancellationToken));

    private Task<IReadOnlyList<JobOfferSummaryDto>> ListByCompanyAsync(Guid companyId, bool includeArchived = false) =>
        _services.CallAsync((IJobOfferService service) => service.ListByCompanyAsync(companyId, includeArchived, TestContext.Current.CancellationToken));

    private Task<IReadOnlyList<JobOfferSummaryDto>> FindSimilarAsync(string companyName, string title) =>
        _services.CallAsync((IJobOfferService service) => service.FindSimilarAsync(companyName, title, TestContext.Current.CancellationToken));

    private Task<IReadOnlyList<JobOfferSummaryDto>> SearchAsync(string text) =>
        _services.CallAsync((IJobOfferService service) => service.SearchAsync(text, TestContext.Current.CancellationToken));

    private Task<JobOfferLinkDto> CreateAsync(NewJobOfferInput input) =>
        _services.CallAsync((IJobOfferService service) => service.CreateAsync(input, TestContext.Current.CancellationToken));

    private Task<JobOfferDetailsDto?> GetAsync(string shortId) =>
        _services.CallAsync((IJobOfferService service) => service.GetAsync(shortId, TestContext.Current.CancellationToken));

    private Task<ServiceResult<JobOfferLinkDto>> UpdateAsync(Guid id, JobOfferInput input) =>
        _services.CallAsync((IJobOfferService service) => service.UpdateAsync(id, input, TestContext.Current.CancellationToken));

    private Task<ServiceResult> AddPostingAsync(Guid offerId, JobOfferPostingInput input) =>
        _services.CallAsync((IJobOfferService service) => service.AddPostingAsync(offerId, input, TestContext.Current.CancellationToken));

    private Task<ServiceResult> RemovePostingAsync(Guid offerId, Guid postingId) =>
        _services.CallAsync((IJobOfferService service) => service.RemovePostingAsync(offerId, postingId, TestContext.Current.CancellationToken));

    private Task<ServiceResult> ArchiveAsync(Guid id) =>
        _services.CallAsync((IJobOfferService service) => service.ArchiveAsync(id, TestContext.Current.CancellationToken));

    private Task<ServiceResult> RestoreAsync(Guid id) =>
        _services.CallAsync((IJobOfferService service) => service.RestoreAsync(id, TestContext.Current.CancellationToken));

    public static TheoryData<string> SameNames =>
    [
        CompanyName,
        "KELVIO",
        " kelvio ",
    ];
}
