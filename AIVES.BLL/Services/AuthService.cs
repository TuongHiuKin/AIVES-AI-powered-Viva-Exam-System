using AIVES.BLL.Interfaces;
using AIVES.BLL.Models;
using AIVES.BLL.Options;
using AIVES.BLL.Security;
using AIVES.DAL.Entities;
using AIVES.DAL.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.DataProtection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace AIVES.BLL.Services;

public sealed class AuthService(
    ISystemAccountRepository accountRepository,
    IPasswordHasher<SystemAccount> passwordHasher,
    IOptions<DefaultAdminOptions> defaultAdminOptions,
    IDataProtectionProvider dataProtectionProvider,
    TimeProvider clock) : IAuthService
{
    private readonly IDataProtector resetProtector = dataProtectionProvider.CreateProtector("AIVES.PasswordReset.v1");
    private sealed record ResetPayload(int AccountId, string Email, string PasswordFingerprint, DateTimeOffset Expires);
    private static string Fingerprint(string hash) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(hash)));

    public async Task<string?> CreatePasswordResetTokenAsync(string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email)) return null;
        var normalized = NormalizeEmail(email);
        if (normalized == NormalizeEmail(defaultAdminOptions.Value.Email)) return null;
        var account = await accountRepository.GetByEmailAsync(normalized, cancellationToken);
        if (account is null || account.IsDeleted || ApplicationRoles.FromDatabaseRole(account.AccountRole) is null) return null;
        return resetProtector.Protect(JsonSerializer.Serialize(new ResetPayload(account.AccountId,
            normalized, Fingerprint(account.AccountPasswordHash), clock.GetUtcNow().AddMinutes(20))));
    }

    public async Task<bool> ResetPasswordAsync(string email, string token, string newPassword,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token) || token.Length > 4096 ||
            string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 8 || newPassword.Length > 128) return false;
        ResetPayload? payload;
        try { payload = JsonSerializer.Deserialize<ResetPayload>(resetProtector.Unprotect(token)); }
        catch (Exception error) when (error is CryptographicException or JsonException or FormatException)
        { return false; }
        if (payload is null || payload.Expires <= clock.GetUtcNow() || payload.Email != NormalizeEmail(email)) return false;
        var account = await accountRepository.GetByIdAsync(payload.AccountId, ct: cancellationToken);
        if (account is null || account.IsDeleted || NormalizeEmail(account.AccountEmail) != payload.Email ||
            ApplicationRoles.FromDatabaseRole(account.AccountRole) is null ||
            Fingerprint(account.AccountPasswordHash) != payload.PasswordFingerprint) return false;
        var newHash = passwordHasher.HashPassword(account, newPassword);
        return await accountRepository.ResetPasswordAsync(account.AccountId, account.AccountEmail,
            account.AccountPasswordHash, newHash, cancellationToken);
    }

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
