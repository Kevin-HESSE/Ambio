using Ambio.Application.Common;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Ambio.Infrastructure.Common;

/// <summary>Stores the documents as files under <see cref="StorageOptions.Root"/>.</summary>
internal sealed class FileSystemDocumentStorage : IDocumentStorage
{
    private readonly string _root;

    public FileSystemDocumentStorage(IOptions<StorageOptions> options, IHostEnvironment environment)
    {
        _root = options.Value.GetFullRoot(environment.ContentRootPath);
    }

    public async Task SaveAsync(string path, Stream content, CancellationToken cancellationToken = default)
    {
        var fullPath = FullPathOf(path);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

        // Opened apart from the copy: a file that already exists isn't ours to remove.
        var file = new FileStream(fullPath, new FileStreamOptions
        {
            Mode = FileMode.CreateNew,
            Access = FileAccess.Write,
            Options = FileOptions.Asynchronous,
        });

        try
        {
            await using (file)
            {
                await content.CopyToAsync(file, cancellationToken);
            }
        }
        catch
        {
            File.Delete(fullPath);
            throw;
        }
    }

    public Task<Stream?> OpenReadAsync(string path, CancellationToken cancellationToken = default)
    {
        var fullPath = FullPathOf(path);

        return Task.FromResult<Stream?>(File.Exists(fullPath)
            ? new FileStream(fullPath, new FileStreamOptions
            {
                Mode = FileMode.Open,
                Access = FileAccess.Read,
                Share = FileShare.Read,
                Options = FileOptions.Asynchronous,
            })
            : null);
    }

    public Task DeleteAsync(string path, CancellationToken cancellationToken = default)
    {
        var fullPath = FullPathOf(path);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }

    // The system path of a stored path, which must stay under the root: GetFullPath turns its '/' into the system's
    // separator and resolves any "..", so both full paths are compared once normalized.
    private string FullPathOf(string path)
    {
        var fullPath = Path.GetFullPath(path, _root);
        if (!fullPath.StartsWith(_root + Path.DirectorySeparatorChar, StringComparison.Ordinal))
        {
            throw new ArgumentException($"The path '{path}' is outside the storage root.", nameof(path));
        }

        return fullPath;
    }
}
