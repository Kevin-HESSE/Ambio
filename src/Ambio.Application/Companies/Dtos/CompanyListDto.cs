namespace Ambio.Application.Companies.Dtos;

/// <summary>The companies list, sorted by name. <paramref name="HasArchived"/> tells whether any company is archived,
/// listed or not, so an empty list can say that every company is archived.</summary>
public record CompanyListDto(IReadOnlyList<CompanySummaryDto> Companies, bool HasArchived);
