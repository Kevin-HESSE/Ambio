using Ambio.Application.Common;
using Ambio.Application.Companies.Dtos;
using Ambio.Application.JobApplications;
using Ambio.Application.JobApplications.Dtos;
using Ambio.Domain.Companies;
using Ambio.Domain.JobApplications;
using Ambio.Infrastructure.Common;
using Ambio.Infrastructure.Tests.Fixtures;
using Ambio.Infrastructure.Tests.Stories;

using Bogus;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using NSubstitute;

namespace Ambio.Infrastructure.Tests.JobApplications;

public class JobApplicationServiceTests : IAsyncLifetime
{
    private const string AlphabetKey = $"{ShortIdOptions.Section}:{nameof(ShortIdOptions.Alphabet)}";
    private const string SizeKey = $"{ShortIdOptions.Section}:{nameof(ShortIdOptions.Size)}";
    private const string ShortIdPattern = "^[0-9]{6}$";
    private const string NewCompanyName = "Lumen Health";
    private const string NewCompanySlug = "lumen-health";
    private const string TargetPosition = "Développeur Backend";
    private const string CompanyName = "Brightwave & Co";
    private const string ApplicationSlug = "developpeur-backend-brightwave-co";
    private const string Comment = "First call with the HR team";
    private const string SearchInTitleAndCompany = "developpeur BRIGHTWAVE";
    private const string SearchInCompany = "brightwave";
    private const string FirstCompanyByName = "Acme";
    private const string LastCompanyByName = "Zephyr Labs";

    private static readonly DateTimeOffset Now = new(2026, 9, 28, 10, 0, 0, TimeSpan.Zero);
    private static readonly DateOnly SentOn = new(2026, 9, 3);

    private readonly Faker<NewOfferApplicationInput> _inputFactory = NewOfferApplicationInputFactory.GetGenerator();
    private readonly Faker<NewSpontaneousApplicationInput> _spontaneousInputFactory = NewSpontaneousApplicationInputFactory.GetGenerator();
    private readonly Faker<OfferApplicationInput> _offerEditFactory = OfferApplicationInputFactory.GetGenerator();
    private readonly Faker<SpontaneousApplicationInput> _spontaneousEditFactory = SpontaneousApplicationInputFactory.GetGenerator();
    private SqliteServiceProvider _services = null!;

    public async ValueTask InitializeAsync() => _services = await CreateServicesAsync();

    public ValueTask DisposeAsync() => _services.DisposeAsync();

    [Fact]
    public async Task ListAsync_WithoutApplications_ReturnsEmpty()
    {
        Assert.Empty(await ListAsync());
    }

    [Fact]
    public async Task ListAsync_WithApplicationViaPosting_ReturnsItsSummary()
    {
        var story = new OfferApplicationStory().WithApplications(1, OfferApplicationFactory.IsSentRuleSet);
        var (offer, application) = (story.JobOffer, story.Applications[0]);
        application.ApplyVia(offer.Postings[0]);
        await story.SaveAsync(_services);

        var summary = Assert.Single(await ListAsync());

        Assert.Equal(
            new JobApplicationSummaryDto(
                application.Id, application.ShortId, ApplicationKind.Offer, offer.Title, story.Company.Name, ApplicationStatus.Applied, application.Channel,
                offer.Postings[0].Site, application.AppliedOn),
            summary);
    }

    [Fact]
    public async Task ListAsync_WithDraft_ReturnsItWithoutPostingNorSendingDay()
    {
        await new OfferApplicationStory().WithApplications(1).SaveAsync(_services);

        var summary = Assert.Single(await ListAsync());

        Assert.Equal((ApplicationStatus.Draft, null, null), (summary.Status, summary.AppliedVia, summary.SentOn));
    }

    [Fact]
    public async Task ListAsync_WithSeveralApplications_SortsBySendingDayOrCreationDayNewestFirst()
    {
        var story = new OfferApplicationStory();
        await story.SaveAsync(_services);
        var generator = OfferApplicationFactory.GetGenerator(story.JobOffer.Id);
        var (sentLongAgo, draftOfToday, sentYesterday, draftOfLastWeek) =
            (generator.Generate(), generator.Generate(), generator.Generate(), generator.Generate());
        sentLongAgo.ChangeStatus(ApplicationStatus.Applied, Now.UtcDateTime, appliedOn: DateOnly.FromDateTime(Now.UtcDateTime).AddDays(-30));
        sentYesterday.ChangeStatus(ApplicationStatus.Applied, Now.UtcDateTime, appliedOn: DateOnly.FromDateTime(Now.UtcDateTime).AddDays(-1));
        // Recorded today, but sent a month ago: it sorts by its sending day.
        await AddAtAsync(Now, sentLongAgo);
        await AddAtAsync(Now.AddHours(-1), draftOfToday);
        await AddAtAsync(Now.AddDays(-1), sentYesterday);
        await AddAtAsync(Now.AddDays(-7), draftOfLastWeek);

        var ids = (await ListAsync()).Select(a => a.Id);

        Assert.Equal([draftOfToday.Id, sentYesterday.Id, draftOfLastWeek.Id, sentLongAgo.Id], ids);
    }

