using Ambio.Application.Common;
using Ambio.Application.Companies.Dtos;

namespace Ambio.Application.Companies;

public interface ICompanyService
{
    /// <summary>Active companies (not archived), sorted by name.</summary>
    public Task<IReadOnlyList<CompanySummaryDto>> ListAsync(CancellationToken cancellationToken = default);

    /// <summary>Creates the company and returns its id, or fails with <see cref="CompanyErrors.AlreadyExists"/>
    /// when a company, archived or not, has the same name ignoring case.</summary>
    public Task<ServiceResult<Guid>> CreateAsync(CreateCompanyInput input, CancellationToken cancellationToken = default);
}
