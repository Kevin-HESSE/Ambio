namespace Ambio.Application.Common;

/// <summary>The files kept outside the database (ADR 0005), such as the CV PDFs. A path is relative to the storage
/// root and uses <c>/</c>, so the paths saved in the database don't depend on the system that wrote them.</summary>
public interface IDocumentStorage
{
    /// <summary>Writes <paramref name="content"/> to a new file at <paramref name="path"/>, creating its folders.
    /// An existing file is never replaced; a file left half-written by a failure is removed.</summary>
    /// <exception cref="IOException">The file exists or can't be written.</exception>
    public Task SaveAsync(string path, Stream content, CancellationToken cancellationToken = default);

    /// <summary>The content of the file at <paramref name="path"/>, or null when the file is missing (deleted by
    /// hand, for example).</summary>
    public Task<Stream?> OpenReadAsync(string path, CancellationToken cancellationToken = default);

    /// <summary>Deletes the file at <paramref name="path"/>; a missing file is left as it is.</summary>
    public Task DeleteAsync(string path, CancellationToken cancellationToken = default);
}
