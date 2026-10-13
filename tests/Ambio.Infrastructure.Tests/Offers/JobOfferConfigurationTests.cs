using Ambio.Domain.Offers;
using Ambio.Infrastructure.Tests.Fixtures;
using Ambio.Infrastructure.Tests.Stories;

using Microsoft.EntityFrameworkCore;

namespace Ambio.Infrastructure.Tests.Offers;

public class JobOfferConfigurationTests : IAsyncLifetime
{
    private SqliteServiceProvider _services = null!;

    public async ValueTask InitializeAsync() => _services = await SqliteServiceProvider.CreateAsync();

    public ValueTask DisposeAsync() => _services.DisposeAsync();

    [Fact]
    public async Task SaveChangesAsync_OfferWithPostings_SavesEveryField()
    {
        var story = new JobOfferStory().WithJobOffers(1, numberOfPosting: 2);
        await story.SaveAsync(_services);
        var offer = story.JobOffers[0];

        var saved = await _services.QueryAsync(context => context.JobOffers
            .Include(o => o.Company)
            .Include(o => o.Postings)
            .SingleAsync(TestContext.Current.CancellationToken));

        Assert.Equal(
            (offer.Title, offer.Description, offer.Location, offer.ContractType, offer.RemotePolicy, offer.Salary, story.Company.Name),
            (saved.Title, saved.Description, saved.Location, saved.ContractType, saved.RemotePolicy, saved.Salary, saved.Company.Name));
        Assert.Equal(
            offer.Postings.Select(p => (p.Id, p.Site, p.Url)).Order(),
            saved.Postings.Select(p => (p.Id, p.Site, p.Url)).Order());
    }

    [Fact]
    public async Task SaveChangesAsync_DeletedOffer_DeletesItsPostings()
    {
        await new JobOfferStory()
            .WithJobOffers(1, numberOfPosting: 1)
            .SaveAsync(_services);

        await using (var context = await _services.CreateDbContextAsync())
        {
            context.JobOffers.Remove(await context.JobOffers.Include(o => o.Postings).SingleAsync(TestContext.Current.CancellationToken));
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        Assert.Empty(await _services.QueryAsync(context =>
            context.Set<JobOfferPosting>().ToListAsync(TestContext.Current.CancellationToken)));
    }

    [Fact]
    public async Task SaveChangesAsync_DeletedCompanyWithOffer_IsRefused()
    {
        await new JobOfferStory()
            .WithJobOffers(1)
            .SaveAsync(_services);

        await using var context = await _services.CreateDbContextAsync();
        context.Companies.Remove(await context.Companies.SingleAsync(TestContext.Current.CancellationToken));

        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task SaveChangesAsync_UrlAlreadyPostedForTheOffer_IsRefused()
    {
        var story = new JobOfferStory().WithJobOffers(1, numberOfPosting: 1);
        await story.SaveAsync(_services);
        var posting = story.JobOffers[0].Postings[0];

        // Loaded without its postings, the offer can't see the URL it already has: only the index guards it.
        await using var context = await _services.CreateDbContextAsync();
        var loaded = await context.JobOffers.SingleAsync(TestContext.Current.CancellationToken);
        loaded.AddPosting(posting.Site, posting.Url);

        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task SaveChangesAsync_SameUrlForTwoOffers_SavesBoth()
    {
        var story = new JobOfferStory()
            .WithJobOffers(1, numberOfPosting: 1)
            .WithJobOffers(1);
        var posting = story.JobOffers[0].Postings[0];
        story.JobOffers[1].AddPosting(posting.Site, posting.Url);

        await story.SaveAsync(_services);

        var count = await _services.QueryAsync(context =>
            context.Set<JobOfferPosting>().CountAsync(p => p.Url == posting.Url, TestContext.Current.CancellationToken));
        Assert.Equal(2, count);
    }
}
