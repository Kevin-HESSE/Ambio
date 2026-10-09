using Ambio.Application.Common;
using Ambio.Application.Companies;
using Ambio.Application.Companies.Dtos;
using Ambio.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace Ambio.Infrastructure.Companies;

internal class CompanyService : ICompanyService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

    public CompanyService(IDbContextFactory<ApplicationDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<IReadOnlyList<CompanySummaryDto>> ListAsync(CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);

        return await db.Companies
            .AsNoTracking()
            .Where(c => c.ArchivedAt == null)
            .OrderBy(c => c.Name)
            .ToSummaryDto()
            .ToListAsync(cancellationToken);
    }

    public async Task<ServiceResult<Guid>> CreateAsync(CreateCompanyInput input, CancellationToken cancellationToken = default)
    {
        var company = input.ToEntity();

        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);

        // The Name column is COLLATE NOCASE: the comparison ignores case, like the unique index.
        if (await db.Companies.AnyAsync(c => c.Name == company.Name, cancellationToken))
        {
            return ServiceResult<Guid>.Failure(CompanyErrors.AlreadyExists);
        }

        db.Companies.Add(company);
        await db.SaveChangesAsync(cancellationToken);

        return ServiceResult<Guid>.Success(company.Id);
    }
}
