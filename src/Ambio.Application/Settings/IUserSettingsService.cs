using Ambio.Application.Settings.Dtos;

namespace Ambio.Application.Settings;

public interface IUserSettingsService
{
    /// <summary>The settings as saved, or the defaults while they never were.</summary>
    public Task<UserSettingsDto> GetAsync(CancellationToken cancellationToken = default);

    /// <summary>Saves the settings, in their single row, created by the first save.</summary>
    public Task UpdateAsync(UserSettingsInput input, CancellationToken cancellationToken = default);
}
