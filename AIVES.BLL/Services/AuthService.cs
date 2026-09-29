using AIVES.BLL.Interfaces;
using AIVES.BLL.Models;
using AIVES.BLL.Options;
using AIVES.BLL.Security;
using AIVES.DAL.Entities;
using AIVES.DAL.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace AIVES.BLL.Services;

public sealed class AuthService(
    ISystemAccountRepository accountRepository,
    IPasswordHasher<SystemAccount> passwordHasher,
    IOptions<DefaultAdminOptions> defaultAdminOptions) : IAuthService
{
    public async Task<AuthenticationResult> AuthenticateAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return AuthenticationResult.Failure;
        }

        var normalizedEmail = NormalizeEmail(email);
        var admin = defaultAdminOptions.Value;
        if (string.Equals(normalizedEmail, NormalizeEmail(admin.Email), StringComparison.Ordinal))
        {
            return string.Equals(password, admin.Password, StringComparison.Ordinal)
                ? AuthenticationResult.Success(
                    ApplicationRoles.AdminSubjectId,
                    "Administrator",
                    normalizedEmail,
                    ApplicationRoles.Admin)
                : AuthenticationResult.Failure;
        }

        var account = await accountRepository.GetByEmailAsync(normalizedEmail, cancellationToken);
        if (account is null || account.IsDeleted)
        {
            return AuthenticationResult.Failure;
        }

        var role = ApplicationRoles.FromDatabaseRole(account.AccountRole);
        if (role is null)
        {
            return AuthenticationResult.Failure;
        }

        var verification = passwordHasher.VerifyHashedPassword(
            account,
            account.AccountPasswordHash,
            password);

        return verification == PasswordVerificationResult.Failed
            ? AuthenticationResult.Failure
            : AuthenticationResult.Success(
                account.AccountId.ToString(),
                account.AccountName,
                account.AccountEmail,
                role);
    }

    public async Task<bool> IsAccountSessionValidAsync(
        int accountId,
        string claimedRole,
        CancellationToken cancellationToken = default)
    {
        if (accountId <= 0 || string.IsNullOrWhiteSpace(claimedRole))
        {
            return false;
        }

        var account = await accountRepository.GetByIdAsync(
            accountId,
            includeDeleted: false,
            cancellationToken);

        if (account is not { IsDeleted: false })
        {
            return false;
        }

        var currentRole = ApplicationRoles.FromDatabaseRole(account.AccountRole);
        return currentRole is not null &&
               string.Equals(currentRole, claimedRole, StringComparison.Ordinal);
    }

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}