    [Fact]
    public async Task ListAsync_WithSpontaneousApplication_ReturnsItsSummaryWithTheTargetPositionAsTitle()
    {
        var story = new SpontaneousApplicationStory().WithApplications(1, SpontaneousApplicationFactory.IsSentRuleSet);
        var application = story.Applications[0];
        await story.SaveAsync(_services);

        var summary = Assert.Single(await ListAsync());

        Assert.Equal(
            new JobApplicationSummaryDto(
                application.Id, application.ShortId, ApplicationKind.Spontaneous, application.TargetPosition, story.Company.Name,
                ApplicationStatus.Applied, application.Channel, AppliedVia: null, application.AppliedOn),
            summary);
    }

    [Fact]
    public async Task ListAsync_WithBothKinds_SortsThemTogetherNewestFirst()
    {
        var offerStory = new OfferApplicationStory();
        await offerStory.SaveAsync(_services);
        var spontaneousStory = new SpontaneousApplicationStory();
        await spontaneousStory.SaveAsync(_services);
        var (offerOfToday, spontaneousOfYesterday, offerOfLastWeek) = (
            OfferApplicationFactory.GetGenerator(offerStory.JobOffer.Id).Generate(),
            SpontaneousApplicationFactory.GetGenerator(spontaneousStory.Company.Id).Generate(),
            OfferApplicationFactory.GetGenerator(offerStory.JobOffer.Id).Generate());
        await AddAtAsync(Now.AddDays(-7), offerOfLastWeek);
        await AddAtAsync(Now.AddDays(-1), spontaneousOfYesterday);
        await AddAtAsync(Now, offerOfToday);

        var ids = (await ListAsync()).Select(a => a.Id);

        Assert.Equal([offerOfToday.Id, spontaneousOfYesterday.Id, offerOfLastWeek.Id], ids);
    }

    [Fact]
    public async Task ListAsync_WithArchivedApplication_LeavesItOut()
    {
        var story = new OfferApplicationStory()
            .WithApplications(1)
            .WithApplications(1, OfferApplicationFactory.IsArchivedRuleSet);
        await story.SaveAsync(_services);

        var summary = Assert.Single(await ListAsync());

        Assert.Equal(story.Applications[0].Id, summary.Id);
    }

    [Fact]
    public async Task ListByOfferAsync_WithApplicationsToSeveralOffers_ReturnsOnlyThoseOfTheOffer()
    {
        var story = new OfferApplicationStory().WithApplications(1);
        await story.SaveAsync(_services);
        await new OfferApplicationStory().WithApplications(1).SaveAsync(_services);

        var summary = Assert.Single(await ListByOfferAsync(story.JobOffer.Id));

        Assert.Equal(story.Applications[0].Id, summary.Id);
    }

    [Fact]
    public async Task ListByOfferAsync_WithSpontaneousApplicationToTheCompany_LeavesItOut()
    {
        var story = new OfferApplicationStory().WithApplications(1);
        await story.SaveAsync(_services);
        await _services.AddAsync(SpontaneousApplicationFactory.GetGenerator(story.Company.Id).Generate());

        var summary = Assert.Single(await ListByOfferAsync(story.JobOffer.Id));

        Assert.Equal(story.Applications[0].Id, summary.Id);
    }

    [Fact]
    public async Task ListByCompanyAsync_WithBothKinds_ReturnsTheApplicationsToItsOffersAndTheSpontaneousOnes()
    {
        var story = new OfferApplicationStory().WithApplications(1);
        await story.SaveAsync(_services);
        var spontaneous = SpontaneousApplicationFactory.GetGenerator(story.Company.Id).Generate();
        await _services.AddAsync(spontaneous);
        await new SpontaneousApplicationStory().WithApplications(1).SaveAsync(_services);

        var ids = (await ListByCompanyAsync(story.Company.Id)).Select(a => a.Id).Order();

        Assert.Equal(new[] { story.Applications[0].Id, spontaneous.Id }.Order(), ids);
    }

    [Fact]
    public async Task ListByCompanyAsync_WithApplicationsToSeveralCompanies_ReturnsOnlyThoseOfTheCompany()
    {
        var story = new OfferApplicationStory().WithApplications(1);
        await story.SaveAsync(_services);
        await new OfferApplicationStory().WithApplications(1).SaveAsync(_services);

        var summary = Assert.Single(await ListByCompanyAsync(story.Company.Id));

        Assert.Equal((story.Applications[0].Id, story.Company.Name), (summary.Id, summary.CompanyName));
    }

    [Fact]
    public async Task ListAsync_WithApplication_ComputesItsSlugFromTheTitleAndTheCompany()
    {
        var story = new SpontaneousApplicationStory().WithApplications(1);
        story.Applications[0].TargetPosition = TargetPosition;
        story.Company.Name = CompanyName;
        await story.SaveAsync(_services);

        var summary = Assert.Single(await ListAsync());

        Assert.Equal(ApplicationSlug, summary.Slug);
    }

    [Fact]
    public async Task ListAsync_WithStatus_ReturnsOnlyTheApplicationsInIt()
    {
        var story = new OfferApplicationStory()
            .WithApplications(1)
            .WithApplications(1, OfferApplicationFactory.IsSentRuleSet);
        await story.SaveAsync(_services);

        var summary = Assert.Single((await ListAsync(new JobApplicationFilter(Status: ApplicationStatus.Applied))).Applications);

        Assert.Equal(story.Applications[1].Id, summary.Id);
    }

