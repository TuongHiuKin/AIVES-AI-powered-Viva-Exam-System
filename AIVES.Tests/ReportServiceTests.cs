using AIVES.BLL.Services;
using AIVES.DAL.Entities;
using AIVES.DAL.Repositories.Interfaces;
using AIVES.DAL.Repositories.Models;
using Xunit;

namespace AIVES.Tests;

public class ReportServiceTests
{
    private class FakeNewsArticleRepository : INewsArticleRepository
    {
        public DateTime? LastStartUtc { get; private set; }
        public DateTime? LastEndExclusiveUtc { get; private set; }

        public Task<List<NewsArticle>> GetByCreatedDateRangeAsync(DateTime startUtc, DateTime endExclusiveUtc, CancellationToken ct = default)
        {
            LastStartUtc = startUtc;
            LastEndExclusiveUtc = endExclusiveUtc;

            var fakeArticles = new List<NewsArticle>
            {
                new()
                {
                    NewsArticleId = 1,
                    NewsTitle = "Báo cáo bài viết mẫu",
                    NewsContent = "Nội dung mẫu",
                    CreatedDate = startUtc.AddHours(2),
                    NewsStatus = 1
                }
            };
            return Task.FromResult(fakeArticles);
        }

        public Task<List<NewsArticle>> SearchAsync(string? keyword = null, CancellationToken ct = default) =>
            Task.FromResult(new List<NewsArticle>());

        public Task<NewsArticle?> GetByIdAsync(int id, bool activeOnly = false, CancellationToken ct = default) =>
            Task.FromResult<NewsArticle?>(null);

        public Task<List<NewsArticle>> GetActiveAsync(string? keyword = null, CancellationToken ct = default) =>
            Task.FromResult(new List<NewsArticle>());

        public Task<List<NewsArticle>> GetByCreatorAsync(int creatorId, string? keyword = null, CancellationToken ct = default) =>
            Task.FromResult(new List<NewsArticle>());

        public Task<int> CreateAsync(NewsCreate input, CancellationToken ct = default) => Task.FromResult(1);
        public Task<bool> UpdateAsync(NewsUpdate input, CancellationToken ct = default) => Task.FromResult(true);
        public Task<bool> DeleteAsync(int id, CancellationToken ct = default) => Task.FromResult(true);
    }

    [Fact]
    public async Task GetReportArticlesAsync_WithValidDates_ConvertsVietnamTimeToUtcHalfOpenRange()
    {
        var repo = new FakeNewsArticleRepository();
        var service = new ReportService(repo);

        var startDateLocal = new DateTime(2026, 9, 1);
        var endDateLocal = new DateTime(2026, 9, 10);

        var result = await service.GetReportArticlesAsync(startDateLocal, endDateLocal);

        Assert.NotEmpty(result);
        Assert.NotNull(repo.LastStartUtc);
        Assert.NotNull(repo.LastEndExclusiveUtc);

        // Giờ Việt Nam là UTC+7: 00:00 2026-09-01 VN => 17:00 2026-08-31 UTC
        Assert.Equal(DateTimeKind.Utc, repo.LastStartUtc.Value.Kind);
        Assert.Equal(new DateTime(2026, 8, 31, 17, 0, 0, DateTimeKind.Utc), repo.LastStartUtc.Value);

        // 00:00 2026-09-11 VN => 17:00 2026-09-10 UTC
        Assert.Equal(DateTimeKind.Utc, repo.LastEndExclusiveUtc.Value.Kind);
        Assert.Equal(new DateTime(2026, 9, 10, 17, 0, 0, DateTimeKind.Utc), repo.LastEndExclusiveUtc.Value);
    }

    [Fact]
    public async Task GetReportArticlesAsync_WhenStartDateAfterEndDate_ThrowsArgumentException()
    {
        var repo = new FakeNewsArticleRepository();
        var service = new ReportService(repo);

        var startDateLocal = new DateTime(2026, 9, 20);
        var endDateLocal = new DateTime(2026, 9, 10);

        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.GetReportArticlesAsync(startDateLocal, endDateLocal));

        Assert.Contains("lớn hơn", ex.Message);
    }

    [Fact]
    public async Task GenerateReportAsync_ValidDates_CallsGetReportArticlesAsync()
    {
        var repo = new FakeNewsArticleRepository();
        var service = new ReportService(repo);

        var startDate = new DateTime(2026, 9, 1);
        var endDate = new DateTime(2026, 9, 5);

        var result = await service.GenerateReportAsync(startDate, endDate);

        Assert.Single(result);
        Assert.Equal(1, result[0].NewsArticleId);
    }
}
