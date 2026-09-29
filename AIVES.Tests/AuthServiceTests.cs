using System.Security.Claims;
using AIVES.BLL.Services;
using AIVES.DAL.Entities;
using AIVES.DAL.Repositories.Interfaces;
using AIVES.DAL.Repositories.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace AIVES.Tests;

public class AuthServiceTests
{
    private class FakeSystemAccountRepository : ISystemAccountRepository
    {
        public List<SystemAccount> Accounts { get; } = new();

        public Task<List<SystemAccount>> SearchAsync(string? keyword = null, CancellationToken ct = default)
            => Task.FromResult(Accounts.ToList());

        public Task<SystemAccount?> GetByIdAsync(int id, bool includeDeleted = false, CancellationToken ct = default)
            => Task.FromResult(Accounts.FirstOrDefault(a => a.AccountId == id && (includeDeleted || a.IsDeleted != true)));

        public Task<SystemAccount?> GetByEmailAsync(string email, CancellationToken ct = default)
            => Task.FromResult(Accounts.FirstOrDefault(a => a.AccountEmail.Equals(email, StringComparison.OrdinalIgnoreCase)));

        public Task<bool> EmailExistsAsync(string email, int? exceptId = null, CancellationToken ct = default)
            => Task.FromResult(Accounts.Any(a => a.AccountEmail.Equals(email, StringComparison.OrdinalIgnoreCase) && a.AccountId != exceptId));

        public Task<bool> IsReferencedAsync(int id, CancellationToken ct = default) => Task.FromResult(false);
        public Task<int> CreateAsync(AccountCreate input, CancellationToken ct = default) => Task.FromResult(1);
        public Task<bool> UpdateAsync(AccountUpdate input, CancellationToken ct = default) => Task.FromResult(true);
        public Task<bool> UpdateProfileAsync(ProfileUpdate input, CancellationToken ct = default) => Task.FromResult(true);
        public Task<bool> SoftDeleteAsync(int id, CancellationToken ct = default) => Task.FromResult(true);
        public Task<DeleteResult> HardDeleteAsync(int id, CancellationToken ct = default) => Task.FromResult(DeleteResult.Deleted);
    }

    private readonly FakeSystemAccountRepository _accountRepo;
    private readonly IConfiguration _config;
    private readonly AuthService _service;
    private readonly PasswordHasher<SystemAccount> _hasher;

    public AuthServiceTests()
    {
        _accountRepo = new FakeSystemAccountRepository();
        _hasher = new PasswordHasher<SystemAccount>();

        var inMemoryConfig = new Dictionary<string, string?>
        {
            { "DefaultAdmin:Email", "admin@AIVESSystem.org" },
            { "DefaultAdmin:Password", "@@abc123@@" }
        };
        _config = new ConfigurationBuilder().AddInMemoryCollection(inMemoryConfig).Build();
        _service = new AuthService(_accountRepo, _config);

        // Seed a staff account and a lecturer account
        var staffAccount = new SystemAccount
        {
            AccountId = 1,
            AccountName = "Nguyễn Văn Staff",
            AccountEmail = "staff@aives.edu.vn",
            AccountRole = 1,
            IsDeleted = false
        };
        staffAccount.AccountPasswordHash = _hasher.HashPassword(staffAccount, "Staff123!");
        _accountRepo.Accounts.Add(staffAccount);

        var lecturerAccount = new SystemAccount
        {
            AccountId = 2,
            AccountName = "Trần Thị Lecturer",
            AccountEmail = "lecturer@aives.edu.vn",
            AccountRole = 2,
            IsDeleted = false
        };
        lecturerAccount.AccountPasswordHash = _hasher.HashPassword(lecturerAccount, "Lecturer123!");
        _accountRepo.Accounts.Add(lecturerAccount);

        var deletedAccount = new SystemAccount
        {
            AccountId = 3,
            AccountName = "Lê Đã Xóa",
            AccountEmail = "deleted@aives.edu.vn",
            AccountRole = 1,
            IsDeleted = true
        };
        deletedAccount.AccountPasswordHash = _hasher.HashPassword(deletedAccount, "Pass123!");
        _accountRepo.Accounts.Add(deletedAccount);
    }

    [Fact]
    public async Task AuthenticateAsync_WithValidAdminCredentials_ReturnsAdminPrincipal()
    {
        // Act
        var principal = await _service.AuthenticateAsync("admin@AIVESSystem.org", "@@abc123@@");

        // Assert
        Assert.NotNull(principal);
        Assert.True(principal.IsInRole("Admin"));
        Assert.Equal("0", principal.FindFirstValue(ClaimTypes.NameIdentifier));
    }

    [Fact]
    public async Task AuthenticateAsync_WithAdminEmailWrongPassword_ReturnsNull()
    {
        // Act
        var principal = await _service.AuthenticateAsync("admin@AIVESSystem.org", "wrong-password");

        // Assert
        Assert.Null(principal);
    }

    [Fact]
    public async Task AuthenticateAsync_WithValidStaffCredentials_ReturnsStaffPrincipal()
    {
        // Act
        var principal = await _service.AuthenticateAsync(" staff@aives.edu.vn ", "Staff123!");

        // Assert
        Assert.NotNull(principal);
        Assert.True(principal.IsInRole("Staff"));
        Assert.True(principal.IsInRole("1"));
        Assert.Equal("1", principal.FindFirstValue(ClaimTypes.NameIdentifier));
    }

    [Fact]
    public async Task AuthenticateAsync_WithValidLecturerCredentials_ReturnsLecturerPrincipal()
    {
        // Act
        var principal = await _service.AuthenticateAsync("lecturer@aives.edu.vn", "Lecturer123!");

        // Assert
        Assert.NotNull(principal);
        Assert.True(principal.IsInRole("Lecturer"));
        Assert.True(principal.IsInRole("2"));
        Assert.Equal("2", principal.FindFirstValue(ClaimTypes.NameIdentifier));
    }

    [Fact]
    public async Task AuthenticateAsync_WithSoftDeletedAccount_ReturnsNull()
    {
        // Act: Tài khoản đã bị đánh dấu IsDeleted = true
        var principal = await _service.AuthenticateAsync("deleted@aives.edu.vn", "Pass123!");

        // Assert
        Assert.Null(principal);
    }

    [Fact]
    public async Task AuthenticateAsync_WithNonExistentEmail_ReturnsNull()
    {
        // Act
        var principal = await _service.AuthenticateAsync("unknown@aives.edu.vn", "any-password");

        // Assert
        Assert.Null(principal);
    }

    [Theory]
    [InlineData("", "password")]
    [InlineData("email@test.com", "")]
    [InlineData(null, null)]
    public async Task AuthenticateAsync_WithEmptyCredentials_ReturnsNull(string? email, string? password)
    {
        // Act
        var principal = await _service.AuthenticateAsync(email!, password!);

        // Assert
        Assert.Null(principal);
    }
}
