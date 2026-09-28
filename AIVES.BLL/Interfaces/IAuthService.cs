using AIVES.BLL.Models;

namespace AIVES.BLL.Interfaces;

public interface IAuthService
{
    Task<AuthenticationResult> AuthenticateAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default);

    Task<bool> IsAccountActiveAsync(
        int accountId,
        CancellationToken cancellationToken = default);
}
