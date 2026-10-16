using Ambio.Domain.JobApplications;

namespace Ambio.Web.Components.JobApplications.Shared;

/// <summary>Display names and icons of the application enums (docs/design-system.md#application-statuses). The
/// database stores the enum names, never these labels.</summary>
public static class JobApplicationLabels
{
    public static string ToLabel(this ApplicationStatus status) => status switch
    {
        ApplicationStatus.Draft => "Draft",
        ApplicationStatus.Applied => "Applied",
        ApplicationStatus.Interview => "Interview",
        ApplicationStatus.Offer => "Offer received",
        ApplicationStatus.Rejected => "Rejected",
        ApplicationStatus.Withdrawn => "Withdrawn",
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, null),
    };

    /// <summary>Bootstrap Icons name, without the <c>bi-</c> prefix.</summary>
    public static string ToIcon(this ApplicationStatus status) => status switch
    {
        ApplicationStatus.Draft => "pencil",
        ApplicationStatus.Applied => "send",
        ApplicationStatus.Interview => "people",
        ApplicationStatus.Offer => "trophy",
        ApplicationStatus.Rejected => "x-circle",
        ApplicationStatus.Withdrawn => "slash-circle",
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, null),
    };

    public static string ToLabel(this Channel channel) => channel switch
    {
        Channel.OnlineForm => "Online form",
        Channel.Email => "Email",
        Channel.Referral => "Referral",
        Channel.InPerson => "In person",
        Channel.Other => "Other",
        _ => throw new ArgumentOutOfRangeException(nameof(channel), channel, null),
    };

    /// <summary>Bootstrap Icons name, without the <c>bi-</c> prefix.</summary>
    public static string ToIcon(this Channel channel) => channel switch
    {
        Channel.OnlineForm => "globe",
        Channel.Email => "envelope",
        Channel.Referral => "link-45deg",
        Channel.InPerson => "geo-alt",
        Channel.Other => "three-dots",
        _ => throw new ArgumentOutOfRangeException(nameof(channel), channel, null),
    };
}
