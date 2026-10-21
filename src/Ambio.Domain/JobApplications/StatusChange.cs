namespace Ambio.Domain.JobApplications;

/// <summary>A change of the status of an application, recorded by <see cref="JobApplication.ChangeStatus"/>. The
/// history is append-only: a change is never updated, so <see cref="ChangedAt"/> is its only timestamp.</summary>
public class StatusChange
{
    internal StatusChange()
    {
    }

    public Guid Id { get; } = Guid.CreateVersion7();

    public required Guid JobApplicationId { get; init; }

    public required ApplicationStatus From { get; init; }

    public required ApplicationStatus To { get; init; }

    // UTC: when the change was recorded.
    public required DateTime ChangedAt { get; init; }

    public string? Comment { get; init; }
}
