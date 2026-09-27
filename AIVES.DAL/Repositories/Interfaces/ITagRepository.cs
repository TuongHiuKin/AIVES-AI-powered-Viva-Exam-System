using AIVES.DAL.Entities;
using AIVES.DAL.Repositories.Models;

namespace AIVES.DAL.Repositories.Interfaces;

public interface ITagRepository
{
    Task<List<Tag>> GetAllAsync(CancellationToken ct = default);
    Task<Tag?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<List<Tag>> GetByIdsAsync(IReadOnlyCollection<int> ids, CancellationToken ct = default);
    Task<bool> IsUsedAsync(int id, CancellationToken ct = default);
    Task<DeleteResult> DeleteAsync(int id, CancellationToken ct = default);
}
