using AIVES.BLL.Services;
using AIVES.DAL.Entities;
using AIVES.DAL.Repositories.Interfaces;
using AIVES.DAL.Repositories.Models;
using Xunit;

namespace AIVES.Tests;

public class SystemAccountServiceTests
{
    private class FakeSystemAccountRepository : ISystemAccountRepository
    {
        public List<SystemAccount> Accounts { get; } = new();
        public HashSet<int> ReferencedIds { get; } = new();

        public Task<bool> ResetPasswordAsync(int id, string email, string expectedHash, string newHash, CancellationToken ct = default)
        {
            var account = Accounts.FirstOrDefault(x => x.AccountId == id && !x.IsDeleted &&
                x.AccountEmail == email && x.AccountPasswordHash == expectedHash);
            if (account is null) return Task.FromResult(false);
            account.AccountPasswordHash = newHash;
            return Task.FromResult(true);
        }

        public Task<List<SystemAccount>> SearchAsync(string? keyword = null, CancellationToken ct = default)
        {
            var query = Accounts.Where(x => !x.IsDeleted);
            if (!string.IsNullOrEmpty(keyword))
            {
                var term = keyword.Trim().ToLowerInvariant();
                query = query.Where(x => x.AccountName.ToLowerInvariant().Contains(term) || x.AccountEmail.Contains(term));
            }
            return Task.FromResult(query.ToList());
        }

        public Task<SystemAccount?> GetByIdAsync(int id, bool includeDeleted = false, CancellationToken ct = default)
        {
            var account = Accounts.FirstOrDefault(x => x.AccountId == id && (includeDeleted || !x.IsDeleted));
            return Task.FromResult(account);
        }

        public Task<SystemAccount?> GetByEmailAsync(string email, CancellationToken ct = default)
        {
            var normalized = email.Trim().ToLowerInvariant();
            var account = Accounts.FirstOrDefault(x => !x.IsDeleted && x.AccountEmail == normalized);
            return Task.FromResult(account);
        }

        public Task<bool> EmailExistsAsync(string email, int? exceptId = null, CancellationToken ct = default)
        {
            var normalized = email.Trim().ToLowerInvariant();
            var exists = Accounts.Any(x => x.AccountEmail == normalized && (!exceptId.HasValue || x.AccountId != exceptId.Value));
            return Task.FromResult(exists);
        }

        public Task<bool> IsReferencedAsync(int id, CancellationToken ct = default)
        {
            return Task.FromResult(ReferencedIds.Contains(id));
        }

        public Task<int> CreateAsync(AccountCreate input, CancellationToken ct = default)
        {
            var newId = Accounts.Count > 0 ? Accounts.Max(x => x.AccountId) + 1 : 1;
            var account = new SystemAccount
            {
                AccountId = newId,
                AccountName = input.Name,
                AccountEmail = input.Email,
                AccountPasswordHash = input.PasswordHash,
                AccountRole = input.Role,
                IsDeleted = false
            };
            Accounts.Add(account);
            return Task.FromResult(newId);
        }

        public Task<bool> UpdateAsync(AccountUpdate input, CancellationToken ct = default)
        {
            var account = Accounts.FirstOrDefault(x => x.AccountId == input.Id && !x.IsDeleted);
            if (account == null) return Task.FromResult(false);

            account.AccountName = input.Name;
            account.AccountEmail = input.Email;
            account.AccountRole = input.Role;
            if (input.NewPasswordHash != null)
                account.AccountPasswordHash = input.NewPasswordHash;

            return Task.FromResult(true);
        }

        public Task<bool> UpdateProfileAsync(ProfileUpdate input, CancellationToken ct = default)
        {
            var account = Accounts.FirstOrDefault(x => x.AccountId == input.Id && !x.IsDeleted);
            if (account == null) return Task.FromResult(false);

            account.AccountName = input.Name;
            account.AccountEmail = input.Email;
            if (input.NewPasswordHash != null)
                account.AccountPasswordHash = input.NewPasswordHash;

            return Task.FromResult(true);
        }

        public Task<bool> SoftDeleteAsync(int id, CancellationToken ct = default)
        {
            var account = Accounts.FirstOrDefault(x => x.AccountId == id);
            if (account == null) return Task.FromResult(false);
            account.IsDeleted = true;
            return Task.FromResult(true);
        }

        public Task<DeleteResult> HardDeleteAsync(int id, CancellationToken ct = default)
        {
            var account = Accounts.FirstOrDefault(x => x.AccountId == id);
            if (account == null) return Task.FromResult(DeleteResult.NotFound);
            if (ReferencedIds.Contains(id)) return Task.FromResult(DeleteResult.InUse);

            Accounts.Remove(account);
            return Task.FromResult(DeleteResult.Deleted);
        }
    }

