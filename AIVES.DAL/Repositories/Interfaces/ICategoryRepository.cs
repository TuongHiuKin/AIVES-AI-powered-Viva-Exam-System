using AIVES.DAL.Entities;
using AIVES.DAL.Repositories.Models;

namespace AIVES.DAL.Repositories.Interfaces;

public interface ICategoryRepository
{
    Task<List<Category>> SearchAsync(string? keyword = null, CancellationToken ct = default);
    Task<Category?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<bool> HasNewsAsync(int id, CancellationToken ct = default);
    Task<int> CreateAsync(string name, string? description, CancellationToken ct = default);
    Task<bool> UpdateAsync(int id, string name, string? description, CancellationToken ct = default);
    Task<DeleteResult> DeleteAsync(int id, CancellationToken ct = default);
}
