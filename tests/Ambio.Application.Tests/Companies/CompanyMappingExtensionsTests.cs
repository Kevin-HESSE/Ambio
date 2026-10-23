using Ambio.Application.Companies;
using Ambio.Application.Companies.Dtos;
using Ambio.Domain.Companies;

namespace Ambio.Application.Tests.Companies;

public class CompanyMappingExtensionsTests
{
    [Fact]
    public void ToInput_CopiesEveryField()
    {
        var company = new CompanyDetailsDto(
            Guid.CreateVersion7(),
            "brightwave",
            "Brightwave",
            CompanyKind.ServiceCompany,
            "Software",
            CompanySize.Medium,
            "Lyon",
            "https://brightwave.io",
            "https://www.linkedin.com/company/brightwave",
            "Payroll software for small companies.",
            ArchivedAt: null);

        var input = company.ToInput();

        Assert.Equal(
            new CompanyInput
            {
                Name = company.Name,
                Kind = company.Kind,
                Industry = company.Industry,
                Size = company.Size,
                Location = company.Location,
                Website = company.Website,
                LinkedInUrl = company.LinkedInUrl,
                Description = company.Description,
            },
            input);
    }
}
