using Ambio.Application.Companies.Dtos;
using Ambio.Domain.Companies;

namespace Ambio.Application.Companies;

public static class CompanyMappingExtensions
{
    /// <summary>Projection translated to SQL by EF Core: only the listed columns are read.</summary>
    public static IQueryable<CompanySummaryDto> ToSummaryDto(this IQueryable<Company> companies) =>
        companies.Select(c => new CompanySummaryDto(c.Id, c.Name, c.Kind, c.Industry, c.Location));

    /// <summary>The optional text fields are trimmed, and a blank one is stored as null.</summary>
    public static Company ToEntity(this CreateCompanyInput input) => new()
    {
        Name = input.Name,
        Kind = input.Kind,
        Industry = NullIfBlank(input.Industry),
        Size = input.Size,
        Location = NullIfBlank(input.Location),
        Website = NullIfBlank(input.Website),
        LinkedInUrl = NullIfBlank(input.LinkedInUrl),
        Description = NullIfBlank(input.Description),
    };

    private static string? NullIfBlank(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
