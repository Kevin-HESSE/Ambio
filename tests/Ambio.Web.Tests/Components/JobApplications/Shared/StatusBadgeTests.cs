using Ambio.Domain.JobApplications;
using Ambio.Web.Components.JobApplications.Shared;

using Bunit;

namespace Ambio.Web.Tests.Components.JobApplications.Shared;

public class StatusBadgeTests : BunitContext
{
    [Theory]
    [MemberData(nameof(Statuses))]
    public void Render_WithStatus_ShowsItsIconLabelAndColorClass(ApplicationStatus status, string colorClass)
    {
        var cut = Render<StatusBadge>(parameters => parameters.Add(p => p.Status, status));

        var badge = cut.Find(".status-badge");
        Assert.Contains(colorClass, badge.ClassList);
        Assert.Equal(status.ToLabel(), badge.TextContent.Trim());
        Assert.Contains($"bi-{status.ToIcon()}", badge.QuerySelector("i")!.ClassList);
    }

    public static TheoryData<ApplicationStatus, string> Statuses => new()
    {
        { ApplicationStatus.Draft, "status-draft" },
        { ApplicationStatus.Applied, "status-applied" },
        { ApplicationStatus.Interview, "status-interview" },
        { ApplicationStatus.Offer, "status-offer" },
        { ApplicationStatus.Rejected, "status-rejected" },
        { ApplicationStatus.Withdrawn, "status-withdrawn" },
    };
}
