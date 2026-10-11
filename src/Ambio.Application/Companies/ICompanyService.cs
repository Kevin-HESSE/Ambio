using Ambio.Application.Common;
using Ambio.Application.Companies.Dtos;

namespace Ambio.Application.Companies;

public interface ICompanyService
{
    /// <summary>Active companies (not archived), sorted by name.</summary>
    public Task<IReadOnlyList<CompanySummaryDto>> ListAsync(CancellationToken cancellationToken = default);

    /// <summary>The company, archived or not, or null when no company has this id.</summary>
    public Task<CompanyDetailsDto?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Creates the company and returns its id, or fails with <see cref="CompanyErrors.AlreadyExists"/>
    /// when a company, archived or not, has the same name ignoring case.</summary>
    public Task<ServiceResult<Guid>> CreateAsync(CompanyInput input, CancellationToken cancellationToken = default);

    /// <summary>Replaces the fields of the company, or fails with <see cref="CompanyErrors.NotFound"/>, or with
    /// <see cref="CompanyErrors.AlreadyExists"/> when another company, archived or not, has the same name ignoring case.</summary>
    public Task<ServiceResult> UpdateAsync(Guid id, CompanyInput input, CancellationToken cancellationToken = default);
}
