using AIVES.DAL.Context;
using AIVES.DAL.Entities;
using AIVES.DAL.Repositories.Models;
using Microsoft.EntityFrameworkCore;

namespace AIVES.DAL.DAOs;

public sealed class NewsArticleDAO
{
    private static readonly Lazy<NewsArticleDAO> Singleton = new(() => new NewsArticleDAO());
    public static NewsArticleDAO Instance => Singleton.Value;
    private NewsArticleDAO() { }

    private static IQueryable<NewsArticle> ReadQuery(AIVESDbContext db) => db.NewsArticles.AsNoTracking()
        .Include(x => x.Category).Include(x => x.CreatedBy).Include(x => x.UpdatedBy)
        .Include(x => x.NewsTags).ThenInclude(x => x.Tag);

    private static IQueryable<NewsArticle> Search(IQueryable<NewsArticle> query, string? keyword)
    {
        var term = keyword?.Trim();
        return string.IsNullOrEmpty(term) ? query : query.Where(x => x.NewsTitle.Contains(term) || x.NewsContent.Contains(term));
    }

    private static Task<List<NewsArticle>> ListAsync(IQueryable<NewsArticle> query, CancellationToken ct) =>
        query.OrderByDescending(x => x.CreatedDate).ThenByDescending(x => x.NewsArticleId).ToListAsync(ct);

    public Task<List<NewsArticle>> SearchAsync(AIVESDbContext db, string? keyword, CancellationToken ct) =>
        ListAsync(Search(ReadQuery(db), keyword), ct);

    public Task<NewsArticle?> GetByIdAsync(AIVESDbContext db, int id, bool activeOnly, CancellationToken ct) =>
        ReadQuery(db).SingleOrDefaultAsync(x => x.NewsArticleId == id && (!activeOnly || x.NewsStatus == 1), ct);

    public Task<List<NewsArticle>> GetActiveAsync(AIVESDbContext db, string? keyword, CancellationToken ct) =>
        ListAsync(Search(ReadQuery(db).Where(x => x.NewsStatus == 1), keyword), ct);

    public Task<List<NewsArticle>> GetByCreatorAsync(AIVESDbContext db, int creatorId, string? keyword, CancellationToken ct) =>
        ListAsync(Search(ReadQuery(db).Where(x => x.CreatedById == creatorId), keyword), ct);

    public Task<List<NewsArticle>> GetByCreatedDateRangeAsync(AIVESDbContext db, DateTime startUtc, DateTime endExclusiveUtc, CancellationToken ct)
    {
        if (startUtc.Kind != DateTimeKind.Utc || endExclusiveUtc.Kind != DateTimeKind.Utc || startUtc >= endExclusiveUtc)
            throw new ArgumentException("Expected a non-empty UTC half-open date range.");
        return ListAsync(ReadQuery(db).Where(x => x.CreatedDate >= startUtc && x.CreatedDate < endExclusiveUtc), ct);
    }

    public async Task<int> CreateAsync(AIVESDbContext db, NewsCreate input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input.TagIds);
        var entity = new NewsArticle { NewsTitle = input.Title, NewsContent = input.Content,
            CategoryId = input.CategoryId, NewsStatus = input.Status, CreatedById = input.CreatedById,
            NewsTags = input.TagIds.Distinct().Select(id => new NewsTag { TagId = id }).ToList() };
        // SQL supplies CreatedDate. One save atomically persists News and its join rows.
        db.NewsArticles.Add(entity);
        await Persistence.SaveAsync(db, ct);
        return entity.NewsArticleId;
    }

    public async Task<bool> UpdateAsync(AIVESDbContext db, NewsUpdate input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input.TagIds);
        if (input.ModifiedUtc.Kind != DateTimeKind.Utc)
            throw new ArgumentException("ModifiedUtc must be UTC.", nameof(input));
        var ids = input.TagIds.ToHashSet();
        var entity = await db.NewsArticles.Include(x => x.NewsTags).SingleOrDefaultAsync(x => x.NewsArticleId == input.Id, ct);
        if (entity is null) return false;
        entity.NewsTitle = input.Title;
        entity.NewsContent = input.Content;
        entity.CategoryId = input.CategoryId;
        entity.NewsStatus = input.Status;
        entity.UpdatedById = input.UpdatedById;
        entity.ModifiedDate = input.ModifiedUtc;
        // Never overwrite CreatedById/CreatedDate. Preserve unchanged link instances.
        var existingIds = entity.NewsTags.Select(x => x.TagId).ToHashSet();
        foreach (var link in entity.NewsTags.Where(x => !ids.Contains(x.TagId)).ToList())
        {
            entity.NewsTags.Remove(link);
            db.NewsTags.Remove(link);
        }
        foreach (var id in ids.Except(existingIds))
            entity.NewsTags.Add(new NewsTag { TagId = id });
        await Persistence.SaveAsync(db, ct);
        return true;
    }

    public async Task<bool> DeleteAsync(AIVESDbContext db, int id, CancellationToken ct)
    {
        var entity = await db.NewsArticles.SingleOrDefaultAsync(x => x.NewsArticleId == id, ct);
        if (entity is null) return false;
        db.NewsArticles.Remove(entity);
        await Persistence.SaveAsync(db, ct);
        return true;
    }
}
