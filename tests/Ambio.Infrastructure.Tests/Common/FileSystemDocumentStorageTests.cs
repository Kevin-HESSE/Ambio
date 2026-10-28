using System.Text;

using Ambio.Application.Common;
using Ambio.Infrastructure.Common;
using Ambio.Infrastructure.Tests.Fixtures;

using Microsoft.Extensions.Options;

namespace Ambio.Infrastructure.Tests.Common;

public class FileSystemDocumentStorageTests : IAsyncLifetime
{
    private const string StoragePath = "documents/2026/cv.pdf";
    private const string DefaultRoot = "Data";

    private static readonly byte[] Content = Encoding.ASCII.GetBytes("%PDF-1.7 a CV");
    private static readonly byte[] OtherContent = Encoding.ASCII.GetBytes("%PDF-1.7 another CV");

    private SqliteServiceProvider _services = null!;

    public async ValueTask InitializeAsync() => _services = await SqliteServiceProvider.CreateAsync();

    public ValueTask DisposeAsync() => _services.DisposeAsync();

    [Fact]
    public async Task OpenReadAsync_SavedFile_GivesItsContentBack()
    {
        await SaveAsync(_services, StoragePath, Content);

        var stream = await _services.CallAsync((IDocumentStorage storage) =>
            storage.OpenReadAsync(StoragePath, TestContext.Current.CancellationToken));

        Assert.Equal(Content, await ReadAsync(Assert.IsAssignableFrom<Stream>(stream)));
    }

    [Fact]
    public async Task SaveAsync_RelativeRoot_WritesUnderTheContentRootAndCreatesTheFolders()
    {
        await SaveAsync(_services, StoragePath, Content);

        Assert.Equal(Content, await File.ReadAllBytesAsync(
            Path.Combine(_services.ContentRootPath, DefaultRoot, "documents", "2026", "cv.pdf"),
            TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task SaveAsync_AbsoluteRoot_WritesUnderIt()
    {
        var root = Path.Combine(Path.GetTempPath(), $"ambio-tests-{Guid.NewGuid()}");
        try
        {
            await using var services = await SqliteServiceProvider.CreateAsync(
                new Dictionary<string, string?> { [$"{StorageOptions.Section}:Root"] = root });

            await SaveAsync(services, StoragePath, Content);

            Assert.True(File.Exists(Path.Combine(root, "documents", "2026", "cv.pdf")));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task SaveAsync_ExistingFile_FailsAndKeepsIt()
    {
        await SaveAsync(_services, StoragePath, Content);

        await Assert.ThrowsAsync<IOException>(() => SaveAsync(_services, StoragePath, OtherContent));

        var stream = await _services.CallAsync((IDocumentStorage storage) =>
            storage.OpenReadAsync(StoragePath, TestContext.Current.CancellationToken));
        Assert.Equal(Content, await ReadAsync(stream!));
    }

    [Fact]
    public async Task SaveAsync_CopyFails_RemovesTheHalfWrittenFile()
    {
        await Assert.ThrowsAsync<IOException>(() => SaveAsync(_services, StoragePath, new FailingStream()));

        var stream = await _services.CallAsync((IDocumentStorage storage) =>
            storage.OpenReadAsync(StoragePath, TestContext.Current.CancellationToken));
        Assert.Null(stream);
    }

    [Theory]
    [MemberData(nameof(PathsOutsideTheRoot))]
    public async Task SaveAsync_PathOutsideTheRoot_IsRefused(string path)
    {
        await Assert.ThrowsAsync<ArgumentException>(() => SaveAsync(_services, path, Content));
    }

    [Fact]
    public async Task OpenReadAsync_MissingFile_ReturnsNull()
    {
        var stream = await _services.CallAsync((IDocumentStorage storage) =>
            storage.OpenReadAsync(StoragePath, TestContext.Current.CancellationToken));

        Assert.Null(stream);
    }

    [Fact]
    public async Task DeleteAsync_SavedFile_RemovesIt()
    {
        await SaveAsync(_services, StoragePath, Content);

        await _services.CallAsync((IDocumentStorage storage) =>
            storage.DeleteAsync(StoragePath, TestContext.Current.CancellationToken));

        var stream = await _services.CallAsync((IDocumentStorage storage) =>
            storage.OpenReadAsync(StoragePath, TestContext.Current.CancellationToken));
        Assert.Null(stream);
    }

    [Fact]
    public async Task DeleteAsync_MissingFile_DoesNothing()
    {
        var exception = await Record.ExceptionAsync(() => _services.CallAsync((IDocumentStorage storage) =>
            storage.DeleteAsync(StoragePath, TestContext.Current.CancellationToken)));

        Assert.Null(exception);
    }

    [Theory]
    [InlineData("Root", " ")]
    [InlineData("MaxUploadSizeInMegabytes", "0")]
    public async Task Options_Invalid_FailValidation(string key, string value)
    {
        await using var services = await SqliteServiceProvider.CreateAsync(
            new Dictionary<string, string?> { [$"{StorageOptions.Section}:{key}"] = value });

        await Assert.ThrowsAsync<OptionsValidationException>(() =>
            services.CallAsync((IDocumentStorage storage) => storage.DeleteAsync(StoragePath, TestContext.Current.CancellationToken)));
    }

    private static async Task SaveAsync(SqliteServiceProvider services, string path, byte[] content)
    {
        using var stream = new MemoryStream(content);
        await SaveAsync(services, path, stream);
    }

    private static Task SaveAsync(SqliteServiceProvider services, string path, Stream content) =>
        services.CallAsync((IDocumentStorage storage) => storage.SaveAsync(path, content, TestContext.Current.CancellationToken));

    private static async Task<byte[]> ReadAsync(Stream stream)
    {
        await using (stream)
        {
            using var copy = new MemoryStream();
            await stream.CopyToAsync(copy, TestContext.Current.CancellationToken);
            return copy.ToArray();
        }
    }

    // A stream that breaks while it is read, as an upload cut off in the middle.
    private sealed class FailingStream : MemoryStream
    {
        public FailingStream() : base(Content)
        {
        }

        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) =>
            Position == 0 ? base.ReadAsync(buffer, cancellationToken) : throw new IOException("The upload was cut off.");

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) =>
            ReadAsync(buffer.AsMemory(offset, count), cancellationToken).AsTask();
    }

    public static TheoryData<string> PathsOutsideTheRoot =>
    [
        "../outside.pdf",
        "documents/../../outside.pdf",
        Path.Combine(Path.GetTempPath(), "outside.pdf"),
    ];
}
