namespace Ambio.Domain.JobApplications;

/// <summary>The transitions of the status workflow (docs/data-model.md#status-workflow): an application only moves
/// forward. <see cref="ApplicationStatus.Applied"/> is only reached from a draft, which is then sent.</summary>
public static class StatusWorkflow
{
    private static readonly Dictionary<ApplicationStatus, ApplicationStatus[]> Transitions = new()
    {
        [ApplicationStatus.Draft] = [ApplicationStatus.Applied, ApplicationStatus.Withdrawn],
        [ApplicationStatus.Applied] = [ApplicationStatus.Interview, ApplicationStatus.Offer, ApplicationStatus.Rejected, ApplicationStatus.Withdrawn],
        [ApplicationStatus.Interview] = [ApplicationStatus.Offer, ApplicationStatus.Rejected, ApplicationStatus.Withdrawn],
        // A job offer can still be declined.
        [ApplicationStatus.Offer] = [ApplicationStatus.Withdrawn],
        [ApplicationStatus.Rejected] = [],
        [ApplicationStatus.Withdrawn] = [],
    };

    /// <summary>The statuses an application can move to from <paramref name="status"/>: none for a final one.</summary>
    public static IReadOnlyList<ApplicationStatus> Next(ApplicationStatus status) => Transitions[status];

    /// <summary>Whether an application can move from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public static bool Allows(ApplicationStatus from, ApplicationStatus to) => Transitions[from].Contains(to);
}
