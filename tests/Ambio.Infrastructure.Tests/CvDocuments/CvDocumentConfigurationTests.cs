using Ambio.Domain.CvDocuments;
using Ambio.Infrastructure.Tests.Fixtures;
using Ambio.Infrastructure.Tests.Stories;

using Microsoft.EntityFrameworkCore;

namespace Ambio.Infrastructure.Tests.CvDocuments;

public class CvDocumentConfigurationTests : IAsyncLifetime
{
    private SqliteServiceProvider _services = null!;

    public async ValueTask InitializeAsync() => _services = await SqliteServiceProvider.CreateAsync();

    public ValueTask DisposeAsync() => _services.DisposeAsync();

    [Fact]
    public async Task SaveChangesAsync_CvOfAnApplication_SavesEveryField()
    {
        var story = new OfferApplicationStory().WithApplications(1);
        await story.SaveAsync(_services);
        var document = CvDocumentFactory.GetGenerator(story.Applications[0].Id).Generate();

        await _services.AddAsync(document);

        var saved = await _services.QueryAsync(context =>
            context.CvDocuments.SingleAsync(TestContext.Current.CancellationToken));
        Assert.Equal(
            (document.Id, document.JobApplicationId, CvSource.Uploaded, document.FileName, document.Size, document.StoragePath),
            (saved.Id, saved.JobApplicationId, saved.Source, saved.FileName, saved.Size, saved.StoragePath));
    }

    [Fact]
    public async Task SaveChangesAsync_CvOfAnUnknownApplication_IsRefused()
    {
        var document = CvDocumentFactory.GetGenerator(Guid.CreateVersion7()).Generate();

        await Assert.ThrowsAsync<DbUpdateException>(() => _services.AddAsync(document));
    }

    [Fact]
    public async Task SaveChangesAsync_DeletedApplicationWithACv_IsRefused()
    {
        var story = new OfferApplicationStory().WithApplications(1);
        await story.SaveAsync(_services);
        await _services.AddAsync(CvDocumentFactory.GetGenerator(story.Applications[0].Id).Generate());

        await using var context = await _services.CreateDbContextAsync();
        context.JobApplications.Remove(await context.JobApplications
            .SingleAsync(TestContext.Current.CancellationToken));

        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync(TestContext.Current.CancellationToken));
    }
}
