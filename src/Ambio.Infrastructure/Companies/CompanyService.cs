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

    public async Task<IReadOnlyList<CompanySummaryDto>> SearchAsync(string text, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return [];
        }

        // SQLite compares neither ignoring accents nor word by word: the few companies are matched once loaded.
        var companies = await ListAsync(cancellationToken);

        return companies.Where(c => TextSearch.Matches(c.Name, text)).ToList();
    }

    public async Task<CompanyDetailsDto?> GetAsync(string slug, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);

        return await db.Companies
            .AsNoTracking()
            .Where(c => c.Slug == slug)
            .ToDetailsDto()
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<ServiceResult<string>> CreateAsync(CompanyInput input, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);

        // The Name column is COLLATE NOCASE: the comparison ignores case, like the unique index.
        var name = input.Name.Trim();
        if (await db.Companies.AnyAsync(c => c.Name == name, cancellationToken))
        {
            return ServiceResult<string>.Failure(CompanyErrors.AlreadyExists);
        }

        var company = input.ToEntity(await CompanySlugs.FreeSlugAsync(db, name, companyId: null, cancellationToken));
        db.Companies.Add(company);
        await db.SaveChangesAsync(cancellationToken);

        return ServiceResult<string>.Success(company.Slug);
    }

    public async Task<ServiceResult<string>> UpdateAsync(Guid id, CompanyInput input, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);

        var company = await db.Companies.SingleOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (company is null)
        {
            return ServiceResult<string>.Failure(CompanyErrors.NotFound);
        }

        var previousName = company.Name;
        input.ApplyTo(company);

        // Same NOCASE comparison as CreateAsync. The company itself is left out, so its name can change case.
        if (await db.Companies.AnyAsync(c => c.Id != id && c.Name == company.Name, cancellationToken))
        {
            return ServiceResult<string>.Failure(CompanyErrors.AlreadyExists);
        }

        // Only a rename moves the company to another URL: old links aren't kept.
        if (company.Name != previousName)
        {
            company.Slug = await CompanySlugs.FreeSlugAsync(db, company.Name, company.Id, cancellationToken);
        }

        await db.SaveChangesAsync(cancellationToken);

        return ServiceResult<string>.Success(company.Slug);
    }
}
