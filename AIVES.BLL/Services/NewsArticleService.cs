using AIVES.BLL.Interfaces;
using AIVES.DAL.Entities;
using AIVES.DAL.Repositories.Interfaces;
using AIVES.DAL.Repositories.Models;

namespace AIVES.BLL.Services;

public class NewsArticleService : INewsArticleService
{
    private readonly INewsArticleRepository _newsRepo;
    private readonly ICategoryRepository _categoryRepo;
    private readonly ITagRepository _tagRepo;

    public NewsArticleService(
        INewsArticleRepository newsRepo,
        ICategoryRepository categoryRepo,
        ITagRepository tagRepo)
    {
        _newsRepo = newsRepo ?? throw new ArgumentNullException(nameof(newsRepo));
        _categoryRepo = categoryRepo ?? throw new ArgumentNullException(nameof(categoryRepo));
        _tagRepo = tagRepo ?? throw new ArgumentNullException(nameof(tagRepo));
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

    public async Task<int> CreateNewsAsync(
        string title,
        string content,
        int categoryId,
        byte status,
        int createdById,
        IReadOnlyCollection<int> tagIds,
        CancellationToken ct = default)
    {
        // 1. Business Validation
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("News title cannot be empty.", nameof(title));

        if (title.Trim().Length > 200)
            throw new ArgumentException("News title cannot exceed 200 characters.", nameof(title));

        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("News content cannot be empty.", nameof(content));

        if (status is not (0 or 1))
            throw new ArgumentException("News status must be either 0 (Inactive) or 1 (Active).", nameof(status));

        if (createdById <= 0)
            throw new ArgumentException("Valid creator identity is required.", nameof(createdById));

        // 2. Validate Category existence
        var category = await _categoryRepo.GetByIdAsync(categoryId, ct);
        if (category == null)
            throw new ArgumentException($"Category with ID {categoryId} does not exist.", nameof(categoryId));

        // 3. Filter valid Tags (SC-02, SC-03: allow empty, no duplicates)
        var distinctTagIds = new List<int>();
        if (tagIds != null && tagIds.Count > 0)
        {
            var validTags = await _tagRepo.GetByIdsAsync(tagIds, ct);
            distinctTagIds = validTags.Select(t => t.TagId).Distinct().ToList();
        }

        // 4. Delegate persistence to DAL
        var command = new NewsCreate(
            Title: title.Trim(),
            Content: content.Trim(),
            CategoryId: categoryId,
            Status: status,
            CreatedById: createdById,
            TagIds: distinctTagIds
        );

        return await _newsRepo.CreateAsync(command, ct);
    }

    public async Task<bool> UpdateNewsAsync(
        int id,
        string title,
        string content,
        int categoryId,
        byte status,
        int updatedById,
        IReadOnlyCollection<int> tagIds,
        CancellationToken ct = default)
    {
        if (id <= 0) return false;

        // 1. Check if news exists
        var existing = await _newsRepo.GetByIdAsync(id, activeOnly: false, ct);
        if (existing == null) return false;

        // 2. Business Validation
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("News title cannot be empty.", nameof(title));

        if (title.Trim().Length > 200)
            throw new ArgumentException("News title cannot exceed 200 characters.", nameof(title));

        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("News content cannot be empty.", nameof(content));

        if (status is not (0 or 1))
            throw new ArgumentException("News status must be either 0 (Inactive) or 1 (Active).", nameof(status));

        if (updatedById <= 0)
            throw new ArgumentException("Valid editor identity is required.", nameof(updatedById));

        // 3. Validate Category existence
        var category = await _categoryRepo.GetByIdAsync(categoryId, ct);
        if (category == null)
            throw new ArgumentException($"Category with ID {categoryId} does not exist.", nameof(categoryId));

        // 4. Filter valid Tags
        var distinctTagIds = new List<int>();
        if (tagIds != null && tagIds.Count > 0)
        {
            var validTags = await _tagRepo.GetByIdsAsync(tagIds, ct);
            distinctTagIds = validTags.Select(t => t.TagId).Distinct().ToList();
        }

        // 5. Delegate persistence to DAL with system UTC modified time
        var command = new NewsUpdate(
            Id: id,
            Title: title.Trim(),
            Content: content.Trim(),
            CategoryId: categoryId,
            Status: status,
            UpdatedById: updatedById,
            ModifiedUtc: DateTime.UtcNow,
            TagIds: distinctTagIds
        );

        return await _newsRepo.UpdateAsync(command, ct);
    }

    public async Task<bool> DeleteNewsAsync(int id, CancellationToken ct = default)
    {
        if (id <= 0) return false;
        return await _newsRepo.DeleteAsync(id, ct);
    }

    public async Task<List<Category>> GetCategoriesForDropdownAsync(CancellationToken ct = default)
    {
        return await _categoryRepo.SearchAsync(null, ct);
    }

    public async Task<List<Tag>> GetAllTagsAsync(CancellationToken ct = default)
    {
        return await _tagRepo.GetAllAsync(ct);
    }

    public async Task<List<NewsArticle>> GetActiveNewsAsync(string? keyword = null, CancellationToken ct = default)
    {
        var trimmed = string.IsNullOrWhiteSpace(keyword) ? null : keyword.Trim();
        return await _newsRepo.GetActiveAsync(trimmed, ct);
    }

    public async Task<List<NewsArticle>> GetNewsByCreatorAsync(int creatorId, string? keyword = null, CancellationToken ct = default)
    {
        if (creatorId <= 0) return new List<NewsArticle>();
        var trimmed = string.IsNullOrWhiteSpace(keyword) ? null : keyword.Trim();
        return await _newsRepo.GetByCreatorAsync(creatorId, trimmed, ct);
    }

    public async Task<List<NewsArticle>> GetNewsByCreatedDateRangeAsync(DateTime startUtc, DateTime endExclusiveUtc, CancellationToken ct = default)
    {
        if (startUtc > endExclusiveUtc)
            throw new ArgumentException("Start date cannot be after end date.");

        return await _newsRepo.GetByCreatedDateRangeAsync(startUtc, endExclusiveUtc, ct);
    }
}
