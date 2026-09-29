using AIVES.BLL.Interfaces;
using AIVES.DAL.Entities;
using AIVES.DAL.Repositories.Interfaces;
using AIVES.DAL.Repositories.Models;
using Microsoft.AspNetCore.Identity;

namespace AIVES.BLL.Services;

public class SystemAccountService : ISystemAccountService
{
    private readonly ISystemAccountRepository _accountRepo;
    private readonly PasswordHasher<SystemAccount> _passwordHasher;

    public SystemAccountService(ISystemAccountRepository accountRepo)
    {
        _accountRepo = accountRepo ?? throw new ArgumentNullException(nameof(accountRepo));
        _passwordHasher = new PasswordHasher<SystemAccount>();
    }

    private static string NormalizeEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email không được để trống.", nameof(email));
        return email.Trim().ToLowerInvariant();
    }

    public async Task<List<SystemAccount>> SearchAccountsAsync(string? keyword = null, CancellationToken ct = default)
    {
        var trimmed = string.IsNullOrWhiteSpace(keyword) ? null : keyword.Trim();
        return await _accountRepo.SearchAsync(trimmed, ct);
    }

    public async Task<SystemAccount?> GetAccountByIdAsync(int id, bool includeDeleted = false, CancellationToken ct = default)
    {
        if (id <= 0) return null;
        return await _accountRepo.GetByIdAsync(id, includeDeleted, ct);
    }

    public async Task<int> CreateAccountAsync(string name, string email, string password, byte role, CancellationToken ct = default)
    {
        // 1. Business validation
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Họ và tên không được để trống.", nameof(name));

        var trimmedName = name.Trim();
        if (trimmedName.Length > 100)
            throw new ArgumentException("Họ và tên không được vượt quá 100 ký tự.", nameof(name));

        var normalizedEmail = NormalizeEmail(email);
        if (normalizedEmail.Length > 254)
            throw new ArgumentException("Email không được vượt quá 254 ký tự.", nameof(email));

        if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
            throw new ArgumentException("Mật khẩu phải có ít nhất 6 ký tự.", nameof(password));

        if (role is not (1 or 2))
            throw new ArgumentException("Vai trò không hợp lệ. Chỉ chấp nhận Staff (1) hoặc Lecturer (2).", nameof(role));

        // 2. Check email uniqueness (kể cả các tài khoản đã soft-delete theo SC-10/BR-26)
        var exists = await _accountRepo.EmailExistsAsync(normalizedEmail, exceptId: null, ct);
        if (exists)
            throw new InvalidOperationException("Địa chỉ email này đã tồn tại trong hệ thống.");

        // 3. Hash password using ASP.NET Core Identity PasswordHasher
        var dummyAccount = new SystemAccount { AccountEmail = normalizedEmail };
        var passwordHash = _passwordHasher.HashPassword(dummyAccount, password);

        // 4. Delegate to repository
        var command = new AccountCreate(
            Name: trimmedName,
            Email: normalizedEmail,
            PasswordHash: passwordHash,
            Role: role
        );

        return await _accountRepo.CreateAsync(command, ct);
    }

    public async Task<bool> UpdateAccountAsync(int id, string name, string email, byte role, string? newPassword = null, CancellationToken ct = default)
    {
        if (id <= 0) return false;

        // 1. Check account exists
        var existing = await _accountRepo.GetByIdAsync(id, includeDeleted: false, ct);
        if (existing == null) return false;

        // 2. Business validation
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Họ và tên không được để trống.", nameof(name));

        var trimmedName = name.Trim();
        if (trimmedName.Length > 100)
            throw new ArgumentException("Họ và tên không được vượt quá 100 ký tự.", nameof(name));

        var normalizedEmail = NormalizeEmail(email);
        if (normalizedEmail.Length > 254)
            throw new ArgumentException("Email không được vượt quá 254 ký tự.", nameof(email));

        if (role is not (1 or 2))
            throw new ArgumentException("Vai trò không hợp lệ. Chỉ chấp nhận Staff (1) hoặc Lecturer (2).", nameof(role));

        // 3. Check email uniqueness excluding current account
        var emailExists = await _accountRepo.EmailExistsAsync(normalizedEmail, exceptId: id, ct);
        if (emailExists)
            throw new InvalidOperationException("Địa chỉ email này đã được sử dụng bởi một tài khoản khác.");

        // 4. Password handling (null = giữ nguyên mật khẩu cũ)
        string? newPasswordHash = null;
        if (!string.IsNullOrWhiteSpace(newPassword))
        {
            if (newPassword.Length < 6)
                throw new ArgumentException("Mật khẩu mới phải có ít nhất 6 ký tự.", nameof(newPassword));
            newPasswordHash = _passwordHasher.HashPassword(existing, newPassword);
        }

        // 5. Delegate to repository
        var command = new AccountUpdate(
            Id: id,
            Name: trimmedName,
            Email: normalizedEmail,
            Role: role,
            NewPasswordHash: newPasswordHash
        );

        return await _accountRepo.UpdateAsync(command, ct);
    }

    public async Task<bool> SoftDeleteAccountAsync(int id, CancellationToken ct = default)
    {
        if (id <= 0) return false;
        return await _accountRepo.SoftDeleteAsync(id, ct);
    }

    public async Task<DeleteResult> HardDeleteAccountAsync(int id, CancellationToken ct = default)
    {
        if (id <= 0) return DeleteResult.NotFound;

        // Check if account is referenced by any news articles
        var isReferenced = await _accountRepo.IsReferencedAsync(id, ct);
        if (isReferenced)
        {
            return DeleteResult.InUse;
        }

        return await _accountRepo.HardDeleteAsync(id, ct);
    }

    public async Task<bool> IsAccountReferencedAsync(int id, CancellationToken ct = default)
    {
        if (id <= 0) return false;
        return await _accountRepo.IsReferencedAsync(id, ct);
    }

    public async Task<bool> UpdateProfileAsync(int id, string name, string email, string? newPassword = null, CancellationToken ct = default)
    {
        if (id <= 0) return false;

        var existing = await _accountRepo.GetByIdAsync(id, includeDeleted: false, ct);
        if (existing == null) return false;

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Họ và tên không được để trống.", nameof(name));

        var trimmedName = name.Trim();
        var normalizedEmail = NormalizeEmail(email);

        var emailExists = await _accountRepo.EmailExistsAsync(normalizedEmail, exceptId: id, ct);
        if (emailExists)
            throw new InvalidOperationException("Địa chỉ email này đã được sử dụng bởi một tài khoản khác.");

        string? newPasswordHash = null;
        if (!string.IsNullOrWhiteSpace(newPassword))
        {
            if (newPassword.Length < 6)
                throw new ArgumentException("Mật khẩu mới phải có ít nhất 6 ký tự.", nameof(newPassword));
            newPasswordHash = _passwordHasher.HashPassword(existing, newPassword);
        }

        var command = new ProfileUpdate(
            Id: id,
            Name: trimmedName,
            Email: normalizedEmail,
            NewPasswordHash: newPasswordHash
        );

        return await _accountRepo.UpdateProfileAsync(command, ct);
    }

    public async Task<SystemAccount?> AuthenticateAsync(string email, string password, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return null;

        var normalizedEmail = NormalizeEmail(email);
        var account = await _accountRepo.GetByEmailAsync(normalizedEmail, ct);
        if (account == null || account.IsDeleted)
            return null;

        var verifyResult = _passwordHasher.VerifyHashedPassword(account, account.AccountPasswordHash, password);
        if (verifyResult == PasswordVerificationResult.Failed)
            return null;

        return account;
    }
}
