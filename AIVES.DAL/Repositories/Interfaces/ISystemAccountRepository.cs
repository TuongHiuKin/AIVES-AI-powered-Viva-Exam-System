using AIVES.DAL.Entities;
using AIVES.DAL.Repositories.Models;

namespace AIVES.DAL.Repositories.Interfaces;

public interface ISystemAccountRepository
{
    Task<List<SystemAccount>> SearchAsync(
        string? keyword = null,
        byte? role = null,
        bool includeDeleted = false,
        CancellationToken ct = default);
    Task<SystemAccount?> GetByIdAsync(int id, bool includeDeleted = false, CancellationToken ct = default);
    Task<SystemAccount?> GetByEmailAsync(string email, CancellationToken ct = default);
    // Includes deleted accounts: soft delete does not release an email address.
    Task<bool> EmailExistsAsync(string email, int? exceptId = null, CancellationToken ct = default);
    Task<bool> IsReferencedAsync(int id, CancellationToken ct = default);
    Task<int> CreateAsync(AccountCreate input, CancellationToken ct = default);
    Task<bool> UpdateAsync(AccountUpdate input, CancellationToken ct = default);
    Task<bool> UpdateProfileAsync(ProfileUpdate input, CancellationToken ct = default);
    Task<bool> SoftDeleteAsync(int id, CancellationToken ct = default);
    Task<DeleteResult> HardDeleteAsync(int id, CancellationToken ct = default);
}
