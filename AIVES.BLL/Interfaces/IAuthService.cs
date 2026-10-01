using AIVES.BLL.Models;

namespace AIVES.BLL.Interfaces;

public interface IAuthService
{
    Task<string?> CreatePasswordResetTokenAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> ResetPasswordAsync(string email, string token, string newPassword, CancellationToken cancellationToken = default);

    Task<AuthenticationResult> AuthenticateAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default);

    Task<bool> IsAccountSessionValidAsync(
        int accountId,
        string claimedRole,
        CancellationToken cancellationToken = default);
}
