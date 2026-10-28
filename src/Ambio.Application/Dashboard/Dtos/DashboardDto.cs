using Ambio.Application.JobApplications.Dtos;
using Ambio.Domain.JobApplications;

namespace Ambio.Application.Dashboard.Dtos;

/// <summary>The dashboard on <paramref name="Today"/>, over the active applications. <paramref name="CountsByStatus"/>
/// holds every status, those without application at 0. <paramref name="FollowUps"/> are the applications still Applied
/// <paramref name="FollowUpAfterDays"/> days or more after they were sent, oldest first; <paramref name="RecentActivity"/>
/// the latest events, newest first.</summary>
public record DashboardDto(
    DateOnly Today,
    IReadOnlyDictionary<ApplicationStatus, int> CountsByStatus,
    int FollowUpAfterDays,
    IReadOnlyList<JobApplicationSummaryDto> FollowUps,
    IReadOnlyList<ActivityDto> RecentActivity)
{
    /// <summary>The number of active applications.</summary>
    public int Total => CountsByStatus.Values.Sum();
}
