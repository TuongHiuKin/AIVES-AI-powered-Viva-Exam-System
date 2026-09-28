using AIVES.BLL.Interfaces;
using AIVES.BLL.Services;
using AIVES.DAL.Entities;
using AIVES.DAL.Repositories.Interfaces;
using AIVES.DAL.Repositories.Models;
using Xunit;

namespace AIVES.Tests;

public class NewsArticleServiceTests
{
    private readonly FakeNewsArticleRepository _newsRepo;
    private readonly FakeCategoryRepository _categoryRepo;
    private readonly FakeTagRepository _tagRepo;
    private readonly INewsArticleService _service;

    public NewsArticleServiceTests()
    {
        _newsRepo = new FakeNewsArticleRepository();
        _categoryRepo = new FakeCategoryRepository();
        _tagRepo = new FakeTagRepository();
        _service = new NewsArticleService(_newsRepo, _categoryRepo, _tagRepo);

        // Seed initial fake categories and tags
        _categoryRepo.Categories.Add(new Category { CategoryId = 1, CategoryName = "Công nghệ", CategoryDescription = "Tin công nghệ" });
        _categoryRepo.Categories.Add(new Category { CategoryId = 2, CategoryName = "Giáo dục", CategoryDescription = "Tin giáo dục" });

        _tagRepo.Tags.Add(new Tag { TagId = 1, TagName = ".NET" });
        _tagRepo.Tags.Add(new Tag { TagId = 2, TagName = "C#" });
    }