    [Fact]
    public async Task CreateAccountAsync_WithValidStaffData_SuccessfullyCreatesAccount()
    {
        var repo = new FakeSystemAccountRepository();
        var service = new SystemAccountService(repo);

        var newId = await service.CreateAccountAsync("Nguyen Van A", "staff1@aives.test", "Password123!", 1);

        Assert.Equal(1, newId);
        var created = await repo.GetByIdAsync(1);
        Assert.NotNull(created);
        Assert.Equal("Nguyen Van A", created.AccountName);
        Assert.Equal("staff1@aives.test", created.AccountEmail);
        Assert.Equal((byte)1, created.AccountRole);
        Assert.NotEmpty(created.AccountPasswordHash);
        Assert.NotEqual("Password123!", created.AccountPasswordHash); // Password must be hashed!
    }

    [Fact]
    public async Task CreateAccountAsync_WithDuplicateEmail_ThrowsInvalidOperationException()
    {
        var repo = new FakeSystemAccountRepository();
        repo.Accounts.Add(new SystemAccount
        {
            AccountId = 1,
            AccountName = "Existing User",
            AccountEmail = "duplicate@aives.test",
            AccountRole = 1
        });
        var service = new SystemAccountService(repo);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateAccountAsync("Another User", "DUPLICATE@AIVES.TEST ", "Password123!", 1));

        Assert.Contains("email", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(3)]
    public async Task CreateAccountAsync_WithInvalidRole_ThrowsArgumentException(byte invalidRole)
    {
        var repo = new FakeSystemAccountRepository();
        var service = new SystemAccountService(repo);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.CreateAccountAsync("User", "user@aives.test", "Password123!", invalidRole));
    }

    [Fact]
    public async Task CreateAccountAsync_WithShortPassword_ThrowsArgumentException()
    {
        var repo = new FakeSystemAccountRepository();
        var service = new SystemAccountService(repo);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.CreateAccountAsync("User", "user@aives.test", "12345", 1));
    }

    [Fact]
    public async Task UpdateAccountAsync_WithNewPassword_HashesNewPassword()
    {
        var repo = new FakeSystemAccountRepository();
        repo.Accounts.Add(new SystemAccount
        {
            AccountId = 10,
            AccountName = "Old Name",
            AccountEmail = "old@aives.test",
            AccountPasswordHash = "OriginalHash",
            AccountRole = 1
        });
        var service = new SystemAccountService(repo);

        var success = await service.UpdateAccountAsync(10, "New Name", "updated@aives.test", 2, "NewSecretPassword");

        Assert.True(success);
        var updated = await repo.GetByIdAsync(10);
        Assert.NotNull(updated);
        Assert.Equal("New Name", updated.AccountName);
        Assert.Equal("updated@aives.test", updated.AccountEmail);
        Assert.Equal((byte)2, updated.AccountRole);
        Assert.NotEqual("OriginalHash", updated.AccountPasswordHash);
        Assert.NotEqual("NewSecretPassword", updated.AccountPasswordHash);
    }

    [Fact]
    public async Task UpdateAccountAsync_WithDuplicateEmailOnAnotherAccount_ThrowsInvalidOperationException()
    {
        var repo = new FakeSystemAccountRepository();
        repo.Accounts.Add(new SystemAccount { AccountId = 1, AccountEmail = "first@aives.test", AccountName = "First", AccountRole = 1 });
        repo.Accounts.Add(new SystemAccount { AccountId = 2, AccountEmail = "second@aives.test", AccountName = "Second", AccountRole = 2 });
        var service = new SystemAccountService(repo);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.UpdateAccountAsync(2, "Second", "first@aives.test", 2));
    }

    [Fact]
    public async Task SoftDeleteAccountAsync_WhenAccountExists_ReturnsTrue()
    {
        var repo = new FakeSystemAccountRepository();
        repo.Accounts.Add(new SystemAccount { AccountId = 5, AccountName = "User 5", AccountEmail = "u5@aives.test", IsDeleted = false });
        var service = new SystemAccountService(repo);

        var result = await service.SoftDeleteAccountAsync(5);

        Assert.True(result);
        var account = await repo.GetByIdAsync(5, includeDeleted: true);
        Assert.NotNull(account);
        Assert.True(account.IsDeleted);
    }

    [Fact]
    public async Task HardDeleteAccountAsync_WhenAccountIsReferenced_ReturnsInUse()
    {
        var repo = new FakeSystemAccountRepository();
        repo.Accounts.Add(new SystemAccount { AccountId = 8, AccountName = "Author", AccountEmail = "author@aives.test" });
        repo.ReferencedIds.Add(8); // Account is an author of news
        var service = new SystemAccountService(repo);

        var result = await service.HardDeleteAccountAsync(8);

        Assert.Equal(DeleteResult.InUse, result);
        Assert.Single(repo.Accounts); // Account was not removed!
    }

    [Fact]
    public async Task HardDeleteAccountAsync_WhenAccountNotReferenced_ReturnsDeleted()
    {
        var repo = new FakeSystemAccountRepository();
        repo.Accounts.Add(new SystemAccount { AccountId = 9, AccountName = "Clean Account", AccountEmail = "clean@aives.test" });
        var service = new SystemAccountService(repo);

        var result = await service.HardDeleteAccountAsync(9);

        Assert.Equal(DeleteResult.Deleted, result);
        Assert.Empty(repo.Accounts);
    }
}
