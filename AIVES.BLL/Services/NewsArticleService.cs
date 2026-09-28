using AIVES.BLL.Interfaces;
using AIVES.DAL.Entities;
using AIVES.DAL.Repositories.Interfaces;

namespace AIVES.BLL.Services;

public class NewsArticleService : INewsArticleService
{
    private readonly INewsArticleRepository _newsRepo;

    public NewsArticleService(INewsArticleRepository newsRepo)
    {
        _newsRepo = newsRepo ?? throw new ArgumentNullException(nameof(newsRepo));
    }

    public async Task<List<NewsArticle>> GetNewsByCreatedDateRangeAsync(DateTime startUtc, DateTime endExclusiveUtc, CancellationToken ct = default)
    {
        if (startUtc.Kind != DateTimeKind.Utc || endExclusiveUtc.Kind != DateTimeKind.Utc)
            throw new ArgumentException("Start and End dates must have DateTimeKind.Utc.");

        if (startUtc >= endExclusiveUtc)
            throw new ArgumentException("Start date must be strictly earlier than End date.");

        return await _newsRepo.GetByCreatedDateRangeAsync(startUtc, endExclusiveUtc, ct);
    }

    public async Task<List<NewsArticle>> GetReportArticlesAsync(DateTime startDateLocal, DateTime endDateLocal, CancellationToken ct = default)
    {
        // 1. Kiểm tra logic ngày (BR-15)
        if (startDateLocal.Date > endDateLocal.Date)
            throw new ArgumentException("Từ ngày không được lớn hơn Đến ngày.");

        // 2. Chuyển đổi từ mốc lịch ngày Việt Nam (UTC+7) sang UTC nửa mở [startUtc, endExclusiveUtc)
        // 00:00:00 của startDateLocal (UTC+7) = trừ đi 7 tiếng để ra UTC
        var startUtc = DateTime.SpecifyKind(startDateLocal.Date.AddHours(-7), DateTimeKind.Utc);

        // 00:00:00 của ngày kế tiếp sau endDateLocal (UTC+7) = cộng 1 ngày rồi trừ 7 tiếng để bao gồm trọn vẹn cả ngày kết thúc
        var endExclusiveUtc = DateTime.SpecifyKind(endDateLocal.Date.AddDays(1).AddHours(-7), DateTimeKind.Utc);

        // 3. Gọi DAL thực hiện truy vấn sắp xếp CreatedDate giảm dần rồi NewsArticleId giảm dần
        return await _newsRepo.GetByCreatedDateRangeAsync(startUtc, endExclusiveUtc, ct);
    }

    public async Task<List<NewsArticle>> SearchNewsAsync(string? keyword = null, CancellationToken ct = default)
    {
        var trimmed = string.IsNullOrWhiteSpace(keyword) ? null : keyword.Trim();
        return await _newsRepo.SearchAsync(trimmed, ct);
    }

    public async Task<NewsArticle?> GetNewsByIdAsync(int id, CancellationToken ct = default)
    {
        if (id <= 0) return null;
        return await _newsRepo.GetByIdAsync(id, activeOnly: false, ct);
    }
}
