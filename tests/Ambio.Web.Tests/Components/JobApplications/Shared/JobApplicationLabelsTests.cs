using Ambio.Domain.JobApplications;
using Ambio.Web.Components.JobApplications.Shared;

namespace Ambio.Web.Tests.Components.JobApplications.Shared;

public class JobApplicationLabelsTests
{
    [Fact]
    public void ToLabel_EveryStatus_ReturnsDistinctLabels()
    {
        var labels = Enum.GetValues<ApplicationStatus>().Select(status => status.ToLabel()).ToList();

        Assert.Equal(labels.Count, labels.Distinct().Count());
    }

    [Fact]
    public void ToIcon_EveryStatus_ReturnsDistinctIcons()
    {
        var icons = Enum.GetValues<ApplicationStatus>().Select(status => status.ToIcon()).ToList();

        Assert.Equal(icons.Count, icons.Distinct().Count());
    }

    [Fact]
    public void ToHint_EveryStatus_ReturnsDistinctHints()
    {
        var hints = Enum.GetValues<ApplicationStatus>().Select(status => status.ToHint()).ToList();

        Assert.Equal(hints.Count, hints.Distinct().Count());
    }

    [Fact]
    public void ToLabel_EveryChannel_ReturnsDistinctLabels()
    {
        var labels = Enum.GetValues<Channel>().Select(channel => channel.ToLabel()).ToList();

        Assert.Equal(labels.Count, labels.Distinct().Count());
    }

    [Fact]
    public void ToIcon_EveryChannel_ReturnsDistinctIcons()
    {
        var icons = Enum.GetValues<Channel>().Select(channel => channel.ToIcon()).ToList();

        Assert.Equal(icons.Count, icons.Distinct().Count());
    }

    [Fact]
    public void ToLabel_EveryKind_ReturnsDistinctLabels()
    {
        var labels = Enum.GetValues<ApplicationKind>().Select(kind => kind.ToLabel()).ToList();

        Assert.Equal(labels.Count, labels.Distinct().Count());
    }

    [Fact]
    public void ToIcon_EveryKind_ReturnsDistinctIcons()
    {
        var icons = Enum.GetValues<ApplicationKind>().Select(kind => kind.ToIcon()).ToList();

        Assert.Equal(icons.Count, icons.Distinct().Count());
    }
}
