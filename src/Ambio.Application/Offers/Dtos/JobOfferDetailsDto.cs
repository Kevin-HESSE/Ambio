using Ambio.Domain.Offers;

namespace Ambio.Application.Offers.Dtos;

/// <summary>An offer and its postings, shown on its detail page. <paramref name="RegisterAt"/> (UTC) is shown as
/// the publication date; <paramref name="Postings"/> are sorted from the first seen.</summary>
public record JobOfferDetailsDto(
    Guid Id,
    string Title,
    string CompanyName,
    string CompanySlug,
    ContractType ContractType,
    RemotePolicy RemotePolicy,
    string? Location,
    string? Salary,
    string? Description,
    DateTime RegisterAt,
    IReadOnlyList<JobOfferPostingDto> Postings);
