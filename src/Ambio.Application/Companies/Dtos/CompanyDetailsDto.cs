using Ambio.Domain.Companies;

namespace Ambio.Application.Companies.Dtos;

/// <summary>The profile of a company, shown on its detail page. <paramref name="ArchivedAt"/> (UTC) is null while it
/// is active.</summary>
public record CompanyDetailsDto(
    Guid Id,
    string Slug,
    string Name,
    CompanyKind Kind,
    string? Industry,
    CompanySize? Size,
    string? Location,
    string? Website,
    string? LinkedInUrl,
    string? Description,
    DateTime? ArchivedAt)
{
    public bool IsArchived => ArchivedAt is not null;
}
