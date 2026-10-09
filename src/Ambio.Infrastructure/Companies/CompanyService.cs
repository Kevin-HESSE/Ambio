using Ambio.Application.Companies;
using Ambio.Application.Companies.Dtos;
using Ambio.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace Ambio.Infrastructure.Companies;

internal class CompanyService(IDbContextFactory<ApplicationDbContext> dbFactory) : ICompanyService
{
    public async Task<IReadOnlyList<CompanySummaryDto>> ListAsync(CancellationToken cancellationToken = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);

        return await db.Companies
            .AsNoTracking()
            .Where(c => c.ArchivedAt == null)
            .OrderBy(c => c.Name)
            .ToSummaryDto()
            .ToListAsync(cancellationToken);
    }
}
