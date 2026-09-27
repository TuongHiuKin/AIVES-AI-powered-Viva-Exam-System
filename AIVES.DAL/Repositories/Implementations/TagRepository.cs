using AIVES.DAL.Context;
using AIVES.DAL.DAOs;
using AIVES.DAL.Entities;
using AIVES.DAL.Repositories.Interfaces;
using AIVES.DAL.Repositories.Models;

namespace AIVES.DAL.Repositories.Implementations;

public sealed class TagRepository(AIVESDbContext context) : ITagRepository
{
    public Task<List<Tag>> GetAllAsync(CancellationToken ct = default) =>
        TagDAO.Instance.GetAllAsync(context, ct);

    public Task<Tag?> GetByIdAsync(int id, CancellationToken ct = default) =>
        TagDAO.Instance.GetByIdAsync(context, id, ct);

    public Task<List<Tag>> GetByIdsAsync(IReadOnlyCollection<int> ids, CancellationToken ct = default) =>
        TagDAO.Instance.GetByIdsAsync(context, ids, ct);

    public Task<bool> IsUsedAsync(int id, CancellationToken ct = default) =>
        TagDAO.Instance.IsUsedAsync(context, id, ct);

    public Task<DeleteResult> DeleteAsync(int id, CancellationToken ct = default) =>
        TagDAO.Instance.DeleteAsync(context, id, ct);
}
