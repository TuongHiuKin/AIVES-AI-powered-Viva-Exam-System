using AIVES.DAL.Entities;
using AIVES.DAL.Repositories.Models;

namespace AIVES.DAL.Repositories.Interfaces;

public interface INewsArticleRepository
{
    Task<List<NewsArticle>> SearchAsync(string? keyword = null, CancellationToken ct = default);
    Task<NewsArticle?> GetByIdAsync(int id, bool activeOnly = false, CancellationToken ct = default);
    Task<List<NewsArticle>> GetActiveAsync(string? keyword = null, CancellationToken ct = default);
    Task<List<NewsArticle>> GetByCreatorAsync(int creatorId, string? keyword = null, CancellationToken ct = default);
    // UTC half-open range; BLL converts Vietnamese calendar dates before calling.
    Task<List<NewsArticle>> GetByCreatedDateRangeAsync(DateTime startUtc, DateTime endExclusiveUtc, CancellationToken ct = default);
    Task<int> CreateAsync(NewsCreate input, CancellationToken ct = default);
    Task<bool> UpdateAsync(NewsUpdate input, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}
