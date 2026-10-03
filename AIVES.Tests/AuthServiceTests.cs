using AIVES.BLL.Options;
using AIVES.BLL.Security;
using AIVES.BLL.Services;
using AIVES.DAL.Entities;
using AIVES.DAL.Repositories.Interfaces;
using AIVES.DAL.Repositories.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.DataProtection;

namespace AIVES.Tests;

public sealed class AuthServiceTests
{
    private const string AdminEmail = "admin@aives.test";
    private const string AdminPassword = "Admin-password-123";

    [Fact]
    public async Task Default_admin_authenticates_without_database_lookup()
    {
        var repository = new StubAccountRepository();
        var service = CreateService(repository);

        var result = await service.AuthenticateAsync(
            "  ADMIN@AIVES.TEST ",
            AdminPassword);

        Assert.True(result.Succeeded);
        Assert.Equal(ApplicationRoles.AdminSubjectId, result.User!.SubjectId);
        Assert.Equal(ApplicationRoles.Admin, result.User.Role);
        Assert.Equal(0, repository.EmailLookupCount);
    }

    [Theory]
    [InlineData((byte)1, ApplicationRoles.Staff)]
    [InlineData((byte)2, ApplicationRoles.Lecturer)]
    public async Task Database_account_uses_normalized_email_and_password_hash(
        byte databaseRole,
        string expectedRole)
    {
        var hasher = new PasswordHasher<SystemAccount>();
        var account = new SystemAccount
        {
            AccountId = 42,
            AccountName = "Member",
            AccountEmail = "member@example.test",
            AccountRole = databaseRole
        };
        account.AccountPasswordHash = hasher.HashPassword(account, "Correct-password-123");

        var repository = new StubAccountRepository { EmailAccount = account };
        var service = CreateService(repository, hasher);

        var result = await service.AuthenticateAsync(
            "  MEMBER@EXAMPLE.TEST ",
            "Correct-password-123");

        Assert.True(result.Succeeded);
        Assert.Equal("member@example.test", repository.LastEmail);
        Assert.Equal("42", result.User!.SubjectId);
        Assert.Equal(expectedRole, result.User.Role);
    }

    [Fact]
    public async Task Wrong_password_deleted_account_and_unknown_role_are_rejected()
    {
        var hasher = new PasswordHasher<SystemAccount>();
        var account = new SystemAccount
        {
            AccountId = 7,
            AccountName = "Member",
            AccountEmail = "member@example.test",
            AccountRole = 1
        };
        account.AccountPasswordHash = hasher.HashPassword(account, "Correct-password-123");

        var repository = new StubAccountRepository { EmailAccount = account };
        var service = CreateService(repository, hasher);

        Assert.False((await service.AuthenticateAsync(account.AccountEmail, "wrong")).Succeeded);

        account.IsDeleted = true;
        Assert.False((await service.AuthenticateAsync(account.AccountEmail, "Correct-password-123")).Succeeded);

        account.IsDeleted = false;
        account.AccountRole = 99;
        Assert.False((await service.AuthenticateAsync(account.AccountEmail, "Correct-password-123")).Succeeded);
    }

    [Fact]
    public async Task Account_activity_uses_non_deleted_repository_lookup()
    {
        var repository = new StubAccountRepository
        {
            IdAccount = new SystemAccount { AccountId = 12, IsDeleted = false }
        };
        var service = CreateService(repository);

        Assert.True(await service.IsAccountActiveAsync(12));
        Assert.False(repository.LastIncludeDeleted);

        repository.IdAccount = null;
        Assert.False(await service.IsAccountActiveAsync(12));
        Assert.False(await service.IsAccountActiveAsync(0));
    }

    private static AuthService CreateService(
        StubAccountRepository repository,
        IPasswordHasher<SystemAccount>? hasher = null,
        TimeProvider? clock = null) => new(
            repository,
            hasher ?? new PasswordHasher<SystemAccount>(),
            Options.Create(new DefaultAdminOptions
            {
                Email = AdminEmail,
                Password = AdminPassword
            }),
            new EphemeralDataProtectionProvider(),
            clock ?? TimeProvider.System);

