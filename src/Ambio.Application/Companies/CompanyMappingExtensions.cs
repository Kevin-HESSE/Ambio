using Ambio.Application.Common;
using Ambio.Application.Companies.Dtos;
using Ambio.Domain.Companies;

namespace Ambio.Application.Companies;

public static class CompanyMappingExtensions
{
    /// <summary>Projection translated to SQL by EF Core: only the listed columns are read.</summary>
    public static IQueryable<CompanySummaryDto> ToSummaryDto(this IQueryable<Company> companies) =>
        companies.Select(c => new CompanySummaryDto(
            Id: c.Id,
            Slug: c.Slug,
            Name: c.Name,
            Kind: c.Kind,
            Industry: c.Industry,
            Location: c.Location,
            IsArchived: c.ArchivedAt != null));

    /// <summary>Projection translated to SQL by EF Core.</summary>
    public static IQueryable<CompanyDetailsDto> ToDetailsDto(this IQueryable<Company> companies) =>
        companies.Select(c => new CompanyDetailsDto(
            Id: c.Id,
            Slug: c.Slug,
            Name: c.Name,
            Kind: c.Kind,
            Industry: c.Industry,
            Size: c.Size,
            Location: c.Location,
            Website: c.Website,
            LinkedInUrl: c.LinkedInUrl,
            Description: c.Description,
            ArchivedAt: c.ArchivedAt));

    /// <summary>A new company with the fields of the form, see <see cref="ApplyTo"/>, and its slug.</summary>
    public static Company ToEntity(this CompanyInput input, string slug)
    {
        var company = new Company { Name = input.Name, Slug = slug, Kind = input.Kind };
        input.ApplyTo(company);
        return company;
    }

    /// <summary>Copies the fields of the form onto the company, but not its slug. The optional text fields
    /// are trimmed, and a blank one is stored as null.</summary>
    public static void ApplyTo(this CompanyInput input, Company company)
    {
        company.Name = input.Name;
        company.Kind = input.Kind;
        company.Industry = input.Industry.NullIfBlank();
        company.Size = input.Size;
        company.Location = input.Location.NullIfBlank();
        company.Website = input.Website.NullIfBlank();
        company.LinkedInUrl = input.LinkedInUrl.NullIfBlank();
        company.Description = input.Description.NullIfBlank();
    }

    /// <summary>The edit form, filled with the current values of the company.</summary>
    public static CompanyInput ToInput(this CompanyDetailsDto company) => new()
    {
        Name = company.Name,
        Kind = company.Kind,
        Industry = company.Industry,
        Size = company.Size,
        Location = company.Location,
        Website = company.Website,
        LinkedInUrl = company.LinkedInUrl,
        Description = company.Description,
    };
}
