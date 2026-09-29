using System.Security.Claims;

namespace AIVES.BLL.Interfaces;

public interface IAuthService
{
    Task<ClaimsPrincipal?> AuthenticateAsync(string email, string password, CancellationToken ct = default);
}
