using AIVES.DAL.Context;
using AIVES.DAL.DAOs;
using AIVES.DAL.Entities;
using AIVES.DAL.Repositories.Interfaces;
using AIVES.DAL.Repositories.Models;

namespace AIVES.DAL.Repositories.Implementations;

public sealed class SystemAccountRepository(AIVESDbContext context) : ISystemAccountRepository
{
    public Task<bool> ResetPasswordAsync(int id, string email, string expectedHash, string newHash, CancellationToken ct = default) =>
        SystemAccountDAO.Instance.ResetPasswordAsync(context, id, email, expectedHash, newHash, ct);

    public Task<List<SystemAccount>> SearchAsync(string? keyword = null, CancellationToken ct = default) =>
        SystemAccountDAO.Instance.SearchAsync(context, keyword, ct);

    public Task<SystemAccount?> GetByIdAsync(int id, bool includeDeleted = false, CancellationToken ct = default) =>
        SystemAccountDAO.Instance.GetByIdAsync(context, id, includeDeleted, ct);

    public Task<SystemAccount?> GetByEmailAsync(string email, CancellationToken ct = default) =>
        SystemAccountDAO.Instance.GetByEmailAsync(context, email, ct);

    public Task<bool> EmailExistsAsync(string email, int? exceptId = null, CancellationToken ct = default) =>
        SystemAccountDAO.Instance.EmailExistsAsync(context, email, exceptId, ct);

    public Task<bool> IsReferencedAsync(int id, CancellationToken ct = default) =>
        SystemAccountDAO.Instance.IsReferencedAsync(context, id, ct);

    public Task<int> CreateAsync(AccountCreate input, CancellationToken ct = default) =>
        SystemAccountDAO.Instance.CreateAsync(context, input, ct);

    public Task<bool> UpdateAsync(AccountUpdate input, CancellationToken ct = default) =>
        SystemAccountDAO.Instance.UpdateAsync(context, input, ct);

    public Task<bool> UpdateProfileAsync(ProfileUpdate input, CancellationToken ct = default) =>
        SystemAccountDAO.Instance.UpdateProfileAsync(context, input, ct);

    public Task<bool> SoftDeleteAsync(int id, CancellationToken ct = default) =>
        SystemAccountDAO.Instance.SoftDeleteAsync(context, id, ct);

    public Task<DeleteResult> HardDeleteAsync(int id, CancellationToken ct = default) =>
        SystemAccountDAO.Instance.HardDeleteAsync(context, id, ct);
}
