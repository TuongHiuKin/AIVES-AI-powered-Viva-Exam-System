using AIVES.DAL.Context;
using AIVES.DAL.DAOs;
using AIVES.DAL.Entities;
using AIVES.DAL.Repositories.Interfaces;
using AIVES.DAL.Repositories.Models;

namespace AIVES.DAL.Repositories.Implementations;

public sealed class NewsArticleRepository(AIVESDbContext context) : INewsArticleRepository
{
    public Task<List<NewsArticle>> SearchAsync(string? keyword = null, CancellationToken ct = default) =>
        NewsArticleDAO.Instance.SearchAsync(context, keyword, ct);

    public Task<NewsArticle?> GetByIdAsync(int id, bool activeOnly = false, CancellationToken ct = default) =>
        NewsArticleDAO.Instance.GetByIdAsync(context, id, activeOnly, ct);

    public Task<List<NewsArticle>> GetActiveAsync(string? keyword = null, CancellationToken ct = default) =>
        NewsArticleDAO.Instance.GetActiveAsync(context, keyword, ct);

    public Task<List<NewsArticle>> GetByCreatorAsync(int creatorId, string? keyword = null, CancellationToken ct = default) =>
        NewsArticleDAO.Instance.GetByCreatorAsync(context, creatorId, keyword, ct);

    public Task<List<NewsArticle>> GetByCreatedDateRangeAsync(DateTime startUtc, DateTime endExclusiveUtc, CancellationToken ct = default) =>
        NewsArticleDAO.Instance.GetByCreatedDateRangeAsync(context, startUtc, endExclusiveUtc, ct);

    public Task<int> CreateAsync(NewsCreate input, CancellationToken ct = default) =>
        NewsArticleDAO.Instance.CreateAsync(context, input, ct);

    public Task<bool> UpdateAsync(NewsUpdate input, CancellationToken ct = default) =>
        NewsArticleDAO.Instance.UpdateAsync(context, input, ct);

    public Task<bool> DeleteAsync(int id, CancellationToken ct = default) =>
        NewsArticleDAO.Instance.DeleteAsync(context, id, ct);
}
