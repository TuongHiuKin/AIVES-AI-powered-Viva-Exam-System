using AIVES.DAL.Context;
using AIVES.DAL.DAOs;
using AIVES.DAL.Entities;
using AIVES.DAL.Repositories.Interfaces;
using AIVES.DAL.Repositories.Models;

namespace AIVES.DAL.Repositories.Implementations;

public sealed class CategoryRepository(AIVESDbContext context) : ICategoryRepository
{
    public Task<List<Category>> SearchAsync(string? keyword = null, CancellationToken ct = default) =>
        CategoryDAO.Instance.SearchAsync(context, keyword, ct);

    public Task<Category?> GetByIdAsync(int id, CancellationToken ct = default) =>
        CategoryDAO.Instance.GetByIdAsync(context, id, ct);

    public Task<bool> HasNewsAsync(int id, CancellationToken ct = default) =>
        CategoryDAO.Instance.HasNewsAsync(context, id, ct);

    public Task<int> CreateAsync(string name, string? description, CancellationToken ct = default) =>
        CategoryDAO.Instance.CreateAsync(context, name, description, ct);

    public Task<bool> UpdateAsync(int id, string name, string? description, CancellationToken ct = default) =>
        CategoryDAO.Instance.UpdateAsync(context, id, name, description, ct);

    public Task<DeleteResult> DeleteAsync(int id, CancellationToken ct = default) =>
        CategoryDAO.Instance.DeleteAsync(context, id, ct);
}
