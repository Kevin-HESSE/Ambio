using Ambio.Application.Common;
using Ambio.Application.Companies;
using Ambio.Application.Companies.Dtos;
using Ambio.Domain.Companies;
using Ambio.Domain.JobApplications;
using Ambio.Domain.Offers;
using Ambio.Infrastructure.Tests.Fixtures;
using Ambio.Infrastructure.Tests.Stories;

using Bogus;

using Microsoft.EntityFrameworkCore;

using NSubstitute;

namespace Ambio.Infrastructure.Tests.Companies;

public class CompanyServiceTests : IAsyncLifetime
{
    private const string CompanyName = "Brightwave";
    private const string CompanySlug = "brightwave";

    private static readonly DateTimeOffset Now = new(2026, 9, 28, 10, 0, 0, TimeSpan.Zero);

    private readonly Faker<Company> _companyFactory;
    private readonly Faker<CompanyInput> _companyInputFactory;
    private SqliteServiceProvider _services = null!;

    public CompanyServiceTests()
    {
        _companyFactory = CompanyFactory.GetGenerator();
        _companyInputFactory = CompanyInputFactory.GetGenerator();
    }

    public async ValueTask InitializeAsync()
    {
        _services = await SqliteServiceProvider.CreateAsync();
        _services.TimeProvider.GetUtcNow().Returns(Now);
    }

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

        Assert.Equal(new CompanySummaryDto(company.Id, company.Slug, company.Name, company.Kind, company.Industry, company.Location, IsArchived: false), summary);
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
    public async Task ListAsync_IncludingArchived_ReturnsEveryCompanyAndFlagsTheArchivedOne()
    {
        var company = _companyFactory.WithCustomName("Atelier Nord").Generate();
        var archivedCompany = _companyFactory.WithCustomName("Brightwave").Generate(CompanyFactory.IsArchivedRuleSet);
        await _services.AddAsync(company, archivedCompany);

        var list = await ListAsync(includeArchived: true);

        Assert.Equal([(company.Id, false), (archivedCompany.Id, true)], list.Companies.Select(c => (c.Id, c.IsArchived)));
    }

    [Fact]
    public async Task ListAsync_WithArchivedCompany_SaysSoWithoutListingIt()
    {
        await _services.AddAsync(_companyFactory.Generate(CompanyFactory.IsArchivedRuleSet));

        var list = await ListAsync(includeArchived: false);

        Assert.Equal((0, true), (list.Companies.Count, list.HasArchived));
    }

