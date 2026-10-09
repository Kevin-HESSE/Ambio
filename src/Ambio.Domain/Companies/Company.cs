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

    public required CompanyKind Kind { get; set; }

    public string? Website { get; set; }

    public string? Industry { get; set; }

    public CompanySize? Size { get; set; }

    public string? Location { get; set; }

    public string? LinkedInUrl { get; set; }

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? ArchivedAt { get; set; }
}
