using Ambio.Application.Common;
using Ambio.Application.Users.Dtos;

namespace Ambio.Application.Users;

public interface IUserService
{
    public Task<bool> HasUserAsync(CancellationToken cancellationToken = default);
    public Task<ServiceResult<string>> RegisterUserAsync(RegisterInput input, CancellationToken cancellationToken = default);
    public Task<ServiceResult> GenerateConfirmEmailAsync(string userId, string callbackUrl);
}
