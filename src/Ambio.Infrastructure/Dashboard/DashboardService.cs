using Ambio.Application.Dashboard;
using Ambio.Application.Dashboard.Dtos;
using Ambio.Application.JobApplications;
using Ambio.Domain.JobApplications;
using Ambio.Domain.Settings;
using Ambio.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace Ambio.Infrastructure.Dashboard;

internal class DashboardService : IDashboardService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
    private readonly TimeProvider _timeProvider;

    public DashboardService(IDbContextFactory<ApplicationDbContext> dbFactory, TimeProvider timeProvider)
    {
        _dbFactory = dbFactory;
        _timeProvider = timeProvider;
    }

    public async Task<DashboardDto> GetAsync(int activityCount, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);

        var today = DateOnly.FromDateTime(_timeProvider.GetUtcNow().UtcDateTime);
        // Until the settings are first saved, the defaults of the domain apply.
        var settings = await db.UserSettings.AsNoTracking().SingleOrDefaultAsync(cancellationToken) ?? new UserSettings();
        var active = db.JobApplications.AsNoTracking().Where(a => a.ArchivedAt == null);

        var counts = await active
            .GroupBy(a => a.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.Status, g => g.Count, cancellationToken);

        var lastSendingDay = settings.LastSendingDayToFollowUp(today);
        var followUps = await active
            .Where(a => a.Status == ApplicationStatus.Applied && a.AppliedOn <= lastSendingDay)
            .OrderBy(a => a.AppliedOn)
            .ThenBy(a => a.CreatedAt)
            .ToSummaryDto()
            .ToListAsync(cancellationToken);

        return new DashboardDto(
            Today: today,
            CountsByStatus: Enum.GetValues<ApplicationStatus>().ToDictionary(s => s, s => counts.GetValueOrDefault(s)),
            FollowUpAfterDays: settings.FollowUpAfterDays,
            FollowUps: followUps,
            RecentActivity: await ListActivityAsync(active, activityCount, cancellationToken));
    }

    /// <summary>The <paramref name="count"/> latest events of the applications, newest first: their status changes, and
    /// the creation of those created as drafts.</summary>
    private static async Task<IReadOnlyList<ActivityDto>> ListActivityAsync(
        IQueryable<JobApplication> applications, int count, CancellationToken cancellationToken)
    {
        var changes = await applications
            .SelectMany(a => a.StatusChanges, (a, c) => new { a.Id, c.To, c.ChangedAt })
            .OrderByDescending(e => e.ChangedAt)
            .Take(count)
            .Select(e => new ApplicationEvent(e.Id, e.To, e.ChangedAt))
            .ToListAsync(cancellationToken);

        // An application created as already sent records its sending before the save that sets its CreatedAt: its
        // creation would repeat its sending, so only the creations without such a change are events.
        var creations = await applications
            .Where(a => !a.StatusChanges.Any(c => c.ChangedAt <= a.CreatedAt))
            .OrderByDescending(a => a.CreatedAt)
            .Take(count)
            .Select(a => new ApplicationEvent(a.Id, ApplicationStatus.Draft, a.CreatedAt))
            .ToListAsync(cancellationToken);

        var events = changes.Concat(creations)
            .OrderByDescending(e => e.OccurredAt)
            .Take(count)
            .ToList();

        var ids = events.Select(e => e.ApplicationId).Distinct().ToList();
        var summaries = await applications
            .Where(a => ids.Contains(a.Id))
            .ToSummaryDto()
            .ToDictionaryAsync(a => a.Id, cancellationToken);

        return events
            .Select(e => new ActivityDto(
                Application: summaries[e.ApplicationId],
                Status: e.Status,
                OccurredAt: e.OccurredAt))
            .ToList();
    }

    private sealed record ApplicationEvent(Guid ApplicationId, ApplicationStatus Status, DateTime OccurredAt);
}
