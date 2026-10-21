using Ambio.Domain.JobApplications;
using Ambio.Infrastructure.JobApplications;
using Ambio.Infrastructure.Tests.Fixtures;
using Ambio.Infrastructure.Tests.Stories;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Ambio.Infrastructure.Tests.JobApplications;

public class JobApplicationConfigurationTests : IAsyncLifetime
{
    private const string OfferKind = "Offer";
    private const string SpontaneousKind = "Spontaneous";
    private const string Comment = "First call with the HR team";

    private static readonly DateTime ChangedAt = DateTime.UtcNow;

    private SqliteServiceProvider _services = null!;

    public async ValueTask InitializeAsync() => _services = await SqliteServiceProvider.CreateAsync();

    public ValueTask DisposeAsync() => _services.DisposeAsync();

    [Fact]
    public async Task SaveChangesAsync_SentApplicationViaPosting_SavesEveryFieldAsOfferKind()
    {
        var story = new OfferApplicationStory().WithApplications(1, OfferApplicationFactory.IsSentRuleSet);
        var application = story.Applications[0];
        application.ApplyVia(story.JobOffer.Postings[0]);
        await story.SaveAsync(_services);

        var (saved, kind) = await _services.QueryAsync(async context =>
        {
            var row = await context.JobApplications
                .Select(a => new { Application = (OfferApplication)a, Kind = EF.Property<string>(a, JobApplicationConfiguration.Discriminator) })
                .SingleAsync(TestContext.Current.CancellationToken);
            return (row.Application, row.Kind);
        });

        Assert.Equal(
            (application.ShortId, application.Status, application.Channel, application.ChannelDetail, application.AppliedOn, application.Notes),
            (saved.ShortId, saved.Status, saved.Channel, saved.ChannelDetail, saved.AppliedOn, saved.Notes));
        Assert.Equal(
            (story.JobOffer.Id, story.JobOffer.Postings[0].Id, OfferKind),
            (saved.JobOfferId, saved.AppliedViaPostingId, kind));
    }

    [Fact]
    public async Task SaveChangesAsync_SentSpontaneousApplication_SavesEveryFieldAsSpontaneousKind()
    {
        var story = new SpontaneousApplicationStory().WithApplications(1, SpontaneousApplicationFactory.IsSentRuleSet);
        var application = story.Applications[0];
        await story.SaveAsync(_services);

        var (saved, kind) = await _services.QueryAsync(async context =>
        {
            var row = await context.JobApplications
                .Select(a => new { Application = (SpontaneousApplication)a, Kind = EF.Property<string>(a, JobApplicationConfiguration.Discriminator) })
                .SingleAsync(TestContext.Current.CancellationToken);
            return (row.Application, row.Kind);
        });

        Assert.Equal(
            (application.ShortId, application.Status, application.Channel, application.ChannelDetail, application.AppliedOn, application.Notes),
            (saved.ShortId, saved.Status, saved.Channel, saved.ChannelDetail, saved.AppliedOn, saved.Notes));
        Assert.Equal(
            (story.Company.Id, application.TargetPosition, SpontaneousKind),
            (saved.CompanyId, saved.TargetPosition, kind));
    }

