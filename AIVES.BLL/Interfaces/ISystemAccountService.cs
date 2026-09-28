using AIVES.DAL.Entities;
using AIVES.DAL.Repositories.Models;

namespace AIVES.BLL.Interfaces;

public interface ISystemAccountService
{
    // --- 1. Admin Account Management (TV5 - ACC-01, ACC-02) ---
    Task<List<SystemAccount>> SearchAccountsAsync(string? keyword = null, CancellationToken ct = default);
    Task<SystemAccount?> GetAccountByIdAsync(int id, bool includeDeleted = false, CancellationToken ct = default);

    // --- 2. Admin Account Create & Update (TV5 - ACC-03, ACC-04) ---
    Task<int> CreateAccountAsync(string name, string email, string password, byte role, CancellationToken ct = default);
    Task<bool> UpdateAccountAsync(int id, string name, string email, byte role, string? newPassword = null, CancellationToken ct = default);

    // --- 3. Admin Account Soft Delete & Hard Delete (TV5 - ACC-05) ---
    Task<bool> SoftDeleteAccountAsync(int id, CancellationToken ct = default);
    Task<DeleteResult> HardDeleteAccountAsync(int id, CancellationToken ct = default);
    Task<bool> IsAccountReferencedAsync(int id, CancellationToken ct = default);

    // --- 4. Hợp đồng dùng chung cho TV4 (PROFILE-01 - Staff tự cập nhật thông tin) ---
    Task<bool> UpdateProfileAsync(int id, string name, string email, string? newPassword = null, CancellationToken ct = default);
}