    [Theory]
    [InlineData(ApplicationKind.Offer)]
    [InlineData(ApplicationKind.Spontaneous)]
    public async Task ListAsync_WithKind_ReturnsOnlyTheApplicationsOfThatKind(ApplicationKind kind)
    {
        var story = new OfferApplicationStory().WithApplications(1);
        await story.SaveAsync(_services);
        var spontaneous = SpontaneousApplicationFactory.GetGenerator(story.Company.Id).Generate();
        await _services.AddAsync(spontaneous);

        var summary = Assert.Single((await ListAsync(new JobApplicationFilter(Kind: kind))).Applications);

        Assert.Equal(kind == ApplicationKind.Offer ? story.Applications[0].Id : spontaneous.Id, summary.Id);
    }

    [Fact]
    public async Task ListAsync_WithCompany_ReturnsTheApplicationsToItsOffersAndTheSpontaneousOnes()
    {
        var story = new OfferApplicationStory().WithApplications(1);
        await story.SaveAsync(_services);
        var spontaneous = SpontaneousApplicationFactory.GetGenerator(story.Company.Id).Generate();
        await _services.AddAsync(spontaneous);
        await new OfferApplicationStory().WithApplications(1).SaveAsync(_services);
        await new SpontaneousApplicationStory().WithApplications(1).SaveAsync(_services);

        var ids = (await ListAsync(new JobApplicationFilter(CompanySlug: story.Company.Slug))).Applications.Select(a => a.Id).Order();

        Assert.Equal(new[] { story.Applications[0].Id, spontaneous.Id }.Order(), ids);
    }

    [Theory]
    [InlineData(SearchInTitleAndCompany)]
    [InlineData(SearchInCompany)]
    public async Task ListAsync_WithSearch_MatchesEveryWordInTheTitleAndTheCompanyIgnoringCaseAndAccents(string search)
    {
        var story = new SpontaneousApplicationStory().WithApplications(1);
        story.Applications[0].TargetPosition = TargetPosition;
        story.Company.Name = CompanyName;
        await story.SaveAsync(_services);
        await new SpontaneousApplicationStory().WithApplications(1).SaveAsync(_services);

        var summary = Assert.Single((await ListAsync(new JobApplicationFilter(Search: search))).Applications);

        Assert.Equal(story.Applications[0].Id, summary.Id);
    }

    [Fact]
    public async Task ListAsync_WithSearchMissingAWord_ReturnsNothing()
    {
        var story = new SpontaneousApplicationStory().WithApplications(1);
        story.Applications[0].TargetPosition = TargetPosition;
        await story.SaveAsync(_services);

        var list = await ListAsync(new JobApplicationFilter(Search: SearchInTitleAndCompany));

        Assert.Equal(0, list.Total);
    }

    [Fact]
    public async Task ListAsync_WithStatus_CountsByStatusTheApplicationsMatchingTheOtherCriteria()
    {
        var story = new OfferApplicationStory()
            .WithApplications(2)
            .WithApplications(1, OfferApplicationFactory.IsSentRuleSet);
        await story.SaveAsync(_services);
        await new SpontaneousApplicationStory().WithApplications(1, SpontaneousApplicationFactory.IsSentRuleSet).SaveAsync(_services);

        var list = await ListAsync(new JobApplicationFilter(Status: ApplicationStatus.Applied, CompanySlug: story.Company.Slug));

        Assert.Equal(story.Applications[2].Id, Assert.Single(list.Applications).Id);
        Assert.Equal(1, list.Total);
        Assert.Equal(
            Enum.GetValues<ApplicationStatus>().ToDictionary(
                status => status,
                status => status switch
                {
                    ApplicationStatus.Draft => 2,
                    ApplicationStatus.Applied => 1,
                    _ => 0,
                }),
            list.CountsByStatus);
    }

    [Fact]
    public async Task ListAsync_WithCount_ReturnsTheNewestOnesAndTheTotal()
    {
        var story = new OfferApplicationStory();
        await story.SaveAsync(_services);
        var generator = OfferApplicationFactory.GetGenerator(story.JobOffer.Id);
        var (ofToday, ofYesterday, ofLastWeek) = (generator.Generate(), generator.Generate(), generator.Generate());
        await AddAtAsync(Now.AddDays(-7), ofLastWeek);
        await AddAtAsync(Now, ofToday);
        await AddAtAsync(Now.AddDays(-1), ofYesterday);

        var list = await ListAsync(new JobApplicationFilter(), count: 2);

        Assert.Equal([ofToday.Id, ofYesterday.Id], list.Applications.Select(a => a.Id));
        Assert.Equal(3, list.Total);
    }

    [Fact]
    public async Task ListCompaniesAsync_WithApplicationsOfBothKinds_ReturnsTheirCompaniesOnceByName()
    {
        var offerStory = new OfferApplicationStory().WithApplications(2);
        offerStory.Company.Name = LastCompanyByName;
        await offerStory.SaveAsync(_services);
        await _services.AddAsync(SpontaneousApplicationFactory.GetGenerator(offerStory.Company.Id).Generate());
        var spontaneousStory = new SpontaneousApplicationStory().WithApplications(1);
        spontaneousStory.Company.Name = FirstCompanyByName;
        await spontaneousStory.SaveAsync(_services);

        var companies = await ListCompaniesAsync();

        Assert.Equal([spontaneousStory.Company.Id, offerStory.Company.Id], companies.Select(c => c.Id));
    }

    [Fact]
    public async Task ListCompaniesAsync_CompanyWithoutActiveApplication_LeavesItOut()
    {
        await new OfferApplicationStory().WithApplications(1, OfferApplicationFactory.IsArchivedRuleSet).SaveAsync(_services);
        await new SpontaneousApplicationStory().WithApplications(1, SpontaneousApplicationFactory.IsArchivedRuleSet).SaveAsync(_services);
        await _services.AddAsync(CompanyFactory.GetGenerator().Generate());

        Assert.Empty(await ListCompaniesAsync());
    }

