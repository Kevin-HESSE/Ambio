using Ambio.Application.Companies.Dtos;
using Ambio.Domain.Companies;

namespace Ambio.Application.Companies;

public static class CompanyMappingExtensions
{
    /// <summary>Projection translated to SQL by EF Core: only the listed columns are read.</summary>
    public static IQueryable<CompanySummaryDto> ToSummaryDto(this IQueryable<Company> companies) =>
        companies.Select(c => new CompanySummaryDto(c.Id, c.Name, c.Kind, c.Industry, c.Location));
}
