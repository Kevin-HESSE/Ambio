using Ambio.Application.Common;
using Ambio.Application.JobApplications.Dtos;
using Ambio.Domain.JobApplications;
using Ambio.Domain.Offers;

namespace Ambio.Application.JobApplications;

public static class JobApplicationMappingExtensions
{
    /// <summary>Projection translated to SQL by EF Core, with the offer, its company and the posting used.</summary>
    public static IQueryable<JobApplicationSummaryDto> ToSummaryDto(this IQueryable<OfferApplication> applications) =>
        applications.Select(a => new JobApplicationSummaryDto(
            Id: a.Id,
            Title: a.JobOffer.Title,
            CompanyName: a.JobOffer.Company.Name,
            Status: a.Status,
            Channel: a.Channel,
            AppliedVia: a.AppliedViaPosting == null ? null : a.AppliedViaPosting.Site,
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

        // The form requires the date once "Already sent" is checked.
        if (input is { AlreadySent: true, SentOn: { } sentOn })
        {
            application.MarkAsSent(sentOn);
        }

        return application;
    }
}