    [Fact]
    public async Task ListCompaniesAsync_ArchivedCompanyWithActiveApplication_KeepsIt()
    {
        var story = new SpontaneousApplicationStory().WithApplications(1);
        story.Company.Archive(Now.UtcDateTime);
        await story.SaveAsync(_services);

        var company = Assert.Single(await ListCompaniesAsync());

        Assert.Equal(story.Company.Id, company.Id);
    }

    [Fact]
    public async Task GetAsync_OfferApplicationViaPosting_ReturnsItsDetails()
    {
        var story = new OfferApplicationStory().WithApplications(1, OfferApplicationFactory.IsSentRuleSet);
        var (offer, application) = (story.JobOffer, story.Applications[0]);
        application.ApplyVia(offer.Postings[0]);
        await story.SaveAsync(_services);

        var details = await GetAsync(application.ShortId);

        // The history has its own tests.
        Assert.Equal(
            new OfferApplicationDetailsDto(
                application.Id, application.ShortId, offer.Title, story.Company.Name, story.Company.Slug, ApplicationStatus.Applied,
                application.Channel, application.ChannelDetail, application.AppliedOn, application.Notes, Now.UtcDateTime,
                details!.StatusHistory, offer.Id, offer.ShortId, offer.Location, offer.ContractType, offer.RemotePolicy, offer.Salary, offer.Postings[0].Id,
                offer.Postings[0].Site, offer.Postings[0].Url),
            details);
    }

    [Fact]
    public async Task GetAsync_DraftWithoutPosting_ReturnsItWithoutPostingNorSendingDay()
    {
        var story = new OfferApplicationStory().WithApplications(1);
        await story.SaveAsync(_services);

        var details = Assert.IsType<OfferApplicationDetailsDto>(await GetAsync(story.Applications[0].ShortId));

        Assert.Equal((ApplicationStatus.Draft, null, null, null), (details.Status, details.SentOn, details.AppliedViaSite, details.AppliedViaUrl));
    }

    [Fact]
    public async Task GetAsync_SpontaneousApplication_ReturnsItsDetailsWithTheTargetPositionAsTitle()
    {
        var story = new SpontaneousApplicationStory().WithApplications(1, SpontaneousApplicationFactory.IsSentRuleSet);
        var application = story.Applications[0];
        await story.SaveAsync(_services);

        var details = await GetAsync(application.ShortId);

        // The history has its own tests.
        Assert.Equal(
            new SpontaneousApplicationDetailsDto(
                application.Id, application.ShortId, application.TargetPosition, story.Company.Name, story.Company.Slug,
                ApplicationStatus.Applied, application.Channel, application.ChannelDetail, application.AppliedOn, application.Notes,
                Now.UtcDateTime, details!.StatusHistory, story.Company.Location),
            details);
    }

    [Fact]
    public async Task GetAsync_ArchivedApplication_ReturnsIt()
    {
        var story = new OfferApplicationStory().WithApplications(1, OfferApplicationFactory.IsArchivedRuleSet);
        await story.SaveAsync(_services);

        var details = await GetAsync(story.Applications[0].ShortId);

        Assert.Equal(story.Applications[0].Id, details?.Id);
    }

    [Fact]
    public async Task GetAsync_UnknownShortId_ReturnsNull()
    {
        var story = new OfferApplicationStory().WithApplications(1);
        await story.SaveAsync(_services);

        Assert.Null(await GetAsync($"{story.Applications[0].ShortId}0"));
    }

    [Fact]
    public async Task GetAsync_WithStatusChanges_ReturnsTheHistoryNewestFirst()
    {
        var story = new OfferApplicationStory().WithApplications(1);
        var application = story.Applications[0];
        application.ChangeStatus(ApplicationStatus.Applied, Now.UtcDateTime, appliedOn: SentOn);
        application.ChangeStatus(ApplicationStatus.Interview, Now.UtcDateTime.AddDays(2), Comment);
        await story.SaveAsync(_services);

        var details = await GetAsync(application.ShortId);

        Assert.Equal(
            [
                new StatusChangeDto(ApplicationStatus.Applied, ApplicationStatus.Interview, Now.UtcDateTime.AddDays(2), Comment),
                new StatusChangeDto(ApplicationStatus.Draft, ApplicationStatus.Applied, Now.UtcDateTime, null),
            ],
            details?.StatusHistory);
    }

    [Fact]
    public async Task GetAsync_Draft_ReturnsAnEmptyHistory()
    {
        var story = new SpontaneousApplicationStory().WithApplications(1);
        await story.SaveAsync(_services);

        var details = await GetAsync(story.Applications[0].ShortId);

        Assert.Empty(details!.StatusHistory);
    }

    [Fact]
    public async Task CreateAsync_Draft_SavesItWithANumericShortId()
    {
        var story = new OfferApplicationStory();
        await story.SaveAsync(_services);
        var input = _inputFactory.Generate() with { JobOfferId = story.JobOffer.Id };

        var result = await CreateAsync(input);

        Assert.True(result.IsSuccess);
        var saved = await FindSingleAsync();
        Assert.Equal(
            (story.JobOffer.Id, ApplicationStatus.Draft, (DateOnly?)null, (Guid?)null, input.Channel, input.ChannelDetail, input.Notes),
            (saved.JobOfferId, saved.Status, saved.AppliedOn, saved.AppliedViaPostingId, saved.Channel, saved.ChannelDetail, saved.Notes));
        Assert.Matches(ShortIdPattern, saved.ShortId);
        Assert.Empty(await FindStatusChangesAsync());
    }

