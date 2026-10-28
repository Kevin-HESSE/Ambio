using Ambio.Application.Settings;
using Ambio.Application.Settings.Dtos;
using Ambio.Domain.Settings;
using Ambio.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace Ambio.Infrastructure.Settings;

internal class UserSettingsService : IUserSettingsService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

    public UserSettingsService(IDbContextFactory<ApplicationDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<UserSettingsDto> GetAsync(CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);

        return await db.UserSettings
                   .AsNoTracking()
                   .ToDto()
                   .SingleOrDefaultAsync(cancellationToken)
               ?? new UserSettingsDto(FollowUpAfterDays: UserSettings.DefaultFollowUpAfterDays);
    }

    public async Task UpdateAsync(UserSettingsInput input, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);

        var settings = await db.UserSettings.SingleOrDefaultAsync(cancellationToken);
        if (settings is null)
        {
            settings = new UserSettings();
            db.UserSettings.Add(settings);
        }

        input.ApplyTo(settings);
        await db.SaveChangesAsync(cancellationToken);
    }
}
