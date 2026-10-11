using Ambio.Application.Common;
using Ambio.Application.Companies.Dtos;
using Ambio.Domain.Companies;

namespace Ambio.Web.Tests.Fixtures;

/// <summary>Company profiles, as returned by the mocked <c>ICompanyService</c>.</summary>
public static class CompanyDetailsFactory
{
    /// <summary>A company whose slug is the one of its name. The optional fields stay empty unless given.</summary>
    public static CompanyDetailsDto New(
        string name,
        CompanyKind kind = CompanyKind.Employer,
        string? industry = null,
        CompanySize? size = null,
        string? location = null,
        string? website = null,
        string? linkedInUrl = null,
        string? description = null) => new(
            Id: Guid.CreateVersion7(),
            Slug: Slug.From(name, "company"),
            Name: name,
            Kind: kind,
            Industry: industry,
            Size: size,
            Location: location,
            Website: website,
            LinkedInUrl: linkedInUrl,
            Description: description);
}
