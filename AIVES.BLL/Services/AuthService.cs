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

public sealed class AuthService : IAuthService
{
    private readonly ISystemAccountRepository _accountRepository;
    private readonly IPasswordHasher<SystemAccount> _passwordHasher;
    private readonly IOptions<DefaultAdminOptions> _defaultAdminOptions;
    private readonly IDataProtector? _resetProtector;
    private readonly TimeProvider _clock;

    public AuthService(
        ISystemAccountRepository accountRepository,
        IPasswordHasher<SystemAccount> passwordHasher,
        IOptions<DefaultAdminOptions> defaultAdminOptions,
        IDataProtectionProvider? dataProtectionProvider = null,
        TimeProvider? clock = null)
    {
        _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        _defaultAdminOptions = defaultAdminOptions ?? throw new ArgumentNullException(nameof(defaultAdminOptions));
        _resetProtector = dataProtectionProvider?.CreateProtector("AIVES.PasswordReset.v1");
        _clock = clock ?? TimeProvider.System;
    }

    private sealed record ResetPayload(int AccountId, string Email, string PasswordFingerprint, DateTimeOffset Expires);
    private static string Fingerprint(string hash) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(hash)));

    public async Task<string?> CreatePasswordResetTokenAsync(string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email) || _resetProtector is null) return null;
        var normalized = NormalizeEmail(email);
        if (normalized == NormalizeEmail(_defaultAdminOptions.Value.Email)) return null;
        var account = await _accountRepository.GetByEmailAsync(normalized, cancellationToken);
        if (account is null || account.IsDeleted || ApplicationRoles.FromDatabaseRole(account.AccountRole) is null) return null;
        return _resetProtector.Protect(JsonSerializer.Serialize(new ResetPayload(account.AccountId,
            normalized, Fingerprint(account.AccountPasswordHash), _clock.GetUtcNow().AddMinutes(20))));
    }

    public async Task<bool> ResetPasswordAsync(string email, string token, string newPassword,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token) || token.Length > 4096 ||
            string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 8 || newPassword.Length > 128 || _resetProtector is null) return false;
        ResetPayload? payload;
        try { payload = JsonSerializer.Deserialize<ResetPayload>(_resetProtector.Unprotect(token)); }
        catch (Exception error) when (error is CryptographicException or JsonException or FormatException)
        { return false; }
        if (payload is null || payload.Expires <= _clock.GetUtcNow() || payload.Email != NormalizeEmail(email)) return false;
        var account = await _accountRepository.GetByIdAsync(payload.AccountId, ct: cancellationToken);
        if (account is null || account.IsDeleted || NormalizeEmail(account.AccountEmail) != payload.Email ||
            ApplicationRoles.FromDatabaseRole(account.AccountRole) is null ||
            Fingerprint(account.AccountPasswordHash) != payload.PasswordFingerprint) return false;
        var newHash = _passwordHasher.HashPassword(account, newPassword);
        return await _accountRepository.ResetPasswordAsync(account.AccountId, account.AccountEmail,
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
        var admin = _defaultAdminOptions.Value;
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

        var account = await _accountRepository.GetByEmailAsync(normalizedEmail, cancellationToken);
        if (account is null || account.IsDeleted)
        {
            return AuthenticationResult.Failure;
        }

        var role = ApplicationRoles.FromDatabaseRole(account.AccountRole);
        if (role is null)
        {
            return AuthenticationResult.Failure;
        }

        var verification = _passwordHasher.VerifyHashedPassword(
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

    public async Task<bool> IsAccountActiveAsync(
        int accountId,
        CancellationToken cancellationToken = default)
    {
        if (accountId <= 0)
        {
            return false;
        }

        var account = await _accountRepository.GetByIdAsync(
            accountId,
            includeDeleted: false,
            cancellationToken);

        return account is { IsDeleted: false };
    }

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}
