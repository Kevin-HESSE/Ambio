using Ambio.Domain.Common;

namespace Ambio.Domain.Companies;

public class Company : ITimeStampable
{
    public Guid Id { get; } = Guid.CreateVersion7();

    public required string Name
    {
        get;
        set
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value);
            field = value.Trim();
        }
    }

    public required string Slug { get; set; }

    public required CompanyKind Kind { get; set; }

    public string? Website { get; set; }

    public string? Industry { get; set; }

    public CompanySize? Size { get; set; }

    public string? Location { get; set; }

    public string? LinkedInUrl { get; set; }

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    // Set by Archive and cleared by Restore: an archived row is hidden from the lists (ADR 0012).
    public DateTime? ArchivedAt { get; private set; }

    public bool IsArchived => ArchivedAt is not null;

    /// <summary>Hides the company from the lists from <paramref name="archivedAt"/> (UTC). An archived company keeps
    /// the day it was first archived, so a cascade passing over it doesn't change it.</summary>
    public void Archive(DateTime archivedAt) => ArchivedAt ??= archivedAt;

    /// <summary>Shows the company in the lists again.</summary>
    public void Restore() => ArchivedAt = null;
}