    [Fact]
    public async Task CreateNewsAsync_WithValidData_ReturnsNewArticleId()
    {
        // Act
        var resultId = await _service.CreateNewsAsync(
            title: "Tin tức kiểm thử hợp lệ",
            content: "Nội dung bài viết kiểm thử chi tiết",
            categoryId: 1,
            status: 1,
            createdById: 10,
            tagIds: new[] { 1, 2 }
        );

        // Assert
        Assert.True(resultId > 0);
        Assert.Single(_newsRepo.Articles);
        Assert.Equal("Tin tức kiểm thử hợp lệ", _newsRepo.Articles[0].NewsTitle);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task CreateNewsAsync_WithBlankTitle_ThrowsArgumentException(string? invalidTitle)
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.CreateNewsAsync(
                title: invalidTitle!,
                content: "Nội dung hợp lệ",
                categoryId: 1,
                status: 1,
                createdById: 10,
                tagIds: Array.Empty<int>()
            ));
    }

    [Fact]
    public async Task CreateNewsAsync_WithTitleExceeding200Chars_ThrowsArgumentException()
    {
        // Arrange
        var longTitle = new string('A', 201);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.CreateNewsAsync(
                title: longTitle,
                content: "Nội dung hợp lệ",
                categoryId: 1,
                status: 1,
                createdById: 10,
                tagIds: Array.Empty<int>()
            ));
    }

    [Fact]
    public async Task CreateNewsAsync_WithNonExistentCategory_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.CreateNewsAsync(
                title: "Tiêu đề",
                content: "Nội dung",
                categoryId: 999, // Không tồn tại
                status: 1,
                createdById: 10,
                tagIds: Array.Empty<int>()
            ));
    }

    [Theory]
    [InlineData(2)]
    [InlineData(5)]
    public async Task CreateNewsAsync_WithInvalidStatus_ThrowsArgumentException(byte invalidStatus)
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.CreateNewsAsync(
                title: "Tiêu đề",
                content: "Nội dung",
                categoryId: 1,
                status: invalidStatus,
                createdById: 10,
                tagIds: Array.Empty<int>()
            ));
    }

    [Fact]
    public async Task UpdateNewsAsync_WithNonExistentId_ReturnsFalse()
    {
        // Act
        var result = await _service.UpdateNewsAsync(
            id: 999,
            title: "Tiêu đề mới",
            content: "Nội dung mới",
            categoryId: 1,
            status: 1,
            updatedById: 10,
            tagIds: Array.Empty<int>()
        );

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task UpdateNewsAsync_WithValidData_ReturnsTrue()
    {
        // Arrange: tạo trước 1 bài viết
        var id = await _service.CreateNewsAsync(
            title: "Tiêu đề ban đầu",
            content: "Nội dung ban đầu",
            categoryId: 1,
            status: 0,
            createdById: 10,
            tagIds: Array.Empty<int>()
        );

        // Act: cập nhật
        var result = await _service.UpdateNewsAsync(
            id: id,
            title: "Tiêu đề đã sửa",
            content: "Nội dung đã sửa",
            categoryId: 2,
            status: 1,
            updatedById: 20,
            tagIds: new[] { 1 }
        );

        // Assert
        Assert.True(result);
        var updated = await _service.GetNewsByIdAsync(id);
        Assert.NotNull(updated);
        Assert.Equal("Tiêu đề đã sửa", updated.NewsTitle);
        Assert.Equal(2, updated.CategoryId);
        Assert.Equal(1, updated.NewsStatus);
    }

    [Fact]
    public async Task DeleteNewsAsync_WithValidId_ReturnsTrue()
    {
        // Arrange: tạo trước bài viết
        var id = await _service.CreateNewsAsync("Bài cần xóa", "Nội dung", 1, 1, 10, Array.Empty<int>());

        // Act
        var result = await _service.DeleteNewsAsync(id);

        // Assert
        Assert.True(result);
        var deletedArticle = await _service.GetNewsByIdAsync(id);
        Assert.Null(deletedArticle);
    }

    [Fact]
    public async Task DeleteNewsAsync_WithNonExistentId_ReturnsFalse()
    {
        // Act
        var result = await _service.DeleteNewsAsync(9999);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetNewsByCreatedDateRangeAsync_WithStartAfterEnd_ThrowsArgumentException()
    {
        // Arrange: startUtc lớn hơn endUtc
        var start = DateTime.UtcNow;
        var end = start.AddDays(-1);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.GetNewsByCreatedDateRangeAsync(start, end));
    }

    #region Fake In-Memory Repositories for Isolated Testing

    private class FakeNewsArticleRepository : INewsArticleRepository
    {
        public List<NewsArticle> Articles { get; } = new();
        private int _nextId = 1;

        public Task<List<NewsArticle>> SearchAsync(string? keyword = null, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return Task.FromResult(Articles.ToList());

            return Task.FromResult(Articles
                .Where(a => a.NewsTitle.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                            a.NewsContent.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                .ToList());
        }

        public Task<NewsArticle?> GetByIdAsync(int id, bool activeOnly = false, CancellationToken ct = default)
        {
            var article = Articles.FirstOrDefault(a => a.NewsArticleId == id);
            if (activeOnly && article?.NewsStatus != 1) return Task.FromResult<NewsArticle?>(null);
            return Task.FromResult(article);
        }

        public Task<List<NewsArticle>> GetActiveAsync(string? keyword = null, CancellationToken ct = default)
        {
            return Task.FromResult(Articles.Where(a => a.NewsStatus == 1).ToList());
        }

        public Task<List<NewsArticle>> GetByCreatorAsync(int creatorId, string? keyword = null, CancellationToken ct = default)
        {
            return Task.FromResult(Articles.Where(a => a.CreatedById == creatorId).ToList());
        }

        public Task<List<NewsArticle>> GetByCreatedDateRangeAsync(DateTime startUtc, DateTime endExclusiveUtc, CancellationToken ct = default)
        {
            return Task.FromResult(Articles
                .Where(a => a.CreatedDate >= startUtc && a.CreatedDate < endExclusiveUtc)
                .OrderByDescending(a => a.CreatedDate)
                .ThenByDescending(a => a.NewsArticleId)
                .ToList());
        }

        public Task<int> CreateAsync(NewsCreate input, CancellationToken ct = default)
        {
            var article = new NewsArticle
            {
                NewsArticleId = _nextId++,
                NewsTitle = input.Title,
                NewsContent = input.Content,
                CategoryId = input.CategoryId,
                NewsStatus = input.Status,
                CreatedById = input.CreatedById,
                CreatedDate = DateTime.UtcNow
            };
            Articles.Add(article);
            return Task.FromResult(article.NewsArticleId);
        }

        public Task<bool> UpdateAsync(NewsUpdate input, CancellationToken ct = default)
        {
            var article = Articles.FirstOrDefault(a => a.NewsArticleId == input.Id);
            if (article == null) return Task.FromResult(false);

            article.NewsTitle = input.Title;
            article.NewsContent = input.Content;
            article.CategoryId = input.CategoryId;
            article.NewsStatus = input.Status;
            article.UpdatedById = input.UpdatedById;
            article.ModifiedDate = input.ModifiedUtc;
            return Task.FromResult(true);
        }

        public Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        {
            var article = Articles.FirstOrDefault(a => a.NewsArticleId == id);
            if (article == null) return Task.FromResult(false);
            Articles.Remove(article);
            return Task.FromResult(true);
        }
    }

    private class FakeCategoryRepository : ICategoryRepository
    {
        public List<Category> Categories { get; } = new();

        public Task<List<Category>> SearchAsync(string? keyword = null, CancellationToken ct = default)
        {
            return Task.FromResult(Categories.ToList());
        }

        public Task<Category?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return Task.FromResult(Categories.FirstOrDefault(c => c.CategoryId == id));
        }

        public Task<bool> HasNewsAsync(int id, CancellationToken ct = default) => Task.FromResult(false);
        public Task<int> CreateAsync(string name, string? description, CancellationToken ct = default) => Task.FromResult(1);
        public Task<bool> UpdateAsync(int id, string name, string? description, CancellationToken ct = default) => Task.FromResult(true);
        public Task<DeleteResult> DeleteAsync(int id, CancellationToken ct = default) => Task.FromResult(DeleteResult.Deleted);
    }

    private class FakeTagRepository : ITagRepository
    {
        public List<Tag> Tags { get; } = new();

        public Task<List<Tag>> GetAllAsync(CancellationToken ct = default) => Task.FromResult(Tags.ToList());
        public Task<Tag?> GetByIdAsync(int id, CancellationToken ct = default) => Task.FromResult(Tags.FirstOrDefault(t => t.TagId == id));
        public Task<List<Tag>> GetByIdsAsync(IReadOnlyCollection<int> ids, CancellationToken ct = default)
        {
            return Task.FromResult(Tags.Where(t => ids.Contains(t.TagId)).ToList());
        }
        public Task<bool> IsUsedAsync(int id, CancellationToken ct = default) => Task.FromResult(false);
        public Task<DeleteResult> DeleteAsync(int id, CancellationToken ct = default) => Task.FromResult(DeleteResult.Deleted);
    }

    #endregion
}
