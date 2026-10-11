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

    public async Task<CompanyDetailsDto?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);

        return await db.Companies
            .AsNoTracking()
            .Where(c => c.Id == id)
            .ToDetailsDto()
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<ServiceResult<Guid>> CreateAsync(CompanyInput input, CancellationToken cancellationToken = default)
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

    public async Task<ServiceResult> UpdateAsync(Guid id, CompanyInput input, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);

        var company = await db.Companies.SingleOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (company is null)
        {
            return ServiceResult.Failure(CompanyErrors.NotFound);
        }

        input.ApplyTo(company);

        // Same NOCASE comparison as CreateAsync. The company itself is left out, so its name can change case.
        if (await db.Companies.AnyAsync(c => c.Id != id && c.Name == company.Name, cancellationToken))
        {
            return ServiceResult.Failure(CompanyErrors.AlreadyExists);
        }

        await db.SaveChangesAsync(cancellationToken);

        return ServiceResult.Success();
    }
}
