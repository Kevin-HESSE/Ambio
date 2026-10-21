using Ambio.Domain.Offers;

namespace Ambio.Application.Offers.Dtos;

/// <summary>A row of the offers list. <paramref name="RegisterAt"/> (UTC) is shown as the publication date.</summary>
public record JobOfferSummaryDto(
    Guid Id,
    string ShortId,
    string Title,
    string CompanyName,
    string CompanySlug,
    ContractType ContractType,
    RemotePolicy RemotePolicy,
    int PostingCount,
    DateTime RegisterAt)
{
    /// <summary>The readable part of its URL, after the <see cref="ShortId"/>.</summary>
    public string Slug => JobOfferSlug.From(Title, CompanyName);
}
