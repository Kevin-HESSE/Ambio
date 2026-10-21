using Ambio.Domain.JobApplications;
using Ambio.Domain.Offers;

namespace Ambio.Application.JobApplications.Dtos;

/// <summary>An application to an offer, with what its page shows of the offer. <paramref name="AppliedViaPostingId"/>,
/// <paramref name="AppliedViaSite"/> and <paramref name="AppliedViaUrl"/> are those of the posting used to apply, null
/// when not specified. The offer's page is reached by <paramref name="JobOfferShortId"/> and <see cref="JobOfferSlug"/>.</summary>
public record OfferApplicationDetailsDto(
    Guid Id,
    string ShortId,
    string Title,
    string CompanyName,
    string CompanySlug,
    ApplicationStatus Status,
    Channel Channel,
    string? ChannelDetail,
    DateOnly? SentOn,
    string? Notes,
    DateTime CreatedAt,
    Guid JobOfferId,
    string JobOfferShortId,
    string? Location,
    ContractType ContractType,
    RemotePolicy RemotePolicy,
    string? Salary,
    Guid? AppliedViaPostingId,
    string? AppliedViaSite,
    string? AppliedViaUrl)
    : JobApplicationDetailsDto(Id, ShortId, Title, CompanyName, CompanySlug, Status, Channel, ChannelDetail, SentOn, Notes, CreatedAt)
{
    /// <summary>The readable part of the offer's URL, after its <see cref="JobOfferShortId"/>: the application's title
    /// and company are the offer's.</summary>
    public string JobOfferSlug => Offers.JobOfferSlug.From(Title, CompanyName);
}
