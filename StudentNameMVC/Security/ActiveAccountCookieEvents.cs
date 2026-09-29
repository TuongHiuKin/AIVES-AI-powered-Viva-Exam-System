using System.Security.Claims;
using AIVES.BLL.Interfaces;
using AIVES.BLL.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace StudentNameMVC.Security;

public sealed class ActiveAccountCookieEvents(IAuthService authService) : CookieAuthenticationEvents
{
    public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
    {
        var role = context.Principal?.FindFirstValue(ClaimTypes.Role);
        if (string.Equals(role, ApplicationRoles.Admin, StringComparison.Ordinal))
        {
            return;
        }

        var subject = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
        var isDatabaseRole = string.Equals(role, ApplicationRoles.Staff, StringComparison.Ordinal) ||
                             string.Equals(role, ApplicationRoles.Lecturer, StringComparison.Ordinal);

        if (role is null ||
            !isDatabaseRole ||
            !int.TryParse(subject, out var accountId) ||
            !await authService.IsAccountSessionValidAsync(
                accountId,
                role,
                context.HttpContext.RequestAborted))
        {
            context.RejectPrincipal();
            await context.HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);
        }
    }
}
