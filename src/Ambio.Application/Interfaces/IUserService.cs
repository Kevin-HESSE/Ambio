using Ambio.Application.Common;
using Ambio.Application.Dtos.Users;

namespace Ambio.Application.Interfaces;

public interface IUserService
{
    public Task<bool> HasUserAsync();
    public Task<ServiceResult<string>> RegisterUserAsync(RegisterInput input);
    public Task<ServiceResult> GenerateConfirmEmailAsync(string userId, string callbackUrl);
}
