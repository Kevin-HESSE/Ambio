namespace Ambio.Application.Offers.Dtos;

/// <summary>The offers list, newest first. <paramref name="HasArchived"/> tells whether any offer is archived, listed
/// or not, so an empty list can say that every offer is archived.</summary>
public record JobOfferListDto(IReadOnlyList<JobOfferSummaryDto> Offers, bool HasArchived);