    [Fact]
    public async Task SaveChangesAsync_DeletedCompanyWithSpontaneousApplication_IsRefused()
    {
        await new SpontaneousApplicationStory().WithApplications(1).SaveAsync(_services);

        await using var context = await _services.CreateDbContextAsync();
        context.Companies.Remove(await context.Companies.SingleAsync(TestContext.Current.CancellationToken));

        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync(TestContext.Current.CancellationToken));
    }

    [Theory]
    [MemberData(nameof(ColumnsBreakingTheKind))]
    public async Task KindConstraint_RowWithTheColumnsOfTheOtherKind_IsRefused(string kind, string assignment)
    {
        var offerStory = new OfferApplicationStory().WithApplications(1);
        await offerStory.SaveAsync(_services);
        await new SpontaneousApplicationStory().WithApplications(1).SaveAsync(_services);
        var sql = $"""UPDATE "JobApplications" SET {assignment} WHERE "{JobApplicationConfiguration.Discriminator}" = '{kind}'""";

        await using var context = await _services.CreateDbContextAsync();
        var exception = await Assert.ThrowsAsync<SqliteException>(() =>
            context.Database.ExecuteSqlRawAsync(sql, TestContext.Current.CancellationToken));

        Assert.Contains(JobApplicationConfiguration.KindConstraint, exception.Message);
    }

    [Fact]
    public async Task SaveChangesAsync_ShortIdAlreadyTaken_IsRefused()
    {
        var story = new OfferApplicationStory().WithApplications(1);
        await story.SaveAsync(_services);
        var duplicate = OfferApplicationFactory.GetGenerator(story.JobOffer.Id)
            .RuleFor(a => a.ShortId, story.Applications[0].ShortId)
            .Generate();

        await using var context = await _services.CreateDbContextAsync();
        context.JobApplications.Add(duplicate);

        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task SaveChangesAsync_DeletedOfferWithApplication_IsRefused()
    {
        await new OfferApplicationStory().WithApplications(1).SaveAsync(_services);

        await using var context = await _services.CreateDbContextAsync();
        context.JobOffers.Remove(await context.JobOffers.Include(o => o.Postings).SingleAsync(TestContext.Current.CancellationToken));

        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task SaveChangesAsync_RemovedPostingAppliedVia_KeepsTheApplicationWithoutPosting()
    {
        var story = new OfferApplicationStory(numberOfPosting: 2).WithApplications(1);
        var posting = story.JobOffer.Postings[0];
        story.Applications[0].ApplyVia(posting);
        await story.SaveAsync(_services);

        await using (var context = await _services.CreateDbContextAsync())
        {
            var offer = await context.JobOffers.Include(o => o.Postings).SingleAsync(TestContext.Current.CancellationToken);
            offer.RemovePosting(posting.Id);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        var saved = await _services.QueryAsync(context =>
            context.Set<OfferApplication>().SingleAsync(TestContext.Current.CancellationToken));
        Assert.Null(saved.AppliedViaPostingId);
    }

    [Fact]
    public async Task SaveChangesAsync_ApplicationWithStatusChanges_SavesThemWithEveryField()
    {
        var story = new OfferApplicationStory().WithApplications(1, OfferApplicationFactory.IsSentRuleSet);
        var application = story.Applications[0];
        application.ChangeStatus(ApplicationStatus.Interview, ChangedAt, Comment);
        await story.SaveAsync(_services);

        var saved = await _services.QueryAsync(context => context.Set<OfferApplication>()
            .Include(a => a.StatusChanges)
            .SingleAsync(TestContext.Current.CancellationToken));

        Assert.Equal(
            application.StatusChanges.Select(c => (c.Id, c.JobApplicationId, c.From, c.To, c.ChangedAt, c.Comment)),
            saved.StatusChanges.OrderBy(c => c.ChangedAt).Select(c => (c.Id, c.JobApplicationId, c.From, c.To, c.ChangedAt, c.Comment)));
    }

    [Fact]
    public async Task SaveChangesAsync_DeletedApplication_DeletesItsStatusChanges()
    {
        await new OfferApplicationStory().WithApplications(1, OfferApplicationFactory.IsSentRuleSet).SaveAsync(_services);

        await using (var context = await _services.CreateDbContextAsync())
        {
            context.JobApplications.Remove(await context.JobApplications.SingleAsync(TestContext.Current.CancellationToken));
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        Assert.Empty(await _services.QueryAsync(context =>
            context.Set<StatusChange>().ToListAsync(TestContext.Current.CancellationToken)));
    }

    public static TheoryData<string, string> ColumnsBreakingTheKind => new()
    {
        { OfferKind, "\"JobOfferId\" = NULL" },
        { OfferKind, "\"CompanyId\" = (SELECT \"Id\" FROM \"Companies\" LIMIT 1)" },
        { OfferKind, "\"TargetPosition\" = 'Backend Developer'" },
        { SpontaneousKind, "\"CompanyId\" = NULL" },
        { SpontaneousKind, "\"TargetPosition\" = NULL" },
        { SpontaneousKind, "\"JobOfferId\" = (SELECT \"Id\" FROM \"JobOffers\" LIMIT 1)" },
        { SpontaneousKind, "\"AppliedViaPostingId\" = (SELECT \"Id\" FROM \"JobOfferPostings\" LIMIT 1)" },
    };
}