    [Fact]
    public async Task Reset_token_is_single_use_and_new_password_authenticates()
    {
        var account = new SystemAccount { AccountId = 1, AccountEmail = "member@example.test", AccountRole = 1 };
        var hasher = new PasswordHasher<SystemAccount>();
        account.AccountPasswordHash = hasher.HashPassword(account, "Old-password-123");
        var service = CreateService(new StubAccountRepository { EmailAccount = account, IdAccount = account }, hasher);
        var token = await service.CreatePasswordResetTokenAsync(" MEMBER@EXAMPLE.TEST ");
        Assert.NotNull(token);
        Assert.DoesNotContain(account.AccountEmail, token);
        Assert.True(await service.ResetPasswordAsync(account.AccountEmail, token, "New-password-123"));
        Assert.False(await service.ResetPasswordAsync(account.AccountEmail, token, "Other-password-123"));
        Assert.False((await service.AuthenticateAsync(account.AccountEmail, "Old-password-123")).Succeeded);
        Assert.True((await service.AuthenticateAsync(account.AccountEmail, "New-password-123")).Succeeded);
    }

    [Fact]
    public async Task Reset_rejects_wrong_email_tampering_expiry_and_locked_account()
    {
        var account = new SystemAccount { AccountId = 1, AccountEmail = "member@example.test", AccountRole = 1, AccountPasswordHash = "old-hash" };
        var clock = new TestClock();
        var service = CreateService(new StubAccountRepository { EmailAccount = account, IdAccount = account }, clock: clock);
        var token = (await service.CreatePasswordResetTokenAsync(account.AccountEmail))!;
        Assert.False(await service.ResetPasswordAsync("other@example.test", token, "New-password-123"));
        Assert.False(await service.ResetPasswordAsync(account.AccountEmail, "invalid-token", "New-password-123"));
        Assert.False(await service.ResetPasswordAsync(account.AccountEmail, token, "short"));
        account.IsDeleted = true;
        Assert.Null(await service.CreatePasswordResetTokenAsync(account.AccountEmail));
        Assert.False(await service.ResetPasswordAsync(account.AccountEmail, token, "New-password-123"));
        account.IsDeleted = false;
        clock.Now = clock.Now.AddMinutes(20);
        Assert.False(await service.ResetPasswordAsync(account.AccountEmail, token, "New-password-123"));
        Assert.Equal("old-hash", account.AccountPasswordHash);
        Assert.Null(await service.CreatePasswordResetTokenAsync(AdminEmail));
        Assert.Null(await CreateService(new StubAccountRepository()).CreatePasswordResetTokenAsync("missing@example.test"));
    }

    private sealed class TestClock : TimeProvider
    {
        public DateTimeOffset Now { get; set; } = DateTimeOffset.UtcNow;
        public override DateTimeOffset GetUtcNow() => Now;
    }

    private sealed class StubAccountRepository : ISystemAccountRepository
    {
        public Task<bool> ResetPasswordAsync(int id, string email, string expectedHash, string newHash, CancellationToken ct = default)
        {
            var account = IdAccount;
            if (account is null || account.IsDeleted || account.AccountId != id ||
                account.AccountEmail != email || account.AccountPasswordHash != expectedHash) return Task.FromResult(false);
            account.AccountPasswordHash = newHash;
            return Task.FromResult(true);
        }

        public SystemAccount? EmailAccount { get; set; }
        public SystemAccount? IdAccount { get; set; }
        public int EmailLookupCount { get; private set; }
        public string? LastEmail { get; private set; }
        public bool LastIncludeDeleted { get; private set; }

        public Task<SystemAccount?> GetByEmailAsync(string email, CancellationToken ct = default)
        {
            EmailLookupCount++;
            LastEmail = email;
            return Task.FromResult(EmailAccount);
        }

        public Task<SystemAccount?> GetByIdAsync(
            int id,
            bool includeDeleted = false,
            CancellationToken ct = default)
        {
            LastIncludeDeleted = includeDeleted;
            return Task.FromResult(IdAccount?.AccountId == id ? IdAccount : null);
        }

        public Task<List<SystemAccount>> SearchAsync(string? keyword = null, CancellationToken ct = default) =>
            throw new NotSupportedException();
        public Task<bool> EmailExistsAsync(string email, int? exceptId = null, CancellationToken ct = default) =>
            throw new NotSupportedException();
        public Task<bool> IsReferencedAsync(int id, CancellationToken ct = default) =>
            throw new NotSupportedException();
        public Task<int> CreateAsync(AccountCreate input, CancellationToken ct = default) =>
            throw new NotSupportedException();
        public Task<bool> UpdateAsync(AccountUpdate input, CancellationToken ct = default) =>
            throw new NotSupportedException();
        public Task<bool> UpdateProfileAsync(ProfileUpdate input, CancellationToken ct = default) =>
            throw new NotSupportedException();
        public Task<bool> SoftDeleteAsync(int id, CancellationToken ct = default) =>
            throw new NotSupportedException();
        public Task<DeleteResult> HardDeleteAsync(int id, CancellationToken ct = default) =>
            throw new NotSupportedException();
    }
}
