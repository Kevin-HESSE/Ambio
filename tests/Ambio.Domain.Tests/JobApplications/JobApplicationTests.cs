using Ambio.Domain.JobApplications;

namespace Ambio.Domain.Tests.JobApplications;

/// <summary>The rules every kind of application shares, checked on an application to an offer.</summary>
public class JobApplicationTests
{
    private const string Comment = "Sent through the careers page";

    private static readonly DateTime ChangedAt = new(2026, 9, 3, 10, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime ArchivedAt = new(2026, 9, 27, 10, 0, 0, DateTimeKind.Utc);
    private static readonly DateOnly SentOn = new(2026, 9, 3);

    [Fact]
    public void ChangeStatus_AllowedTransition_MovesToTheStatusAndRecordsTheChange()
    {
        var application = SentApplication();

        application.ChangeStatus(ApplicationStatus.Interview, ChangedAt.AddDays(1), Comment);

        Assert.Equal(ApplicationStatus.Interview, application.Status);
        var change = application.StatusChanges[^1];
        Assert.Equal(
            (application.Id, ApplicationStatus.Applied, ApplicationStatus.Interview, ChangedAt.AddDays(1), Comment),
            (change.JobApplicationId, change.From, change.To, change.ChangedAt, change.Comment));
    }

    [Fact]
    public void ChangeStatus_DraftToApplied_RecordsTheSendingDay()
    {
        var application = NewApplication();

        application.ChangeStatus(ApplicationStatus.Applied, ChangedAt, appliedOn: SentOn);

        Assert.Equal((ApplicationStatus.Applied, SentOn), (application.Status, application.AppliedOn));
        Assert.Equal((ApplicationStatus.Draft, ApplicationStatus.Applied), (application.StatusChanges[0].From, application.StatusChanges[0].To));
    }

    [Fact]
    public void ChangeStatus_DraftToAppliedWithoutSendingDay_ThrowsArgumentException()
    {
        var application = NewApplication();

        Assert.Throws<ArgumentException>(() => application.ChangeStatus(ApplicationStatus.Applied, ChangedAt));
        Assert.Equal(ApplicationStatus.Draft, application.Status);
        Assert.Empty(application.StatusChanges);
    }

    [Fact]
    public void ChangeStatus_TransitionTheWorkflowRefuses_ThrowsInvalidOperationException()
    {
        var application = NewApplication();

        Assert.Throws<InvalidOperationException>(() => application.ChangeStatus(ApplicationStatus.Interview, ChangedAt));
        Assert.Equal(ApplicationStatus.Draft, application.Status);
        Assert.Empty(application.StatusChanges);
    }

    [Fact]
    public void ChangeStatus_DraftToWithdrawn_LeavesItWithoutSendingDay()
    {
        var application = NewApplication();

        application.ChangeStatus(ApplicationStatus.Withdrawn, ChangedAt, appliedOn: SentOn);

        Assert.Equal((ApplicationStatus.Withdrawn, (DateOnly?)null), (application.Status, application.AppliedOn));
    }

    [Fact]
    public void ChangeStatus_AfterSending_KeepsTheSendingDay()
    {
        var application = SentApplication();

        application.ChangeStatus(ApplicationStatus.Rejected, ChangedAt.AddDays(1), appliedOn: SentOn.AddDays(1));

        Assert.Equal(SentOn, application.AppliedOn);
        Assert.Equal(2, application.StatusChanges.Count);
    }

    [Fact]
    public void CanChangeStatusTo_FollowsTheWorkflowFromTheCurrentStatus()
    {
        var application = SentApplication();

        Assert.Equal((true, false), (application.CanChangeStatusTo(ApplicationStatus.Interview), application.CanChangeStatusTo(ApplicationStatus.Applied)));
    }

    [Fact]
    public void ChangeAppliedOn_WhenSent_ChangesTheDayButNotTheStatus()
    {
        var application = SentApplication();

        application.ChangeAppliedOn(SentOn.AddDays(-1));

        Assert.Equal((ApplicationStatus.Applied, SentOn.AddDays(-1)), (application.Status, application.AppliedOn));
    }

    [Fact]
    public void ChangeAppliedOn_OnDraft_ThrowsInvalidOperationException()
    {
        var application = NewApplication();

        Assert.Throws<InvalidOperationException>(() => application.ChangeAppliedOn(SentOn));
        Assert.Null(application.AppliedOn);
    }

    [Fact]
    public void Archive_Active_RecordsTheDay()
    {
        var application = NewApplication();

        application.Archive(ArchivedAt);

        Assert.Equal(ArchivedAt, application.ArchivedAt);
        Assert.True(application.IsArchived);
    }

    [Fact]
    public void Archive_AlreadyArchived_KeepsTheFirstDay()
    {
        var application = NewApplication();
        application.Archive(ArchivedAt);

        application.Archive(ArchivedAt.AddDays(1));

        Assert.Equal(ArchivedAt, application.ArchivedAt);
    }

    [Fact]
    public void Restore_Archived_ClearsTheDay()
    {
        var application = NewApplication();
        application.Archive(ArchivedAt);

        application.Restore();

        Assert.Null(application.ArchivedAt);
        Assert.False(application.IsArchived);
    }

    [Fact]
    public void CanChangeStatusTo_Archived_ReturnsFalse()
    {
        var application = SentApplication();
        application.Archive(ArchivedAt);

        Assert.False(application.CanChangeStatusTo(ApplicationStatus.Interview));
    }

    [Fact]
    public void ChangeStatus_Archived_ThrowsAndKeepsTheStatusAndTheHistory()
    {
        var application = SentApplication();
        application.Archive(ArchivedAt);

        Assert.Throws<InvalidOperationException>(() => application.ChangeStatus(ApplicationStatus.Interview, ChangedAt.AddDays(1)));
        Assert.Equal(ApplicationStatus.Applied, application.Status);
        Assert.Single(application.StatusChanges);
    }

    [Fact]
    public void ChangeStatus_Restored_MovesToTheStatus()
    {
        var application = SentApplication();
        application.Archive(ArchivedAt);
        application.Restore();

        application.ChangeStatus(ApplicationStatus.Interview, ChangedAt.AddDays(1));

        Assert.Equal(ApplicationStatus.Interview, application.Status);
    }

    private static OfferApplication NewApplication() => new()
    {
        ShortId = "482913",
        JobOfferId = Guid.CreateVersion7(),
        Channel = Channel.OnlineForm,
    };

    private static OfferApplication SentApplication()
    {
        var application = NewApplication();
        application.ChangeStatus(ApplicationStatus.Applied, ChangedAt, appliedOn: SentOn);
        return application;
    }
}
