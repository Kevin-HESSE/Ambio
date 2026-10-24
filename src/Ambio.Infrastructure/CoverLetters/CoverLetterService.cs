using Ambio.Application.Common;
using Ambio.Application.CoverLetters;
using Ambio.Application.CoverLetters.Dtos;
using Ambio.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace Ambio.Infrastructure.CoverLetters;

internal class CoverLetterService : ICoverLetterService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

    public CoverLetterService(IDbContextFactory<ApplicationDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<IReadOnlyList<CoverLetterDto>> ListAsync(Guid applicationId, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);

        // GUID v7 keys follow the creation order.
        return await db.CoverLetters
            .AsNoTracking()
            .Where(l => l.JobApplicationId == applicationId)
            .OrderBy(l => l.CreatedAt)
            .ThenBy(l => l.Id)
            .ToDto()
            .ToListAsync(cancellationToken);
    }

    public async Task<ServiceResult<Guid>> CreateAsync(Guid applicationId, CoverLetterInput input, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);

        if (!await db.JobApplications.AnyAsync(a => a.Id == applicationId, cancellationToken))
        {
            return ServiceResult<Guid>.Failure(CoverLetterErrors.ApplicationNotFound);
        }

        var letter = input.ToEntity(applicationId);
        db.CoverLetters.Add(letter);
        await db.SaveChangesAsync(cancellationToken);

        return ServiceResult<Guid>.Success(letter.Id);
    }

    public async Task<ServiceResult> UpdateAsync(Guid id, CoverLetterInput input, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);

        var letter = await db.CoverLetters.SingleOrDefaultAsync(l => l.Id == id, cancellationToken);
        if (letter is null)
        {
            return ServiceResult.Failure(CoverLetterErrors.NotFound);
        }

        input.ApplyTo(letter);
        await db.SaveChangesAsync(cancellationToken);

        return ServiceResult.Success();
    }

    public async Task<ServiceResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);

        var letter = await db.CoverLetters.SingleOrDefaultAsync(l => l.Id == id, cancellationToken);
        if (letter is null)
        {
            return ServiceResult.Failure(CoverLetterErrors.NotFound);
        }

        db.CoverLetters.Remove(letter);
        await db.SaveChangesAsync(cancellationToken);

        return ServiceResult.Success();
    }
}