    [Fact]
    public async Task CreateAsync_ToOffer_ReturnsItsShortIdAndSlug()
    {
        var story = new OfferApplicationStory();
        story.JobOffer.Title = TargetPosition;
        story.Company.Name = CompanyName;
        await story.SaveAsync(_services);

        var result = await CreateAsync(_inputFactory.Generate() with { JobOfferId = story.JobOffer.Id });

        Assert.True(result.TryGetValue(out var link));
        Assert.Equal(new JobApplicationLinkDto((await FindSingleAsync()).ShortId, ApplicationSlug), link);
    }

    [Fact]
    public async Task CreateAsync_AlreadySentViaPosting_SavesItAsApplied()
    {
        var story = new OfferApplicationStory(numberOfPosting: 2);
        await story.SaveAsync(_services);
        var posting = story.JobOffer.Postings[1];
        var input = _inputFactory.Generate() with
        {
            JobOfferId = story.JobOffer.Id,
            AppliedViaPostingId = posting.Id,
            AlreadySent = true,
            SentOn = SentOn,
        };

        Assert.True((await CreateAsync(input)).IsSuccess);

        var saved = await FindSingleAsync();
        Assert.Equal((ApplicationStatus.Applied, SentOn, posting.Id), (saved.Status, saved.AppliedOn, saved.AppliedViaPostingId));
        AssertSentAtCreation(saved.Id, Assert.Single(await FindStatusChangesAsync()));
    }

    [Fact]
    public async Task CreateAsync_WithBlankOptionalTexts_StoresThemAsNull()
    {
        var story = new OfferApplicationStory();
        await story.SaveAsync(_services);

        await CreateAsync(_inputFactory.Generate() with { JobOfferId = story.JobOffer.Id, ChannelDetail = " ", Notes = "\n" });

        var saved = await FindSingleAsync();
        Assert.Equal((null, null), (saved.ChannelDetail, saved.Notes));
    }

    [Fact]
    public async Task CreateAsync_UnknownOffer_FailsWithOfferNotFound()
    {
        var result = await CreateAsync(_inputFactory.Generate() with { JobOfferId = Guid.CreateVersion7() });

        Assert.Equal(JobApplicationErrors.OfferNotFound, result.GetMessage);
        Assert.Empty(await ListAsync());
    }

    [Fact]
    public async Task CreateAsync_PostingOfAnotherOffer_FailsWithPostingNotFound()
    {
        var story = new OfferApplicationStory();
        await story.SaveAsync(_services);
        var other = new OfferApplicationStory();
        await other.SaveAsync(_services);

        var result = await CreateAsync(_inputFactory.Generate() with
        {
            JobOfferId = story.JobOffer.Id,
            AppliedViaPostingId = other.JobOffer.Postings[0].Id,
        });

        Assert.Equal(JobApplicationErrors.PostingNotFound, result.GetMessage);
    }

    [Fact]
    public async Task CreateAsync_SpontaneousDraftToExistingCompany_SavesItWithANumericShortId()
    {
        var company = CompanyFactory.GetGenerator().Generate();
        await _services.AddAsync(company);
        var input = _spontaneousInputFactory.Generate() with { CompanyName = company.Name };

        await CreateAsync(input);

        var saved = await FindSingleSpontaneousAsync();
        Assert.Equal(
            (company.Id, input.TargetPosition, ApplicationStatus.Draft, (DateOnly?)null, input.Channel, input.ChannelDetail, input.Notes),
            (saved.CompanyId, saved.TargetPosition, saved.Status, saved.AppliedOn, saved.Channel, saved.ChannelDetail, saved.Notes));
        Assert.Matches(ShortIdPattern, saved.ShortId);
    }

    [Fact]
    public async Task CreateAsync_SpontaneousToCompanyNamedInAnotherCase_ReturnsItsShortIdAndSlug()
    {
        var company = CompanyFactory.GetGenerator().Generate();
        company.Name = CompanyName;
        await _services.AddAsync(company);

        var link = await CreateAsync(_spontaneousInputFactory.Generate() with
        {
            CompanyName = $" {CompanyName.ToUpperInvariant()} ",
            TargetPosition = TargetPosition,
        });

        Assert.Equal(new JobApplicationLinkDto((await FindSingleSpontaneousAsync()).ShortId, ApplicationSlug), link);
    }

    [Fact]
    public async Task CreateAsync_SpontaneousToCompanyNamedInAnotherCaseOrArchived_LinksItWithoutCreatingOne()
    {
        var company = CompanyFactory.GetGenerator().Generate(CompanyFactory.IsArchivedRuleSet);
        await _services.AddAsync(company);

        await CreateAsync(_spontaneousInputFactory.Generate() with { CompanyName = $" {company.Name.ToUpperInvariant()} " });

        Assert.Equal(company.Id, (await FindSingleSpontaneousAsync()).CompanyId);
        Assert.Equal(1, await _services.QueryAsync(context => context.Companies.CountAsync(TestContext.Current.CancellationToken)));
    }

