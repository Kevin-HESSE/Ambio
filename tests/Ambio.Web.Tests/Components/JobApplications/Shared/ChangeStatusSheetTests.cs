using Ambio.Application.Common;
using Ambio.Application.JobApplications;
using Ambio.Application.JobApplications.Dtos;
using Ambio.Domain.JobApplications;
using Ambio.Web.Components.JobApplications.Shared;

using Bunit;
using Bunit.TestDoubles;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

using NSubstitute;

namespace Ambio.Web.Tests.Components.JobApplications.Shared;

public class ChangeStatusSheetTests : BunitContext
{
    private const string CloseSheet = "ambio.sheet.close";
    private const string Comment = "Sent by email to the recruiter";
    private const string SentOn = "2026-09-03";

    private static readonly Guid ApplicationId = Guid.CreateVersion7();

    private readonly IJobApplicationService _jobApplicationService = Substitute.For<IJobApplicationService>();

    public ChangeStatusSheetTests()
    {
        Services.AddSingleton(_jobApplicationService);
        SetRendererInfo(new RendererInfo("Server", isInteractive: true));
        JSInterop.SetupVoid(CloseSheet, _ => true).SetVoidResult();
    }

    [Fact]
    public void Render_BeforeTheCircuit_DisablesTheButton()
    {
        SetRendererInfo(new RendererInfo("Static", isInteractive: false));

        var cut = RenderSheet(ApplicationStatus.Draft);

        Assert.True(cut.Find(".change-status-open").HasAttribute("disabled"));
    }

    [Fact]
    public void Render_Interactive_OpensTheSheetWithOnlyTheNextStatuses()
    {
        var cut = RenderSheet(ApplicationStatus.Applied);

        var open = cut.Find(".change-status-open");
        Assert.False(open.HasAttribute("disabled"));
        Assert.Equal(ChangeStatusSheet.SheetId, open.GetAttribute("data-sheet-open"));
        Assert.Equal(ApplicationStatus.Applied.ToLabel(), cut.Find(".change-status-current .status-badge").TextContent.Trim());
        Assert.Equal(
            StatusWorkflow.Next(ApplicationStatus.Applied).Select(status => (status.ToLabel(), status.ToHint())),
            cut.FindAll(".change-status-option").Select(option => (
                option.QuerySelector(".status-badge")!.TextContent.Trim(),
                option.QuerySelector(".change-status-hint")!.TextContent)));
    }

    [Fact]
    public void Choose_Applied_AsksForTheSendingDayOnlyThen()
    {
        var cut = RenderSheet(ApplicationStatus.Draft);

        Choose(cut, ApplicationStatus.Applied);
        Assert.NotNull(cut.Find("#status-sent-on"));

        Choose(cut, ApplicationStatus.Withdrawn);
        Assert.Empty(cut.FindAll("#status-sent-on"));
    }

    [Fact]
    public void Submit_WithoutStatus_ShowsTheErrorWithoutChanging()
    {
        var cut = RenderSheet(ApplicationStatus.Draft);

        cut.Find($"#{ChangeStatusSheet.SheetId} form").Submit();

        Assert.Equal("Choose the new status.", cut.Find(".validation-message").TextContent);
        AssertNotChanged();
    }

    [Fact]
    public void Submit_AppliedWithoutSendingDay_ShowsTheErrorWithoutChanging()
    {
        var cut = RenderSheet(ApplicationStatus.Draft);

        Choose(cut, ApplicationStatus.Applied);
        cut.Find($"#{ChangeStatusSheet.SheetId} form").Submit();

        Assert.Equal(JobApplicationInput.SentOnRequired, cut.Find("#status-sent-on ~ .validation-message").TextContent);
        AssertNotChanged();
    }

    [Fact]
    public void Submit_WithStatusDayAndComment_ChangesTheStatusClosesTheSheetAndRefreshesThePage()
    {
        _jobApplicationService.ChangeStatusAsync(ApplicationId, Arg.Any<StatusChangeInput>(), Arg.Any<CancellationToken>())
            .Returns(ServiceResult.Success());
        var navigation = Services.GetRequiredService<BunitNavigationManager>();
        var cut = RenderSheet(ApplicationStatus.Draft);

        Choose(cut, ApplicationStatus.Applied);
        cut.Find("#status-sent-on").Change(SentOn);
        cut.Find("#status-comment").Change(Comment);
        cut.Find($"#{ChangeStatusSheet.SheetId} form").Submit();

        _jobApplicationService.Received(1).ChangeStatusAsync(
            ApplicationId,
            Arg.Is<StatusChangeInput>(input =>
                input.To == ApplicationStatus.Applied && input.SentOn == DateOnly.Parse(SentOn) && input.Comment == Comment),
            Arg.Any<CancellationToken>());
        Assert.Equal(ChangeStatusSheet.SheetId, JSInterop.VerifyInvoke(CloseSheet).Arguments[0]);
        Assert.Equal(navigation.Uri, Assert.Single(navigation.History).Uri);
        Assert.Equal("", cut.Find("#status-comment").TextContent);
    }

    [Fact]
    public void Submit_StatusThatCantFollowAnymore_ShowsTheMessageAndKeepsTheSheetOpen()
    {
        _jobApplicationService.ChangeStatusAsync(ApplicationId, Arg.Any<StatusChangeInput>(), Arg.Any<CancellationToken>())
            .Returns(ServiceResult.Failure(JobApplicationErrors.StatusNotAllowed));
        var cut = RenderSheet(ApplicationStatus.Draft);

        Choose(cut, ApplicationStatus.Withdrawn);
        cut.Find($"#{ChangeStatusSheet.SheetId} form").Submit();

        Assert.Equal(JobApplicationErrors.StatusNotAllowed, cut.Find(".validation-message").TextContent);
        JSInterop.VerifyNotInvoke(CloseSheet);
    }

    [Fact]
    public void Submit_UnknownApplication_RequestsNotFound()
    {
        _jobApplicationService.ChangeStatusAsync(ApplicationId, Arg.Any<StatusChangeInput>(), Arg.Any<CancellationToken>())
            .Returns(ServiceResult.Failure(JobApplicationErrors.NotFound));
        var notFound = false;
        Services.GetRequiredService<NavigationManager>().OnNotFound += (_, _) => notFound = true;
        var cut = RenderSheet(ApplicationStatus.Draft);

        Choose(cut, ApplicationStatus.Withdrawn);
        cut.Find($"#{ChangeStatusSheet.SheetId} form").Submit();

        Assert.True(notFound);
    }

    private void AssertNotChanged()
    {
        _jobApplicationService.DidNotReceiveWithAnyArgs().ChangeStatusAsync(default, default!, default);
        JSInterop.VerifyNotInvoke(CloseSheet);
    }

    private static void Choose(IRenderedComponent<ChangeStatusSheet> cut, ApplicationStatus status) =>
        cut.Find($".change-status-option input[value='{status}']").Change(status.ToString());

    private IRenderedComponent<ChangeStatusSheet> RenderSheet(ApplicationStatus status) =>
        Render<ChangeStatusSheet>(parameters => parameters
            .Add(sheet => sheet.ApplicationId, ApplicationId)
            .Add(sheet => sheet.Status, status)
            .Add(sheet => sheet.NextStatuses, StatusWorkflow.Next(status)));
}
