using Ambio.Application.Common;
using Ambio.Application.JobApplications.Dtos;
using Ambio.Domain.JobApplications;
using Ambio.Domain.Offers;

namespace Ambio.Application.JobApplications;

public static class JobApplicationMappingExtensions
{
    /// <summary>Projection translated to SQL by EF Core, for every kind: the offer, its company and the posting used,
    /// or the company of a spontaneous application.</summary>
    /// <remarks>An expression tree accepts neither a switch expression nor a pattern that declares a variable
    /// (<c>is OfferApplication o</c>), and EF Core can't translate a call to a method: each member tests the kind and
    /// casts.</remarks>
    public static IQueryable<JobApplicationSummaryDto> ToSummaryDto(this IQueryable<JobApplication> applications) =>
        applications.Select(a => new JobApplicationSummaryDto(
            Id: a.Id,
            ShortId: a.ShortId,
            Kind: a is SpontaneousApplication ? ApplicationKind.Spontaneous : ApplicationKind.Offer,
            Title: a is OfferApplication
                ? ((OfferApplication)a).JobOffer.Title
                : ((SpontaneousApplication)a).TargetPosition,
            CompanyName: a is OfferApplication
                ? ((OfferApplication)a).JobOffer.Company.Name
                : ((SpontaneousApplication)a).Company.Name,
            Status: a.Status,
            Channel: a.Channel,
            AppliedVia: a is OfferApplication && ((OfferApplication)a).AppliedViaPosting != null
                ? ((OfferApplication)a).AppliedViaPosting!.Site
                : null,
            SentOn: a.AppliedOn));

    /// <summary>Projection translated to SQL by EF Core, with the offer, its company and the posting used.</summary>
    public static IQueryable<OfferApplicationDetailsDto> ToDetailsDto(this IQueryable<OfferApplication> applications) =>
        applications.Select(a => new OfferApplicationDetailsDto(
            Id: a.Id,
            ShortId: a.ShortId,
            Title: a.JobOffer.Title,
            CompanyName: a.JobOffer.Company.Name,
            CompanySlug: a.JobOffer.Company.Slug,
            Status: a.Status,
            Channel: a.Channel,
            ChannelDetail: a.ChannelDetail,
            SentOn: a.AppliedOn,
            Notes: a.Notes,
            CreatedAt: a.CreatedAt,
            StatusHistory: a.StatusChanges
                .OrderByDescending(c => c.ChangedAt)
                .Select(c => new StatusChangeDto(
                    From: c.From,
                    To: c.To,
                    ChangedAt: c.ChangedAt,
                    Comment: c.Comment))
                .ToList(),
            JobOfferId: a.JobOfferId,
            JobOfferShortId: a.JobOffer.ShortId,
            Location: a.JobOffer.Location,
            ContractType: a.JobOffer.ContractType,
            RemotePolicy: a.JobOffer.RemotePolicy,
            Salary: a.JobOffer.Salary,
            AppliedViaPostingId: a.AppliedViaPostingId,
            AppliedViaSite: a.AppliedViaPosting != null ? a.AppliedViaPosting.Site : null,
            AppliedViaUrl: a.AppliedViaPosting != null ? a.AppliedViaPosting.Url : null));

    /// <summary>Projection translated to SQL by EF Core, with the company.</summary>
    public static IQueryable<SpontaneousApplicationDetailsDto> ToDetailsDto(this IQueryable<SpontaneousApplication> applications) =>
        applications.Select(a => new SpontaneousApplicationDetailsDto(
            Id: a.Id,
            ShortId: a.ShortId,
            Title: a.TargetPosition,
            CompanyName: a.Company.Name,
            CompanySlug: a.Company.Slug,
            Status: a.Status,
            Channel: a.Channel,
            ChannelDetail: a.ChannelDetail,
            SentOn: a.AppliedOn,
            Notes: a.Notes,
            CreatedAt: a.CreatedAt,
            StatusHistory: a.StatusChanges
                .OrderByDescending(c => c.ChangedAt)
                .Select(c => new StatusChangeDto(
                    From: c.From,
                    To: c.To,
                    ChangedAt: c.ChangedAt,
                    Comment: c.Comment))
                .ToList(),
            CompanyLocation: a.Company.Location));

