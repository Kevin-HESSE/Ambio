using Ambio.Infrastructure.Tests.Fixtures;
using Ambio.Infrastructure.Tests.Stories;

using Microsoft.EntityFrameworkCore;

namespace Ambio.Infrastructure.Tests.CoverLetters;

public class CoverLetterConfigurationTests : IAsyncLifetime
{
    private SqliteServiceProvider _services = null!;

    public async ValueTask InitializeAsync() => _services = await SqliteServiceProvider.CreateAsync();

    public ValueTask DisposeAsync() => _services.DisposeAsync();

    [Fact]
    public async Task SaveChangesAsync_LetterOfAnApplication_SavesEveryField()
    {
        var story = new OfferApplicationStory().WithApplications(1);
        await story.SaveAsync(_services);
        var letter = CoverLetterFactory.GetGenerator(story.Applications[0].Id).Generate();

        await _services.AddAsync(letter);

        var saved = await _services.QueryAsync(context =>
            context.CoverLetters.SingleAsync(TestContext.Current.CancellationToken));
        Assert.Equal(
            (letter.Id, letter.JobApplicationId, letter.Title, letter.Content),
            (saved.Id, saved.JobApplicationId, saved.Title, saved.Content));
    }

    [Fact]
    public async Task SaveChangesAsync_LetterOfAnUnknownApplication_IsRefused()
    {
        var letter = CoverLetterFactory.GetGenerator(Guid.CreateVersion7()).Generate();

        await Assert.ThrowsAsync<DbUpdateException>(() => _services.AddAsync(letter));
    }

    [Fact]
    public async Task SaveChangesAsync_DeletedApplication_DeletesItsLetters()
    {
        var story = new OfferApplicationStory().WithApplications(2);
        await story.SaveAsync(_services);
        var kept = CoverLetterFactory.GetGenerator(story.Applications[1].Id).Generate();
        await _services.AddAsync([.. CoverLetterFactory.GetGenerator(story.Applications[0].Id).Generate(2), kept]);

        await using (var context = await _services.CreateDbContextAsync())
        {
            context.JobApplications.Remove(await context.JobApplications
                .SingleAsync(a => a.Id == story.Applications[0].Id, TestContext.Current.CancellationToken));
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        var letters = await _services.QueryAsync(context =>
            context.CoverLetters.ToListAsync(TestContext.Current.CancellationToken));
        Assert.Equal(kept.Id, Assert.Single(letters).Id);
    }
}
