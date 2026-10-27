using System.Text;

using Ambio.Application.Common;
using Ambio.Application.CvDocuments;
using Ambio.Application.CvDocuments.Dtos;
using Ambio.Domain.CvDocuments;
using Ambio.Infrastructure.Common;
using Ambio.Infrastructure.Tests.Fixtures;
using Ambio.Infrastructure.Tests.Stories;

using Microsoft.EntityFrameworkCore;

using NSubstitute;

namespace Ambio.Infrastructure.Tests.CvDocuments;

public class CvDocumentServiceTests : IAsyncLifetime
{
    private const string FileName = "CV_Backend_2026-09.pdf";
    private const int MaxUploadSizeInMegabytes = 1;
    private const long MaxUploadSize = MaxUploadSizeInMegabytes * 1024 * 1024;

    private static readonly DateTimeOffset Now = new(2026, 9, 28, 10, 0, 0, TimeSpan.Zero);
    private static readonly byte[] Pdf = Encoding.ASCII.GetBytes("%PDF-1.7\n1 0 obj << >> endobj\n%%EOF");

    private SqliteServiceProvider _services = null!;

    public async ValueTask InitializeAsync()
    {
        _services = await SqliteServiceProvider.CreateAsync(new Dictionary<string, string?>
        {
            [$"{StorageOptions.Section}:MaxUploadSizeInMegabytes"] = $"{MaxUploadSizeInMegabytes}",
        });
        _services.TimeProvider.GetUtcNow().Returns(Now);
    }

    public ValueTask DisposeAsync() => _services.DisposeAsync();

    [Fact]
    public async Task MaxUploadSize_IsTheSettingInBytes()
    {
        var maxUploadSize = await _services.CallAsync((ICvDocumentService service) => Task.FromResult(service.MaxUploadSize));

        Assert.Equal(MaxUploadSize, maxUploadSize);
    }

    [Fact]
    public async Task ListAsync_WithoutCvs_ReturnsEmpty()
    {
        var story = await SaveApplicationsAsync(1);

        var documents = await ListAsync(story.Applications[0].Id);

        Assert.Empty(documents);
    }

    [Fact]
    public async Task ListAsync_WithCvs_ReturnsTheCvsOfTheApplicationNewestFirst()
    {
        var story = await SaveApplicationsAsync(2);
        var applicationId = story.Applications[0].Id;
        var oldest = CvDocumentFactory.GetGenerator(applicationId).Generate();
        var newest = CvDocumentFactory.GetGenerator(applicationId).Generate();
        await AddAtAsync(Now.AddDays(-1), oldest);
        await AddAtAsync(Now, newest);
        await AddAtAsync(Now, CvDocumentFactory.GetGenerator(story.Applications[1].Id).Generate());

        var documents = await ListAsync(applicationId);

        Assert.Equal(
            [
                new CvDocumentDto(newest.Id, newest.FileName, newest.Size, Now.UtcDateTime),
                new CvDocumentDto(oldest.Id, oldest.FileName, oldest.Size, Now.AddDays(-1).UtcDateTime),
            ],
            documents);
    }

    [Fact]
    public async Task UploadAsync_Pdf_StoresTheFileAndTheTrimmedCvAndReturnsItsId()
    {
        var story = await SaveApplicationsAsync(1);

        var result = await UploadAsync(story.Applications[0].Id, Pdf, fileName: $" {FileName} ");

        Assert.True(result.TryGetValue(out var id));
        var saved = await _services.QueryAsync(context =>
            context.CvDocuments.SingleAsync(d => d.Id == id, TestContext.Current.CancellationToken));
        Assert.Equal(
            (story.Applications[0].Id, CvSource.Uploaded, FileName, (long)Pdf.Length, $"documents/2026/{id}.pdf", Now.UtcDateTime),
            (saved.JobApplicationId, saved.Source, saved.FileName, saved.Size, saved.StoragePath, saved.CreatedAt));
        Assert.Equal(Pdf, await ReadStoredFileAsync(saved.StoragePath));
    }

