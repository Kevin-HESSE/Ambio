using System.Globalization;

using Ambio.Domain.Common;

namespace Ambio.Domain.CvDocuments;

/// <summary>The metadata of a CV file sent with an application (ADR 0007). The file itself is stored outside the
/// database, at <see cref="StoragePath"/> (ADR 0005). An application has any number of them, none at first.</summary>
public class CvDocument : ITimeStampable
{
    private CvDocument(Guid jobApplicationId, CvSource source, string fileName, long size, DateTime createdOn)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(size);

        JobApplicationId = jobApplicationId;
        Source = source;
        FileName = fileName.Trim();
        Size = size;
        StoragePath = string.Create(CultureInfo.InvariantCulture, $"documents/{createdOn:yyyy}/{Id}.pdf");
    }

    // For EF Core, which sets the properties from the row.
    private CvDocument()
    {
    }

    public Guid Id { get; } = Guid.CreateVersion7();

    public Guid JobApplicationId { get; private init; }

    public CvSource Source { get; private init; }

    /// <summary>The name of the file as uploaded, shown in the list and given back on download.</summary>
    public string FileName { get; private init; } = "";

    /// <summary>The size of the file, in bytes.</summary>
    public long Size { get; private init; }

    /// <summary>The path of the file, relative to the storage root: <c>documents/&lt;yyyy&gt;/&lt;id&gt;.pdf</c>.
    /// It is named after the id, so the file can be written before the row is saved.</summary>
    public string StoragePath { get; private init; } = "";

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    /// <summary>The PDF sent with the application, uploaded on <paramref name="uploadedAt"/>, whose year sorts its
    /// file into a folder. The name is trimmed; a blank name or a size that isn't positive is refused.</summary>
    public static CvDocument Upload(Guid jobApplicationId, string fileName, long size, DateTime uploadedAt) =>
        new(jobApplicationId, CvSource.Uploaded, fileName, size, uploadedAt);
}
