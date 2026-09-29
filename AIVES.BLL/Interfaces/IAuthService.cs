using AIVES.BLL.Models;

namespace AIVES.BLL.Interfaces;

public interface IAuthService
{
    Task<AuthenticationResult> AuthenticateAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default);

    Task<bool> IsAccountSessionValidAsync(
        int accountId,
        string claimedRole,
        CancellationToken cancellationToken = default);
}
