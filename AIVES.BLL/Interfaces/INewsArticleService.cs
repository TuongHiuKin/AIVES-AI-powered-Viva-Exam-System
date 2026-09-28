using AIVES.DAL.Entities;

namespace AIVES.BLL.Interfaces;

public interface INewsArticleService
{
    // --- 1. TV5 (REPORT-01): Báo cáo Admin theo khoảng thời gian ngày tạo ---
    Task<List<NewsArticle>> GetNewsByCreatedDateRangeAsync(DateTime startUtc, DateTime endExclusiveUtc, CancellationToken ct = default);
    Task<List<NewsArticle>> GetReportArticlesAsync(DateTime startDateLocal, DateTime endDateLocal, CancellationToken ct = default);

    // --- 2. TV3 News Management contracts (để tương thích chéo với các module khác) ---
    Task<List<NewsArticle>> SearchNewsAsync(string? keyword = null, CancellationToken ct = default);
    Task<NewsArticle?> GetNewsByIdAsync(int id, CancellationToken ct = default);
}