    [Fact]
    public async Task UploadAsync_ArchivedApplication_StoresTheCv()
    {
        var story = new OfferApplicationStory().WithApplications(1, OfferApplicationFactory.IsArchivedRuleSet);
        await story.SaveAsync(_services);

        var result = await UploadAsync(story.Applications[0].Id, Pdf);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task UploadAsync_PdfOfTheLimit_StoresTheCv()
    {
        var story = await SaveApplicationsAsync(1);

        var result = await UploadAsync(story.Applications[0].Id, PdfOfSize(MaxUploadSize));

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task UploadAsync_UnknownApplication_FailsAndStoresNothing()
    {
        var result = await UploadAsync(Guid.CreateVersion7(), Pdf);

        Assert.Equal(CvDocumentErrors.ApplicationNotFound, result.GetMessage);
        await AssertNothingStoredAsync();
    }

    [Theory]
    [MemberData(nameof(NotPdfContents))]
    public async Task UploadAsync_NotAPdf_FailsAndStoresNothing(string content)
    {
        var story = await SaveApplicationsAsync(1);

        var result = await UploadAsync(story.Applications[0].Id, Encoding.ASCII.GetBytes(content));

        Assert.Equal(CvDocumentErrors.NotPdf, result.GetMessage);
        await AssertNothingStoredAsync();
    }

    [Fact]
    public async Task UploadAsync_PdfOverTheLimit_FailsAndStoresNothing()
    {
        var story = await SaveApplicationsAsync(1);

        var result = await UploadAsync(story.Applications[0].Id, PdfOfSize(MaxUploadSize + 1));

        Assert.Equal(CvDocumentErrors.TooLarge, result.GetMessage);
        await AssertNothingStoredAsync();
    }

    private async Task<OfferApplicationStory> SaveApplicationsAsync(int count)
    {
        var story = new OfferApplicationStory().WithApplications(count);
        await story.SaveAsync(_services);
        return story;
    }

    private async Task AddAtAsync(DateTimeOffset createdAt, CvDocument document)
    {
        _services.TimeProvider.GetUtcNow().Returns(createdAt);
        await _services.AddAsync(document);
    }

    private Task<IReadOnlyList<CvDocumentDto>> ListAsync(Guid applicationId) =>
        _services.CallAsync((ICvDocumentService service) =>
            service.ListAsync(applicationId, TestContext.Current.CancellationToken));

    private async Task<ServiceResult<Guid>> UploadAsync(Guid applicationId, byte[] content, string fileName = FileName)
    {
        using var stream = new MemoryStream(content);
        return await _services.CallAsync((ICvDocumentService service) =>
            service.UploadAsync(applicationId, fileName, stream, TestContext.Current.CancellationToken));
    }

    private async Task<byte[]> ReadStoredFileAsync(string storagePath)
    {
        await using var stream = await _services.CallAsync((IDocumentStorage storage) =>
            storage.OpenReadAsync(storagePath, TestContext.Current.CancellationToken));
        using var copy = new MemoryStream();
        await stream!.CopyToAsync(copy, TestContext.Current.CancellationToken);
        return copy.ToArray();
    }

    private async Task AssertNothingStoredAsync()
    {
        Assert.False(await _services.QueryAsync(context =>
            context.CvDocuments.AnyAsync(TestContext.Current.CancellationToken)));
        Assert.False(Directory.Exists(Path.Combine(_services.ContentRootPath, "Data")));
    }

    // A PDF header followed by padding, up to the size.
    private static byte[] PdfOfSize(long size)
    {
        var content = new byte[size];
        Pdf.CopyTo(content, 0);
        return content;
    }

    public static TheoryData<string> NotPdfContents =>
    [
        "",
        "%PDF",
        "Dear Brightwave team, here is my CV.",
    ];
}
