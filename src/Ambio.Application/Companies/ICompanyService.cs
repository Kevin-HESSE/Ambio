using Ambio.Application.Common;
using Ambio.Application.Companies.Dtos;

namespace Ambio.Application.Companies;

public interface ICompanyService
{
    /// <summary>Active companies (not archived), and the archived ones too with <paramref name="includeArchived"/>, sorted
    /// by name.</summary>
    public Task<CompanyListDto> ListAsync(bool includeArchived = false, CancellationToken cancellationToken = default);

    /// <summary>Active companies whose name contains every word of <paramref name="text"/>, ignoring case and accents,
    /// sorted by name; none for a blank text.</summary>
    public Task<IReadOnlyList<CompanySummaryDto>> SearchAsync(string text, CancellationToken cancellationToken = default);

    /// <summary>The company, archived or not, or null when no company has this slug.</summary>
    public Task<CompanyDetailsDto?> GetAsync(string slug, CancellationToken cancellationToken = default);

    /// <summary>Creates the company and returns its slug, or fails with <see cref="CompanyErrors.AlreadyExists"/>
    /// when a company, archived or not, has the same name ignoring case.</summary>
    public Task<ServiceResult<string>> CreateAsync(CompanyInput input, CancellationToken cancellationToken = default);

    /// <summary>Replaces the fields of the company and returns its slug, recomputed when the name changes, or fails
    /// with <see cref="CompanyErrors.NotFound"/>, or with <see cref="CompanyErrors.AlreadyExists"/> when another
    /// company, archived or not, has the same name ignoring case.</summary>
    public Task<ServiceResult<string>> UpdateAsync(Guid id, CompanyInput input, CancellationToken cancellationToken = default);

    /// <summary>Archives the company with its active offers and applications, to its offers or spontaneous, all on the
    /// same day, or fails with <see cref="CompanyErrors.NotFound"/>. A row already archived keeps its day, and an
    /// archived company is left as it is.</summary>
    public Task<ServiceResult> ArchiveAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Restores the company with the offers and applications archived with it, on the same day, or fails with
    /// <see cref="CompanyErrors.NotFound"/>. Those archived before stay archived, and an active company is left as it
    /// is.</summary>
    public Task<ServiceResult> RestoreAsync(Guid id, CancellationToken cancellationToken = default);
}
