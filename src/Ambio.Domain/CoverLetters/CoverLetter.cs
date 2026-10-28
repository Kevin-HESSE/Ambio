using Ambio.Domain.Common;

namespace Ambio.Domain.CoverLetters;

/// <summary>A cover letter of an application, in plain text. An application has any number of them, none at first;
/// they go with their application. It was last edited on its <see cref="UpdatedAt"/>.</summary>
public class CoverLetter : ITimeStampable
{
    public Guid Id { get; } = Guid.CreateVersion7();

    public required Guid JobApplicationId { get; init; }

    public required string Title
    {
        get;
        set
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value);
            field = value.Trim();
        }
    }

    // Plain text: trimmed at both ends, its line breaks kept.
    public required string Content
    {
        get;
        set
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value);
            field = value.Trim();
        }
    }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
