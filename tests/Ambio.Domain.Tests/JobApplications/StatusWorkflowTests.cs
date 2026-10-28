using Ambio.Domain.JobApplications;

namespace Ambio.Domain.Tests.JobApplications;

public class StatusWorkflowTests
{
    [Theory]
    [MemberData(nameof(AllowedTransitions))]
    public void Allows_TransitionOfTheWorkflow_ReturnsTrue(ApplicationStatus from, ApplicationStatus to)
    {
        Assert.True(StatusWorkflow.Allows(from, to));
    }

    [Theory]
    [MemberData(nameof(RefusedTransitions))]
    public void Allows_AnyOtherTransition_ReturnsFalse(ApplicationStatus from, ApplicationStatus to)
    {
        Assert.False(StatusWorkflow.Allows(from, to));
    }

    [Theory]
    [InlineData(ApplicationStatus.Rejected)]
    [InlineData(ApplicationStatus.Withdrawn)]
    public void Next_FinalStatus_ReturnsNone(ApplicationStatus status)
    {
        Assert.Empty(StatusWorkflow.Next(status));
    }

    [Fact]
    public void Next_Draft_ReturnsAppliedAndWithdrawn()
    {
        Assert.Equal([ApplicationStatus.Applied, ApplicationStatus.Withdrawn], StatusWorkflow.Next(ApplicationStatus.Draft));
    }

    private static readonly (ApplicationStatus From, ApplicationStatus To)[] Transitions =
    [
        (ApplicationStatus.Draft, ApplicationStatus.Applied),
        (ApplicationStatus.Draft, ApplicationStatus.Withdrawn),
        (ApplicationStatus.Applied, ApplicationStatus.Interview),
        (ApplicationStatus.Applied, ApplicationStatus.Offer),
        (ApplicationStatus.Applied, ApplicationStatus.Rejected),
        (ApplicationStatus.Applied, ApplicationStatus.Withdrawn),
        (ApplicationStatus.Interview, ApplicationStatus.Offer),
        (ApplicationStatus.Interview, ApplicationStatus.Rejected),
        (ApplicationStatus.Interview, ApplicationStatus.Withdrawn),
        (ApplicationStatus.Offer, ApplicationStatus.Withdrawn),
    ];

    public static TheoryData<ApplicationStatus, ApplicationStatus> AllowedTransitions => [.. Transitions];

    public static TheoryData<ApplicationStatus, ApplicationStatus> RefusedTransitions =>
    [
        .. from origin in Enum.GetValues<ApplicationStatus>()
           from target in Enum.GetValues<ApplicationStatus>()
           where !Transitions.Contains((origin, target))
           select (origin, target),
    ];
}
