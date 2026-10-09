using Ambio.Application.Companies.Dtos;

namespace Ambio.Application.Companies;

public interface ICompanyService
{
    /// <summary>Active companies (not archived), sorted by name.</summary>
    public Task<IReadOnlyList<CompanySummaryDto>> ListAsync(CancellationToken cancellationToken = default);
}
