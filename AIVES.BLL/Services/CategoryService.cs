using AIVES.BLL.Interfaces;
using AIVES.DAL.Entities;
using AIVES.DAL.Repositories.Interfaces;
using AIVES.DAL.Repositories.Models;

namespace AIVES.BLL.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepo;

    public CategoryService(ICategoryRepository categoryRepo)
    {
        _categoryRepo = categoryRepo ?? throw new ArgumentNullException(nameof(categoryRepo));
    }

    public async Task<int> CreateCategoriesAsync(string name, string? description = null, CancellationToken ct = default)
    {
        // 1. Xác định các input
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Category title cannot be empty.", nameof(name));
        if (description != null)
            if (description.Trim().Length > 500)
                throw new ArgumentException("Category description cannot exceed 500 characters.", nameof(name));

        // 2. Chuyển qua cho Repo thực thi
        return await _categoryRepo.CreateAsync(name, description, ct);
    }

    public async Task<DeleteResult> DeleteCategoriesAsync(int id, CancellationToken ct = default)
    {
        return await _categoryRepo.DeleteAsync(id, ct);
    }

    public async Task<Category?> GetCategoriesAsync(int id, CancellationToken ct = default)
    {
        if (id <= 0) return null;
        return await _categoryRepo.GetByIdAsync(id, ct);
    }

    public async Task<List<Category>> SearchCategoriesAsync(string? keyword = null, CancellationToken ct = default)
    {
        var trimmed = string.IsNullOrWhiteSpace(keyword) ? null : keyword.Trim();
        return await _categoryRepo.SearchAsync(trimmed, ct);
    }

    public async Task<bool> UpdateCategoriesAsync(int id, string name, string? description = null, CancellationToken ct = default)
    {
        if (id <= 0) return false;

        // 1. Lấy Category trong DB
        var exist = await _categoryRepo.GetByIdAsync(id, ct);
        if (exist == null) return false;

        // 2. Xác định các input
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Category title cannot be empty.", nameof(name));
        if (description != null)
            if (description.Trim().Length > 500)
                throw new ArgumentException("Category description cannot exceed 500 characters.", nameof(name));

        // 3. Chuyển qua cho Repo thực thi
        return await _categoryRepo.UpdateAsync(id, name, description, ct);
    }
}