    /// <summary>A new application to the offer, sent through <paramref name="posting"/> when given, and already sent
    /// when the form says so: its sending is then recorded at <paramref name="createdAt"/> (UTC). The optional text
    /// fields are trimmed, and a blank one is stored as null.</summary>
    public static OfferApplication ToEntity(this NewOfferApplicationInput input, Guid jobOfferId, string shortId, JobOfferPosting? posting, DateTime createdAt)
    {
        var application = new OfferApplication
        {
            ShortId = shortId,
            JobOfferId = jobOfferId,
            Channel = input.Channel,
            ChannelDetail = input.ChannelDetail.NullIfBlank(),
            Notes = input.Notes.NullIfBlank(),
        };

        if (posting is not null)
        {
            application.ApplyVia(posting);
        }

        SendWhenSent(input, application, createdAt);
        return application;
    }

    /// <summary>A new spontaneous application to the company, already sent when the form says so: its sending is then
    /// recorded at <paramref name="createdAt"/> (UTC). The optional text fields are trimmed, and a blank one is stored
    /// as null.</summary>
    public static SpontaneousApplication ToEntity(this NewSpontaneousApplicationInput input, Guid companyId, string shortId, DateTime createdAt)
    {
        var application = new SpontaneousApplication
        {
            ShortId = shortId,
            CompanyId = companyId,
            TargetPosition = input.TargetPosition,
            Channel = input.Channel,
            ChannelDetail = input.ChannelDetail.NullIfBlank(),
            Notes = input.Notes.NullIfBlank(),
        };

        SendWhenSent(input, application, createdAt);
        return application;
    }

    /// <summary>The edit form, filled with the current values of the application.</summary>
    public static OfferApplicationInput ToInput(this OfferApplicationDetailsDto application) => new()
    {
        AppliedViaPostingId = application.AppliedViaPostingId,
        Channel = application.Channel,
        ChannelDetail = application.ChannelDetail,
        AlreadySent = application.SentOn is not null,
        SentOn = application.SentOn,
        Notes = application.Notes,
    };

    /// <summary>The edit form, filled with the current values of the application.</summary>
    public static SpontaneousApplicationInput ToInput(this SpontaneousApplicationDetailsDto application) => new()
    {
        TargetPosition = application.Title,
        Channel = application.Channel,
        ChannelDetail = application.ChannelDetail,
        AlreadySent = application.SentOn is not null,
        SentOn = application.SentOn,
        Notes = application.Notes,
    };

    /// <summary>Copies the fields of the edit form onto the application, sent through <paramref name="posting"/>, or
    /// through an unspecified site when null. Its offer doesn't change.</summary>
    public static void ApplyTo(this OfferApplicationInput input, OfferApplication application, JobOfferPosting? posting)
    {
        if (posting is not null)
        {
            application.ApplyVia(posting);
        }
        else
        {
            application.ClearAppliedVia();
        }

        ApplySharedFields(input, application);
    }

    /// <summary>Copies the fields of the edit form onto the application. Its company doesn't change.</summary>
    public static void ApplyTo(this SpontaneousApplicationInput input, SpontaneousApplication application)
    {
        application.TargetPosition = input.TargetPosition;
        ApplySharedFields(input, application);
    }

    /// <summary>Moves the application to the status of the form, recorded at <paramref name="changedAt"/> (UTC). The
    /// comment is trimmed, and a blank one is stored as null. The service checks first that the workflow allows
    /// it.</summary>
    public static void ApplyTo(this StatusChangeInput input, JobApplication application, DateTime changedAt) =>
        application.ChangeStatus(input.To!.Value, changedAt, input.Comment.NullIfBlank(), input.SentOn);

    // The optional text fields are trimmed, and a blank one is stored as null. Only the sending day of an application
    // already sent is corrected: a draft is sent by changing its status.
    private static void ApplySharedFields(JobApplicationInput input, JobApplication application)
    {
        application.Channel = input.Channel;
        application.ChannelDetail = input.ChannelDetail.NullIfBlank();
        application.Notes = input.Notes.NullIfBlank();

        if (application.AppliedOn is not null && input.SentOn is { } sentOn)
        {
            application.ChangeAppliedOn(sentOn);
        }
    }

    // Every application is born a draft: one already sent records its sending as its first change. The form requires
    // the date once "Already sent" is checked.
    private static void SendWhenSent(JobApplicationInput input, JobApplication application, DateTime createdAt)
    {
        if (input is { AlreadySent: true, SentOn: { } sentOn })
        {
            application.ChangeStatus(ApplicationStatus.Applied, createdAt, appliedOn: sentOn);
        }
    }
}
