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

    /// <summary>A new application to the offer, sent through <paramref name="posting"/> when given, and already sent
    /// when the form says so. The optional text fields are trimmed, and a blank one is stored as null.</summary>
    public static OfferApplication ToEntity(this NewOfferApplicationInput input, Guid jobOfferId, string shortId, JobOfferPosting? posting)
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

        MarkAsSentWhenSent(input, application);
        return application;
    }

    /// <summary>A new spontaneous application to the company, already sent when the form says so. The optional text
    /// fields are trimmed, and a blank one is stored as null.</summary>
    public static SpontaneousApplication ToEntity(this NewSpontaneousApplicationInput input, Guid companyId, string shortId)
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

        MarkAsSentWhenSent(input, application);
        return application;
    }

    // The form requires the date once "Already sent" is checked.
    private static void MarkAsSentWhenSent(NewJobApplicationInput input, JobApplication application)
    {
        if (input is { AlreadySent: true, SentOn: { } sentOn })
        {
            application.MarkAsSent(sentOn);
        }
    }
}
