using AIVES.DAL.Entities;

namespace AIVES.BLL.Interfaces;

public interface IReportService
{
    Task<List<NewsArticle>> GetReportArticlesAsync(DateTime startDateLocal, DateTime endDateLocal, CancellationToken ct = default);
    Task<List<NewsArticle>> GenerateReportAsync(DateTime startDate, DateTime endDate, CancellationToken ct = default);
    Task<List<NewsArticle>> GetNewsByCreatedDateRangeAsync(DateTime startUtc, DateTime endExclusiveUtc, CancellationToken ct = default);
}
