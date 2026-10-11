using Ambio.Application.Common;
using Ambio.Application.Companies.Dtos;

namespace Ambio.Application.Companies;

public interface ICompanyService
{
    /// <summary>Active companies (not archived), sorted by name.</summary>
    public Task<IReadOnlyList<CompanySummaryDto>> ListAsync(CancellationToken cancellationToken = default);

    /// <summary>The company, archived or not, or null when no company has this slug.</summary>
    public Task<CompanyDetailsDto?> GetAsync(string slug, CancellationToken cancellationToken = default);

    /// <summary>Creates the company and returns its slug, or fails with <see cref="CompanyErrors.AlreadyExists"/>
    /// when a company, archived or not, has the same name ignoring case.</summary>
    public Task<ServiceResult<string>> CreateAsync(CompanyInput input, CancellationToken cancellationToken = default);

    /// <summary>Replaces the fields of the company and returns its slug, recomputed when the name changes, or fails
    /// with <see cref="CompanyErrors.NotFound"/>, or with <see cref="CompanyErrors.AlreadyExists"/> when another
    /// company, archived or not, has the same name ignoring case.</summary>
    public Task<ServiceResult<string>> UpdateAsync(Guid id, CompanyInput input, CancellationToken cancellationToken = default);
}
