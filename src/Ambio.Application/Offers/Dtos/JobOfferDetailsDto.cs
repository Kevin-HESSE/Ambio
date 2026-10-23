using Ambio.Domain.Offers;

namespace Ambio.Application.Offers.Dtos;

/// <summary>An offer and its postings, shown on its detail page. <paramref name="RegisterAt"/> (UTC) is shown as
/// the publication date, and <paramref name="ArchivedAt"/> (UTC) is null while it is active; <paramref name="Postings"/>
/// are sorted from the first seen.</summary>
public record JobOfferDetailsDto(
    Guid Id,
    string ShortId,
    string Title,
    string CompanyName,
    string CompanySlug,
    ContractType ContractType,
    RemotePolicy RemotePolicy,
    string? Location,
    string? Salary,
    string? Description,
    DateTime RegisterAt,
    DateTime? ArchivedAt,
    IReadOnlyList<JobOfferPostingDto> Postings)
{
    public bool IsArchived => ArchivedAt is not null;

    /// <summary>The readable part of its URL, after the <see cref="ShortId"/>.</summary>
    public string Slug => JobOfferSlug.From(Title, CompanyName);
}