    [Fact]
    public async Task CreateAsync_SpontaneousToUnknownCompany_CreatesItAsEmployerWithItsSlug()
    {
        await CreateAsync(_spontaneousInputFactory.Generate() with { CompanyName = NewCompanyName });

        var company = await _services.QueryAsync(context => context.Companies.SingleAsync(TestContext.Current.CancellationToken));
        Assert.Equal((NewCompanyName, NewCompanySlug, CompanyKind.Employer), (company.Name, company.Slug, company.Kind));
        Assert.Equal(company.Id, (await FindSingleSpontaneousAsync()).CompanyId);
    }

    [Fact]
    public async Task CreateAsync_SpontaneousAlreadySent_SavesItAsApplied()
    {
        await CreateAsync(_spontaneousInputFactory.Generate() with { CompanyName = NewCompanyName, AlreadySent = true, SentOn = SentOn });

        var saved = await FindSingleSpontaneousAsync();
        Assert.Equal((ApplicationStatus.Applied, SentOn), (saved.Status, saved.AppliedOn));
        AssertSentAtCreation(saved.Id, Assert.Single(await FindStatusChangesAsync()));
    }

    [Fact]
    public async Task CreateAsync_SpontaneousWithBlankOptionalTexts_StoresThemAsNull()
    {
        await CreateAsync(_spontaneousInputFactory.Generate() with { CompanyName = NewCompanyName, ChannelDetail = " ", Notes = "\n" });

        var saved = await FindSingleSpontaneousAsync();
        Assert.Equal((null, null), (saved.ChannelDetail, saved.Notes));
    }

    [Fact]
    public async Task CreateAsync_WhenTheDrawnShortIdIsTaken_DrawsAnother()
    {
        await using var services = await CreateServicesAsync(alphabet: "01", size: 1);
        var story = new OfferApplicationStory();
        await story.SaveAsync(services);
        await services.AddAsync(OfferApplicationFactory.GetGenerator(story.JobOffer.Id).RuleFor(a => a.ShortId, "0").Generate());

        await services.CallAsync((IJobApplicationService service) =>
            service.CreateAsync(_inputFactory.Generate() with { JobOfferId = story.JobOffer.Id }, TestContext.Current.CancellationToken));

        var shortIds = await services.QueryAsync(context =>
            context.JobApplications.Select(a => a.ShortId).OrderBy(id => id).ToListAsync(TestContext.Current.CancellationToken));
        Assert.Equal(["0", "1"], shortIds);
    }

    [Fact]
    public async Task CreateAsync_WhenEveryShortIdIsTaken_ThrowsInvalidOperationException()
    {
        await using var services = await CreateServicesAsync(alphabet: "0", size: 1);
        var story = new OfferApplicationStory();
        await story.SaveAsync(services);
        await services.AddAsync(OfferApplicationFactory.GetGenerator(story.JobOffer.Id).RuleFor(a => a.ShortId, "0").Generate());

        await Assert.ThrowsAsync<InvalidOperationException>(() => services.CallAsync((IJobApplicationService service) =>
            service.CreateAsync(_inputFactory.Generate() with { JobOfferId = story.JobOffer.Id }, TestContext.Current.CancellationToken)));
    }

    [Fact]
    public async Task UpdateAsync_OfferApplication_ReplacesItsFieldsAndPostingAndReturnsItsLink()
    {
        var story = new OfferApplicationStory(numberOfPosting: 2).WithApplications(1);
        story.JobOffer.Title = TargetPosition;
        story.Company.Name = CompanyName;
        var application = story.Applications[0];
        application.ApplyVia(story.JobOffer.Postings[0]);
        await story.SaveAsync(_services);
        var posting = story.JobOffer.Postings[1];
        var input = _offerEditFactory.Generate() with { AppliedViaPostingId = posting.Id };

        var result = await UpdateAsync(application.Id, input);

        Assert.True(result.TryGetValue(out var link));
        Assert.Equal(new JobApplicationLinkDto(application.ShortId, ApplicationSlug), link);
        var saved = await FindSingleAsync();
        Assert.Equal(
            (posting.Id, input.Channel, input.ChannelDetail, input.Notes, story.JobOffer.Id, ApplicationStatus.Draft),
            (saved.AppliedViaPostingId, saved.Channel, saved.ChannelDetail, saved.Notes, saved.JobOfferId, saved.Status));
    }

    [Fact]
    public async Task UpdateAsync_WithoutPosting_ForgetsThePostingAndStoresBlankTextsAsNull()
    {
        var story = new OfferApplicationStory().WithApplications(1);
        var application = story.Applications[0];
        application.ApplyVia(story.JobOffer.Postings[0]);
        await story.SaveAsync(_services);

        await UpdateAsync(application.Id, _offerEditFactory.Generate() with { ChannelDetail = " ", Notes = "\n" });

        var saved = await FindSingleAsync();
        Assert.Equal(((Guid?)null, (string?)null, (string?)null), (saved.AppliedViaPostingId, saved.ChannelDetail, saved.Notes));
    }

    [Fact]
    public async Task UpdateAsync_SentApplication_CorrectsTheSendingDay()
    {
        var story = new OfferApplicationStory().WithApplications(1, OfferApplicationFactory.IsSentRuleSet);
        await story.SaveAsync(_services);

        await UpdateAsync(story.Applications[0].Id, _offerEditFactory.Generate() with { AlreadySent = true, SentOn = SentOn });

        var saved = await FindSingleAsync();
        Assert.Equal((ApplicationStatus.Applied, SentOn), (saved.Status, saved.AppliedOn));
    }

