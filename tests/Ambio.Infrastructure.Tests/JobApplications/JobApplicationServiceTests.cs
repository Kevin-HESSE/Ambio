using Ambio.Application.Common;
using Ambio.Application.JobApplications;
using Ambio.Application.JobApplications.Dtos;
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

    private static readonly DateTimeOffset Now = new(2026, 9, 28, 10, 0, 0, TimeSpan.Zero);
    private static readonly DateOnly SentOn = new(2026, 9, 3);

    private readonly Faker<NewOfferApplicationInput> _inputFactory = NewOfferApplicationInputFactory.GetGenerator();
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
                application.Id, offer.Title, story.Company.Name, ApplicationStatus.Applied, application.Channel,
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
        sentLongAgo.MarkAsSent(DateOnly.FromDateTime(Now.UtcDateTime).AddDays(-30));
        sentYesterday.MarkAsSent(DateOnly.FromDateTime(Now.UtcDateTime).AddDays(-1));
        // Recorded today, but sent a month ago: it sorts by its sending day.
        await AddAtAsync(Now, sentLongAgo);
        await AddAtAsync(Now.AddHours(-1), draftOfToday);
        await AddAtAsync(Now.AddDays(-1), sentYesterday);
        await AddAtAsync(Now.AddDays(-7), draftOfLastWeek);

        var ids = (await ListAsync()).Select(a => a.Id);

        Assert.Equal([draftOfToday.Id, sentYesterday.Id, draftOfLastWeek.Id, sentLongAgo.Id], ids);
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
    public async Task ListByCompanyAsync_WithApplicationsToSeveralCompanies_ReturnsOnlyThoseOfTheCompany()
    {
        var story = new OfferApplicationStory().WithApplications(1);
        await story.SaveAsync(_services);
        await new OfferApplicationStory().WithApplications(1).SaveAsync(_services);

        var summary = Assert.Single(await ListByCompanyAsync(story.Company.Id));

        Assert.Equal((story.Applications[0].Id, story.Company.Name), (summary.Id, summary.CompanyName));
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

    [Theory]
    [MemberData(nameof(InvalidShortIdOptions))]
    public async Task ShortIdOptions_WithInvalidAlphabetOrSize_FailValidation(string alphabet, int size)
    {
        await using var services = await CreateServicesAsync(alphabet, size);

        await Assert.ThrowsAsync<OptionsValidationException>(() =>
            services.CallAsync((IJobApplicationService service) => service.ListAsync(TestContext.Current.CancellationToken)));
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

    private async Task AddAtAsync(DateTimeOffset createdAt, OfferApplication application)
    {
        _services.TimeProvider.GetUtcNow().Returns(createdAt);
        await _services.AddAsync(application);
    }

    private Task<OfferApplication> FindSingleAsync() =>
        _services.QueryAsync(context => context.Set<OfferApplication>().SingleAsync(TestContext.Current.CancellationToken));

    private Task<IReadOnlyList<JobApplicationSummaryDto>> ListAsync() =>
        _services.CallAsync((IJobApplicationService service) => service.ListAsync(TestContext.Current.CancellationToken));

    private Task<IReadOnlyList<JobApplicationSummaryDto>> ListByOfferAsync(Guid offerId) =>
        _services.CallAsync((IJobApplicationService service) => service.ListByOfferAsync(offerId, TestContext.Current.CancellationToken));

    private Task<IReadOnlyList<JobApplicationSummaryDto>> ListByCompanyAsync(Guid companyId) =>
        _services.CallAsync((IJobApplicationService service) => service.ListByCompanyAsync(companyId, TestContext.Current.CancellationToken));

    private Task<ServiceResult> CreateAsync(NewOfferApplicationInput input) =>
        _services.CallAsync((IJobApplicationService service) => service.CreateAsync(input, TestContext.Current.CancellationToken));

    public static TheoryData<string, int> InvalidShortIdOptions => new()
    {
        { "0123456789-", 6 },
        { "0123456789ABC", 6 },
        { "0123456789", 0 },
    };
}
