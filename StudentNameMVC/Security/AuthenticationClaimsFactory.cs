using System.Security.Claims;
using AIVES.BLL.Models;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace StudentNameMVC.Security;

public static class AuthenticationClaimsFactory
{
    public static ClaimsPrincipal CreatePrincipal(AuthenticatedUser user)
    {
        ArgumentNullException.ThrowIfNull(user);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.SubjectId),
            new Claim(ClaimTypes.Name, user.DisplayName),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme,
            ClaimTypes.Name,
            ClaimTypes.Role);

        return new ClaimsPrincipal(identity);
    }
}