    [Fact]
    public async Task UpdateAsync_DraftWithASendingDay_LeavesItADraft()
    {
        var story = new OfferApplicationStory().WithApplications(1);
        await story.SaveAsync(_services);

        await UpdateAsync(story.Applications[0].Id, _offerEditFactory.Generate() with { SentOn = SentOn });

        var saved = await FindSingleAsync();
        Assert.Equal((ApplicationStatus.Draft, (DateOnly?)null), (saved.Status, saved.AppliedOn));
    }

    [Fact]
    public async Task UpdateAsync_PostingOfAnotherOffer_FailsWithPostingNotFound()
    {
        var story = new OfferApplicationStory().WithApplications(1);
        await story.SaveAsync(_services);
        var other = new OfferApplicationStory();
        await other.SaveAsync(_services);

        var result = await UpdateAsync(
            story.Applications[0].Id, _offerEditFactory.Generate() with { AppliedViaPostingId = other.JobOffer.Postings[0].Id });

        Assert.Equal(JobApplicationErrors.PostingNotFound, result.GetMessage);
        Assert.Equal(story.Applications[0].Channel, (await FindSingleAsync()).Channel);
    }

    [Fact]
    public async Task UpdateAsync_UnknownApplication_FailsWithNotFound()
    {
        var result = await UpdateAsync(Guid.CreateVersion7(), _offerEditFactory.Generate());

        Assert.Equal(JobApplicationErrors.NotFound, result.GetMessage);
    }

    [Fact]
    public async Task UpdateAsync_SpontaneousApplication_ReplacesItsFieldsAndReturnsTheSlugOfItsNewPosition()
    {
        var story = new SpontaneousApplicationStory().WithApplications(1);
        story.Company.Name = CompanyName;
        var application = story.Applications[0];
        await story.SaveAsync(_services);
        var input = _spontaneousEditFactory.Generate() with { TargetPosition = $" {TargetPosition} " };

        var result = await UpdateAsync(application.Id, input);

        Assert.True(result.TryGetValue(out var link));
        Assert.Equal(new JobApplicationLinkDto(application.ShortId, ApplicationSlug), link);
        var saved = await FindSingleSpontaneousAsync();
        Assert.Equal(
            (TargetPosition, input.Channel, input.ChannelDetail, input.Notes, story.Company.Id),
            (saved.TargetPosition, saved.Channel, saved.ChannelDetail, saved.Notes, saved.CompanyId));
    }

    [Fact]
    public async Task UpdateAsync_UnknownSpontaneousApplication_FailsWithNotFound()
    {
        var story = new OfferApplicationStory().WithApplications(1);
        await story.SaveAsync(_services);

        // An application of the other kind isn't found either.
        var result = await UpdateAsync(story.Applications[0].Id, _spontaneousEditFactory.Generate());

        Assert.Equal(JobApplicationErrors.NotFound, result.GetMessage);
    }

    [Fact]
    public async Task ChangeStatusAsync_DraftToApplied_SendsItAndRecordsTheChangeWithItsTrimmedComment()
    {
        var story = new OfferApplicationStory().WithApplications(1);
        await story.SaveAsync(_services);
        var application = story.Applications[0];

        var result = await ChangeStatusAsync(
            application.Id, new StatusChangeInput { To = ApplicationStatus.Applied, SentOn = SentOn, Comment = $"  {Comment}\n" });

        Assert.True(result.IsSuccess);
        var saved = await FindSingleAsync();
        Assert.Equal((ApplicationStatus.Applied, SentOn), (saved.Status, saved.AppliedOn));
        var change = Assert.Single(await FindStatusChangesAsync());
        Assert.Equal(
            (application.Id, ApplicationStatus.Draft, ApplicationStatus.Applied, Now.UtcDateTime, Comment),
            (change.JobApplicationId, change.From, change.To, change.ChangedAt, change.Comment));
    }

    [Fact]
    public async Task ChangeStatusAsync_WithBlankComment_StoresItAsNull()
    {
        var story = new OfferApplicationStory().WithApplications(1);
        await story.SaveAsync(_services);

        await ChangeStatusAsync(story.Applications[0].Id, new StatusChangeInput { To = ApplicationStatus.Withdrawn, Comment = " " });

        Assert.Null(Assert.Single(await FindStatusChangesAsync()).Comment);
    }

    [Fact]
    public async Task ChangeStatusAsync_SentApplication_KeepsItsSendingDay()
    {
        var story = new SpontaneousApplicationStory().WithApplications(1, SpontaneousApplicationFactory.IsSentRuleSet);
        await story.SaveAsync(_services);
        var application = story.Applications[0];

        var result = await ChangeStatusAsync(application.Id, new StatusChangeInput { To = ApplicationStatus.Interview, SentOn = SentOn });

        Assert.True(result.IsSuccess);
        var saved = await FindSingleSpontaneousAsync();
        Assert.Equal((ApplicationStatus.Interview, application.AppliedOn), (saved.Status, saved.AppliedOn));
        Assert.Equal(
            [ApplicationStatus.Applied, ApplicationStatus.Interview],
            (await FindStatusChangesAsync()).OrderBy(c => c.ChangedAt).Select(c => c.To));
    }

    [Fact]
    public async Task ChangeStatusAsync_StatusTheWorkflowRefuses_FailsWithStatusNotAllowed()
    {
        var story = new OfferApplicationStory().WithApplications(1);
        await story.SaveAsync(_services);

        var result = await ChangeStatusAsync(story.Applications[0].Id, new StatusChangeInput { To = ApplicationStatus.Interview });

        Assert.Equal(JobApplicationErrors.StatusNotAllowed, result.GetMessage);
        Assert.Equal(ApplicationStatus.Draft, (await FindSingleAsync()).Status);
        Assert.Empty(await FindStatusChangesAsync());
    }

