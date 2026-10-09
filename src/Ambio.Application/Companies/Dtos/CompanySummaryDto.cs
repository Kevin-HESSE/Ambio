using Ambio.Domain.Companies;

namespace Ambio.Application.Companies.Dtos;

/// <summary>A row of the companies list.</summary>
public record CompanySummaryDto(Guid Id, string Name, CompanyKind Kind, string? Industry, string? Location);
