using AIVES.DAL.Context;
using AIVES.DAL.Entities;
using AIVES.DAL.Repositories.Models;
using Microsoft.EntityFrameworkCore;

namespace AIVES.DAL.DAOs;

public sealed class CategoryDAO
{
    private static readonly Lazy<CategoryDAO> Singleton = new(() => new CategoryDAO());
    public static CategoryDAO Instance => Singleton.Value;
    private CategoryDAO() { }

    public Task<List<Category>> SearchAsync(AIVESDbContext db, string? keyword, CancellationToken ct)
    {
        var query = db.Categories.AsNoTracking();
        var term = keyword?.Trim();
        if (!string.IsNullOrEmpty(term))
            query = query.Where(x => x.CategoryName.Contains(term) ||
                (x.CategoryDescription != null && x.CategoryDescription.Contains(term)));
        return query.OrderBy(x => x.CategoryName).ThenBy(x => x.CategoryId).ToListAsync(ct);
    }

    public Task<Category?> GetByIdAsync(AIVESDbContext db, int id, CancellationToken ct) =>
        db.Categories.AsNoTracking().SingleOrDefaultAsync(x => x.CategoryId == id, ct);

    public Task<bool> HasNewsAsync(AIVESDbContext db, int id, CancellationToken ct) =>
        db.NewsArticles.AnyAsync(x => x.CategoryId == id, ct);

    public async Task<int> CreateAsync(AIVESDbContext db, string name, string? description, CancellationToken ct)
    {
        var entity = new Category { CategoryName = name, CategoryDescription = description };
        db.Categories.Add(entity);
        await Persistence.SaveAsync(db, ct);
        return entity.CategoryId;
    }

    public async Task<bool> UpdateAsync(AIVESDbContext db, int id, string name, string? description, CancellationToken ct)
    {
        var entity = await db.Categories.SingleOrDefaultAsync(x => x.CategoryId == id, ct);
        if (entity is null) return false;
        entity.CategoryName = name;
        entity.CategoryDescription = description;
        await Persistence.SaveAsync(db, ct);
        return true;
    }

    public async Task<DeleteResult> DeleteAsync(AIVESDbContext db, int id, CancellationToken ct)
    {
        var entity = await db.Categories.SingleOrDefaultAsync(x => x.CategoryId == id, ct);
        if (entity is null) return DeleteResult.NotFound;
        if (await HasNewsAsync(db, id, ct)) return DeleteResult.InUse;
        return await Persistence.DeleteAsync(db, entity, ct);
    }
}