    [Fact]
    public async Task ChangeStatusAsync_UnknownApplication_FailsWithNotFound()
    {
        var result = await ChangeStatusAsync(Guid.CreateVersion7(), new StatusChangeInput { To = ApplicationStatus.Withdrawn });

        Assert.Equal(JobApplicationErrors.NotFound, result.GetMessage);
    }

    [Theory]
    [MemberData(nameof(InvalidShortIdOptions))]
    public async Task ShortIdOptions_WithInvalidAlphabetOrSize_FailValidation(string alphabet, int size)
    {
        await using var services = await CreateServicesAsync(alphabet, size);

        await Assert.ThrowsAsync<OptionsValidationException>(() =>
            services.CallAsync((IJobApplicationService service) => service.ListCompaniesAsync(TestContext.Current.CancellationToken)));
    }

    private async Task<SqliteServiceProvider> CreateServicesAsync(string? alphabet = null, int? size = null)
    {
        var settings = new Dictionary<string, string?>();
        if (alphabet is not null)
        {
            settings[AlphabetKey] = alphabet;
        }
        if (size is not null)
        {
            settings[SizeKey] = size.ToString();
        }

        var services = await SqliteServiceProvider.CreateAsync(settings);
        services.TimeProvider.GetUtcNow().Returns(Now);
        return services;
    }

    private async Task AddAtAsync(DateTimeOffset createdAt, JobApplication application)
    {
        _services.TimeProvider.GetUtcNow().Returns(createdAt);
        await _services.AddAsync(application);
    }

    // Every application is born a draft: one created as already sent records its sending, when it is created.
    private static void AssertSentAtCreation(Guid applicationId, StatusChange change) =>
        Assert.Equal(
            (applicationId, ApplicationStatus.Draft, ApplicationStatus.Applied, Now.UtcDateTime, (string?)null),
            (change.JobApplicationId, change.From, change.To, change.ChangedAt, change.Comment));

    private Task<List<StatusChange>> FindStatusChangesAsync() =>
        _services.QueryAsync(context => context.Set<StatusChange>().ToListAsync(TestContext.Current.CancellationToken));

    private Task<OfferApplication> FindSingleAsync() =>
        _services.QueryAsync(context => context.Set<OfferApplication>().SingleAsync(TestContext.Current.CancellationToken));

    private Task<SpontaneousApplication> FindSingleSpontaneousAsync() =>
        _services.QueryAsync(context => context.Set<SpontaneousApplication>().SingleAsync(TestContext.Current.CancellationToken));

    private async Task<IReadOnlyList<JobApplicationSummaryDto>> ListAsync() =>
        (await ListAsync(new JobApplicationFilter())).Applications;

    private Task<JobApplicationListDto> ListAsync(JobApplicationFilter filter, int count = int.MaxValue) =>
        _services.CallAsync((IJobApplicationService service) => service.ListAsync(filter, count, TestContext.Current.CancellationToken));

    private Task<IReadOnlyList<CompanySummaryDto>> ListCompaniesAsync() =>
        _services.CallAsync((IJobApplicationService service) => service.ListCompaniesAsync(TestContext.Current.CancellationToken));

    private Task<IReadOnlyList<JobApplicationSummaryDto>> ListByOfferAsync(Guid offerId) =>
        _services.CallAsync((IJobApplicationService service) => service.ListByOfferAsync(offerId, TestContext.Current.CancellationToken));

    private Task<IReadOnlyList<JobApplicationSummaryDto>> ListByCompanyAsync(Guid companyId) =>
        _services.CallAsync((IJobApplicationService service) => service.ListByCompanyAsync(companyId, TestContext.Current.CancellationToken));

    private Task<JobApplicationDetailsDto?> GetAsync(string shortId) =>
        _services.CallAsync((IJobApplicationService service) => service.GetAsync(shortId, TestContext.Current.CancellationToken));

    private Task<ServiceResult<JobApplicationLinkDto>> CreateAsync(NewOfferApplicationInput input) =>
        _services.CallAsync((IJobApplicationService service) => service.CreateAsync(input, TestContext.Current.CancellationToken));

    private Task<JobApplicationLinkDto> CreateAsync(NewSpontaneousApplicationInput input) =>
        _services.CallAsync((IJobApplicationService service) => service.CreateAsync(input, TestContext.Current.CancellationToken));

    private Task<ServiceResult<JobApplicationLinkDto>> UpdateAsync(Guid id, OfferApplicationInput input) =>
        _services.CallAsync((IJobApplicationService service) => service.UpdateAsync(id, input, TestContext.Current.CancellationToken));

    private Task<ServiceResult<JobApplicationLinkDto>> UpdateAsync(Guid id, SpontaneousApplicationInput input) =>
        _services.CallAsync((IJobApplicationService service) => service.UpdateAsync(id, input, TestContext.Current.CancellationToken));

    private Task<ServiceResult> ChangeStatusAsync(Guid id, StatusChangeInput input) =>
        _services.CallAsync((IJobApplicationService service) => service.ChangeStatusAsync(id, input, TestContext.Current.CancellationToken));

    public static TheoryData<string, int> InvalidShortIdOptions => new()
    {
        { "0123456789-", 6 },
        { "0123456789ABC", 6 },
        { "0123456789", 0 },
    };
}