    [Fact]
    public async Task ListAsync_WithActiveCompaniesOnly_SaysNoneIsArchived()
    {
        await _services.AddAsync(_companyFactory.Generate());

        Assert.False((await ListAsync(includeArchived: true)).HasArchived);
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
            new CompanyDetailsDto(company.Id, company.Slug, company.Name, company.Kind, company.Industry, company.Size, company.Location, company.Website, company.LinkedInUrl, company.Description, ArchivedAt: null),
            details);
    }

    [Fact]
    public async Task GetAsync_WithArchivedCompany_ReturnsIt()
    {
        var archivedCompany = _companyFactory.Generate(CompanyFactory.IsArchivedRuleSet);

        await _services.AddAsync(archivedCompany);

        var details = await GetAsync(archivedCompany.Slug);

        Assert.Equal((archivedCompany.Id, archivedCompany.ArchivedAt, true), (details?.Id, details?.ArchivedAt, details?.IsArchived));
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

    [Fact]
    public async Task ArchiveAsync_ActiveCompany_ArchivesItWithItsOffersAndApplicationsOnTheSameDay()
    {
        var story = new OfferApplicationStory().WithApplications(1);
        var spontaneous = SpontaneousApplicationFactory.GetGenerator(story.Company.Id).Generate();
        await story.SaveAsync(_services);
        await _services.AddAsync(spontaneous);

        var result = await ArchiveAsync(story.Company.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal(
            [Now.UtcDateTime, Now.UtcDateTime, Now.UtcDateTime, Now.UtcDateTime],
            [
                (await FindAsync(story.Company.Id)).ArchivedAt,
                (await FindOfferAsync(story.JobOffer.Id)).ArchivedAt,
                (await FindApplicationAsync(story.Applications[0].Id)).ArchivedAt,
                (await FindApplicationAsync(spontaneous.Id)).ArchivedAt,
            ]);
    }

    [Fact]
    public async Task ArchiveAsync_WithRowsArchivedBefore_KeepsTheirDay()
    {
        var story = new JobOfferStory().WithJobOffers(1, ruleSet: JobOfferFactory.IsArchivedRuleSet);
        var archivedApplication = SpontaneousApplicationFactory.GetGenerator(story.Company.Id).Generate(SpontaneousApplicationFactory.IsArchivedRuleSet);
        await story.SaveAsync(_services);
        await _services.AddAsync(archivedApplication);

        await ArchiveAsync(story.Company.Id);

        Assert.Equal(story.JobOffers[0].ArchivedAt, (await FindOfferAsync(story.JobOffers[0].Id)).ArchivedAt);
        Assert.Equal(archivedApplication.ArchivedAt, (await FindApplicationAsync(archivedApplication.Id)).ArchivedAt);
    }

    [Fact]
    public async Task ArchiveAsync_WithAnotherCompany_LeavesItsRowsActive()
    {
        var story = new OfferApplicationStory().WithApplications(1);
        var other = new OfferApplicationStory().WithApplications(1);
        await story.SaveAsync(_services);
        await other.SaveAsync(_services);

        await ArchiveAsync(story.Company.Id);

        Assert.Equal(
            [null, null, null],
            [
                (await FindAsync(other.Company.Id)).ArchivedAt,
                (await FindOfferAsync(other.JobOffer.Id)).ArchivedAt,
                (await FindApplicationAsync(other.Applications[0].Id)).ArchivedAt,
            ]);
    }

    [Fact]
    public async Task ArchiveAsync_ArchivedCompany_KeepsItsDay()
    {
        var archivedCompany = _companyFactory.Generate(CompanyFactory.IsArchivedRuleSet);
        await _services.AddAsync(archivedCompany);

        var result = await ArchiveAsync(archivedCompany.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal(archivedCompany.ArchivedAt, (await FindAsync(archivedCompany.Id)).ArchivedAt);
    }

    [Fact]
    public async Task ArchiveAsync_WithUnknownId_ReturnsNotFound()
    {
        var result = await ArchiveAsync(Guid.CreateVersion7());

        Assert.Equal((false, CompanyErrors.NotFound), (result.IsSuccess, result.GetMessage));
    }

    [Fact]
    public async Task RestoreAsync_ArchivedCompany_RestoresWhatWasArchivedWithItButNotWhatWasArchivedBefore()
    {
        var story = new JobOfferStory().WithJobOffers(1).WithJobOffers(1, ruleSet: JobOfferFactory.IsArchivedRuleSet);
        var (offer, archivedOffer) = (story.JobOffers[0], story.JobOffers[1]);
        var application = OfferApplicationFactory.GetGenerator(offer.Id).Generate();
        var archivedApplication = SpontaneousApplicationFactory.GetGenerator(story.Company.Id).Generate(SpontaneousApplicationFactory.IsArchivedRuleSet);
        await story.SaveAsync(_services);
        await _services.AddAsync<JobApplication>(application, archivedApplication);
        await ArchiveAsync(story.Company.Id);

        var result = await RestoreAsync(story.Company.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal(
            [null, null, null, archivedOffer.ArchivedAt, archivedApplication.ArchivedAt],
            [
                (await FindAsync(story.Company.Id)).ArchivedAt,
                (await FindOfferAsync(offer.Id)).ArchivedAt,
                (await FindApplicationAsync(application.Id)).ArchivedAt,
                (await FindOfferAsync(archivedOffer.Id)).ArchivedAt,
                (await FindApplicationAsync(archivedApplication.Id)).ArchivedAt,
            ]);
    }

    [Fact]
    public async Task RestoreAsync_ActiveCompany_LeavesItActive()
    {
        var company = _companyFactory.Generate();
        await _services.AddAsync(company);

        var result = await RestoreAsync(company.Id);

        Assert.True(result.IsSuccess);
        Assert.Null((await FindAsync(company.Id)).ArchivedAt);
    }

    [Fact]
    public async Task RestoreAsync_WithUnknownId_ReturnsNotFound()
    {
        var result = await RestoreAsync(Guid.CreateVersion7());

        Assert.Equal((false, CompanyErrors.NotFound), (result.IsSuccess, result.GetMessage));
    }

    private async Task<IReadOnlyList<CompanySummaryDto>> ListAsync() =>
        (await ListAsync(includeArchived: false)).Companies;

    private Task<CompanyListDto> ListAsync(bool includeArchived) =>
        _services.CallAsync((ICompanyService service) => service.ListAsync(includeArchived, TestContext.Current.CancellationToken));

    private Task<IReadOnlyList<CompanySummaryDto>> SearchAsync(string text) =>
        _services.CallAsync((ICompanyService service) => service.SearchAsync(text, TestContext.Current.CancellationToken));

    private Task<CompanyDetailsDto?> GetAsync(string slug) =>
        _services.CallAsync((ICompanyService service) => service.GetAsync(slug, TestContext.Current.CancellationToken));

    private Task<ServiceResult<string>> CreateAsync(CompanyInput input) =>
        _services.CallAsync((ICompanyService service) => service.CreateAsync(input, TestContext.Current.CancellationToken));

    private Task<ServiceResult<string>> UpdateAsync(Guid id, CompanyInput input) =>
        _services.CallAsync((ICompanyService service) => service.UpdateAsync(id, input, TestContext.Current.CancellationToken));

    private Task<ServiceResult> ArchiveAsync(Guid id) =>
        _services.CallAsync((ICompanyService service) => service.ArchiveAsync(id, TestContext.Current.CancellationToken));

    private Task<ServiceResult> RestoreAsync(Guid id) =>
        _services.CallAsync((ICompanyService service) => service.RestoreAsync(id, TestContext.Current.CancellationToken));

    private Task<JobOffer> FindOfferAsync(Guid id) =>
        _services.QueryAsync(context => context.JobOffers.SingleAsync(o => o.Id == id, TestContext.Current.CancellationToken));

    private Task<JobApplication> FindApplicationAsync(Guid id) =>
        _services.QueryAsync(context => context.JobApplications.SingleAsync(a => a.Id == id, TestContext.Current.CancellationToken));

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
