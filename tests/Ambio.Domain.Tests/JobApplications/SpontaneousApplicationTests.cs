using Ambio.Domain.JobApplications;

namespace Ambio.Domain.Tests.JobApplications;

public class SpontaneousApplicationTests
{
    private const string ShortId = "662140";
    private const string TargetPosition = "Backend Developer";

    private static readonly DateOnly SentOn = new(2026, 9, 3);

    [Fact]
    public void New_GeneratesVersion7Id()
    {
        Assert.Equal(7, NewApplication().Id.Version);
    }

    [Fact]
    public void New_IsDraftWithoutSendingDate()
    {
        var application = NewApplication();

        Assert.Equal((ApplicationStatus.Draft, (DateOnly?)null), (application.Status, application.AppliedOn));
    }

    [Theory]
    [MemberData(nameof(BlankValues))]
    public void TargetPosition_WhenBlank_ThrowsArgumentException(string targetPosition)
    {
        Assert.Throws<ArgumentException>(() => NewApplication(targetPosition));
    }

    [Fact]
    public void TargetPosition_IsTrimmed()
    {
        Assert.Equal(TargetPosition, NewApplication($"  {TargetPosition}\n").TargetPosition);
    }

    [Fact]
    public void MarkAsSent_OnDraft_MovesToAppliedWithTheDate()
    {
        var application = NewApplication();

        application.MarkAsSent(SentOn);

        Assert.Equal((ApplicationStatus.Applied, SentOn), (application.Status, application.AppliedOn));
    }

    private static SpontaneousApplication NewApplication(string targetPosition = TargetPosition) => new()
    {
        ShortId = ShortId,
        CompanyId = Guid.CreateVersion7(),
        TargetPosition = targetPosition,
        Channel = Channel.Email,
    };

    public static TheoryData<string> BlankValues =>
    [
        "",
        " ",
        "\t\n",
    ];
}
