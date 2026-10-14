using Ambio.Domain.JobApplications;
using Ambio.Infrastructure.JobApplications;
using Ambio.Infrastructure.Tests.Fixtures;
using Ambio.Infrastructure.Tests.Stories;

using Microsoft.EntityFrameworkCore;

namespace Ambio.Infrastructure.Tests.JobApplications;

public class JobApplicationConfigurationTests : IAsyncLifetime
{
    private const string OfferKind = "Offer";

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
}
