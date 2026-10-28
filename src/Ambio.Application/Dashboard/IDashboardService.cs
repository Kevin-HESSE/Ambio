using Ambio.Application.Dashboard.Dtos;

namespace Ambio.Application.Dashboard;

public interface IDashboardService
{
    /// <summary>What the home page shows about the active applications (not archived): their count by status, those
    /// still Applied after the follow-up delay of the settings, oldest first, and the <paramref name="activityCount"/>
    /// latest events, newest first.</summary>
    public Task<DashboardDto> GetAsync(int activityCount, CancellationToken cancellationToken = default);
}
