using Ambio.Domain.Offers;

namespace Ambio.Application.Offers.Dtos;

/// <summary>A row of the offers list. <paramref name="RegisterAt"/> (UTC) is shown as the publication date.</summary>
public record JobOfferSummaryDto(
    Guid Id,
    string Title,
    string CompanyName,
    string CompanySlug,
    ContractType ContractType,
    RemotePolicy RemotePolicy,
    int PostingCount,
    DateTime RegisterAt);
