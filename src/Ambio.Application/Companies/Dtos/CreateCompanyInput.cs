using System.ComponentModel.DataAnnotations;

using Ambio.Application.Common.Attributes;
using Ambio.Domain.Companies;

namespace Ambio.Application.Companies.Dtos;

/// <summary>The fields of the new company form.</summary>
public record CreateCompanyInput
{
    [Required(ErrorMessage = "Enter the name of the company.")]
    [StringLength(200, ErrorMessage = "The name can't be longer than {1} characters.")]
    public string Name { get; set; } = "";

    public CompanyKind Kind { get; set; } = CompanyKind.Employer;

    public string? Industry { get; set; }

    public CompanySize? Size { get; set; }

    public string? Location { get; set; }

    [HttpUrl]
    public string? Website { get; set; }

    [HttpUrl]
    public string? LinkedInUrl { get; set; }

    public string? Description { get; set; }
}
