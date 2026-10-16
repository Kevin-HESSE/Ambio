using Ambio.Domain.Companies;

namespace Ambio.Domain.JobApplications;

/// <summary>An application sent to a company without any offer: its title is the position applied for.</summary>
public class SpontaneousApplication : JobApplication
{
    public required Guid CompanyId { get; init; }

    public Company Company { get; private set; } = null!;

    // The position applied for, such as "Backend Developer".
    public required string TargetPosition
    {
        get;
        set
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value);
            field = value.Trim();
        }
    }
}
