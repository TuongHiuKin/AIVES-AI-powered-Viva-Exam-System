using System.Security.Claims;
using AIVES.BLL.Interfaces;
using AIVES.DAL.Entities;
using AIVES.DAL.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace AIVES.BLL.Services;

public class AuthService : IAuthService
{
    private readonly ISystemAccountRepository _accountRepository;
    private readonly IConfiguration _configuration;
    private readonly PasswordHasher<SystemAccount> _passwordHasher;

    public AuthService(ISystemAccountRepository accountRepository, IConfiguration configuration)
    {
        _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _passwordHasher = new PasswordHasher<SystemAccount>();
    }

    public async Task<ClaimsPrincipal?> AuthenticateAsync(string email, string password, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return null;
        }

        var normalizedEmail = email.Trim().ToLowerInvariant();

        // 1. Dual Credential Verification: Default Administrator (from appsettings.json)
        var adminEmail = _configuration["DefaultAdmin:Email"]?.Trim().ToLowerInvariant();
        var adminPassword = _configuration["DefaultAdmin:Password"];

        if (!string.IsNullOrEmpty(adminEmail) && normalizedEmail == adminEmail)
        {
            if (password == adminPassword)
            {
                var adminClaims = new List<Claim>
                {
                    new(ClaimTypes.NameIdentifier, "0"),
                    new(ClaimTypes.Name, "Administrator"),
                    new(ClaimTypes.Email, _configuration["DefaultAdmin:Email"] ?? email),
                    new(ClaimTypes.Role, "Admin")
                };
                var adminIdentity = new ClaimsIdentity(adminClaims, "CookieAuth");
                return new ClaimsPrincipal(adminIdentity);
            }

            return null; // Email trùng admin nhưng mật khẩu sai
        }

        // 2. Staff & Lecturer Verification (từ Database qua ISystemAccountRepository)
        var account = await _accountRepository.GetByEmailAsync(normalizedEmail, ct);
        if (account == null || account.IsDeleted == true)
        {
            return null;
        }

        var verifyResult = _passwordHasher.VerifyHashedPassword(account, account.AccountPasswordHash, password);
        if (verifyResult == PasswordVerificationResult.Success || verifyResult == PasswordVerificationResult.SuccessRehashNeeded)
        {
            var roleName = account.AccountRole == 1 ? "Staff" : "Lecturer";
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, account.AccountId.ToString()),
                new(ClaimTypes.Name, account.AccountName),
                new(ClaimTypes.Email, account.AccountEmail),
                new(ClaimTypes.Role, roleName),
                new(ClaimTypes.Role, account.AccountRole.ToString()),
                new("AccountRole", account.AccountRole.ToString())
            };
            var identity = new ClaimsIdentity(claims, "CookieAuth");
            return new ClaimsPrincipal(identity);
        }

        return null;
    }
}
