using AIVES.DAL.Context;
using AIVES.DAL.Entities;
using AIVES.DAL.Repositories.Implementations;
using AIVES.DAL.Repositories.Models;
using Microsoft.EntityFrameworkCore;

namespace AIVES.Tests.Data;

[Trait("Category", "SqlServerWrite")]
public sealed class RepositorySqlServerTests(TemporarySqlServerDatabase database) : IClassFixture<TemporarySqlServerDatabase>
{
    private static string Key() => Guid.NewGuid().ToString("N");
    private static Task<int> Account(AIVESDbContext db, string? email = null) =>
        new SystemAccountRepository(db).CreateAsync(new AccountCreate("Test " + Key(), email ?? Key() + "@example.test", "opaque-test-hash", 1));

    private static async Task<(int Author, int Category, int Tag)> Seed(AIVESDbContext db)
    {
        var author = await Account(db);
        var category = await new CategoryRepository(db).CreateAsync("Category " + Key(), null);
        var tag = new Tag { TagName = "Tag " + Key() };
        db.Tags.Add(tag);
        await db.SaveChangesAsync(); // Fixture data only, never application DB.
        return (author, category, tag.TagId);
    }

    [SqlServerWriteFact]
    public async Task Accounts_normalize_email_preserve_profile_role_and_soft_delete_reserves_email()
    {
        await using var db = database.CreateContext();
        var repo = new SystemAccountRepository(db);
        var email = Key() + "@example.test";
        var id = await Account(db, "  " + email.ToUpperInvariant() + "  ");
        Assert.Equal(id, (await repo.GetByEmailAsync(email.ToUpperInvariant()))!.AccountId);
        Assert.False((await repo.GetByIdAsync(id))!.IsDeleted);
        Assert.True(await repo.EmailExistsAsync(email));
        Assert.False(await repo.EmailExistsAsync(email, id));
        Assert.True(await repo.UpdateAsync(new(id, "Admin edited", email, 2)));
        Assert.True(await repo.UpdateProfileAsync(new(id, "Own profile", email)));
        Assert.Equal(2, (await repo.GetByIdAsync(id))!.AccountRole);
        Assert.Equal("opaque-test-hash", (await repo.GetByIdAsync(id))!.AccountPasswordHash);
        Assert.True(await repo.SoftDeleteAsync(id));
        Assert.Null(await repo.GetByIdAsync(id));
        Assert.Null(await repo.GetByEmailAsync(email));
        Assert.Empty(await repo.SearchAsync(email));
        Assert.True((await repo.GetByIdAsync(id, includeDeleted: true))!.IsDeleted);
        Assert.True(await repo.EmailExistsAsync(email));
        Assert.False(await repo.UpdateAsync(new(id, "Cannot restore", email, 1)));
        await Assert.ThrowsAsync<DbUpdateException>(() => Account(db, email));
        Assert.Equal(DeleteResult.Deleted, await repo.HardDeleteAsync(id));
        Assert.False(await repo.EmailExistsAsync(email));
    }

    [SqlServerWriteFact]
    public async Task News_create_update_sync_tags_preserve_creator_and_delete_only_links()
    {
        await using var db = database.CreateContext();
        var (author, category, tag) = await Seed(db);
        var editor = await Account(db);
        var repo = new NewsArticleRepository(db);
        var id = await repo.CreateAsync(new("First", "Content", category, 1, author, new[] { tag, tag }));
        var original = (await repo.GetByIdAsync(id))!;
        Assert.Single(original.NewsTags);
        Assert.Equal(tag, original.NewsTags.Single().Tag.TagId);
        Assert.Equal(category, original.Category.CategoryId);
        Assert.Null(original.UpdatedById);
        Assert.Null(original.ModifiedDate);
        Assert.NotEqual(default, original.CreatedDate);
        Assert.True(await repo.UpdateAsync(new(id, "Edited", "New body", category, 0, editor, DateTime.UtcNow.AddSeconds(1), Array.Empty<int>())));
        var updated = (await repo.GetByIdAsync(id))!;
        Assert.Empty(updated.NewsTags);
        Assert.Equal(author, updated.CreatedById);
        Assert.Equal(original.CreatedDate, updated.CreatedDate);
        Assert.Equal(editor, updated.UpdatedById);
        Assert.Equal("Edited", updated.NewsTitle);
        Assert.Equal(0, updated.NewsStatus);
        Assert.True(await repo.UpdateAsync(new(id, "Edited twice", "Body", category, 1, editor, DateTime.UtcNow.AddSeconds(2), new[] { tag, tag })));
        Assert.Single((await repo.GetByIdAsync(id))!.NewsTags);
        Assert.True(await repo.DeleteAsync(id));
        Assert.Null(await repo.GetByIdAsync(id));
        Assert.False(await db.NewsTags.AnyAsync(x => x.NewsArticleId == id));
        Assert.NotNull(await db.Tags.FindAsync(tag));
        Assert.NotNull(await db.Categories.FindAsync(category));
        Assert.NotNull(await db.SystemAccounts.FindAsync(author));
    }

    [SqlServerWriteFact]
    public async Task Failed_tag_write_rolls_back_news_content_audit_and_link_changes()
    {
        await using var db = database.CreateContext();
        var (author, category, tag) = await Seed(db);
        var repo = new NewsArticleRepository(db);
        var id = await repo.CreateAsync(new("Before", "Original content", category, 1, author, new[] { tag }));
        await Assert.ThrowsAsync<DbUpdateException>(() => repo.UpdateAsync(new(id, "Must rollback", "Changed", category, 0,
            author, DateTime.UtcNow.AddSeconds(1), new[] { int.MaxValue })));
        Assert.Empty(db.ChangeTracker.Entries());
        await using var verify = database.CreateContext();
        var actual = (await new NewsArticleRepository(verify).GetByIdAsync(id))!;
        Assert.Equal("Before", actual.NewsTitle);
        Assert.Equal("Original content", actual.NewsContent);
        Assert.Equal(1, actual.NewsStatus);
        Assert.Null(actual.ModifiedDate);
        Assert.Null(actual.UpdatedById);
        Assert.Equal(tag, Assert.Single(actual.NewsTags).TagId);
        var marker = Key();
        await Assert.ThrowsAsync<DbUpdateException>(() => repo.CreateAsync(new(marker, "Body", category, 1, author, new[] { int.MaxValue })));
        Assert.Empty(await repo.SearchAsync(marker));
    }

    [SqlServerWriteFact]
    public async Task Used_category_tag_creator_and_editor_cannot_be_deleted_and_soft_deleted_author_remains_visible()
    {
        await using var db = database.CreateContext();
        var (author, category, tag) = await Seed(db);
        var editor = await Account(db);
        var news = new NewsArticleRepository(db);
        var accounts = new SystemAccountRepository(db);
        var categories = new CategoryRepository(db);
        var tags = new TagRepository(db);
        var id = await news.CreateAsync(new("References", "Inactive also counts", category, 0, author, new[] { tag }));
        Assert.True(await news.UpdateAsync(new(id, "References", "Body", category, 0, editor, DateTime.UtcNow.AddSeconds(1), new[] { tag })));
        Assert.True(await categories.HasNewsAsync(category));
        Assert.Equal(DeleteResult.InUse, await categories.DeleteAsync(category));
        Assert.True(await tags.IsUsedAsync(tag));
        Assert.Equal(DeleteResult.InUse, await tags.DeleteAsync(tag));
        Assert.True(await accounts.IsReferencedAsync(editor));
        Assert.Equal(DeleteResult.InUse, await accounts.HardDeleteAsync(author));
        Assert.Equal(DeleteResult.InUse, await accounts.HardDeleteAsync(editor));
        Assert.True(await accounts.SoftDeleteAsync(author));
        Assert.True((await news.GetByIdAsync(id))!.CreatedBy.IsDeleted);
        Assert.Contains(await news.GetByCreatorAsync(author), x => x.NewsArticleId == id);
        await news.DeleteAsync(id);
        Assert.Equal(DeleteResult.Deleted, await categories.DeleteAsync(category));
        Assert.Equal(DeleteResult.Deleted, await tags.DeleteAsync(tag));
        Assert.Equal(DeleteResult.Deleted, await accounts.HardDeleteAsync(author));
        Assert.Equal(DeleteResult.Deleted, await accounts.HardDeleteAsync(editor));
    }

    [SqlServerWriteFact]
    public async Task Search_public_and_history_keep_filters_and_handle_empty_keywords()
    {
        await using var db = database.CreateContext();
        var (author, category, _) = await Seed(db);
        var other = await Account(db);
        var repo = new NewsArticleRepository(db);
        var key = Key();
        var first = await repo.CreateAsync(new(key, "Title match", category, 1, author, Array.Empty<int>()));
        var second = await repo.CreateAsync(new("Content match", key, category, 0, author, Array.Empty<int>()));
        await repo.CreateAsync(new(key, "Other creator", category, 1, other, Array.Empty<int>()));
        Assert.Equal(3, (await repo.SearchAsync("  " + key + "  ")).Count);
        Assert.Equal(2, (await repo.GetActiveAsync(key)).Count);
        Assert.Equal(new[] { second, first }, (await repo.GetByCreatorAsync(author, key)).Select(x => x.NewsArticleId));
        Assert.Null(await repo.GetByIdAsync(second, activeOnly: true));
        Assert.NotNull(await repo.GetByIdAsync(first, activeOnly: true));
        Assert.Equal((await repo.SearchAsync(null)).Count, (await repo.SearchAsync("  ")).Count);
        Assert.All(await repo.GetActiveAsync(" "), x => Assert.Equal(1, x.NewsStatus));
        var categories = new CategoryRepository(db);
        Assert.True(await categories.UpdateAsync(category, "Name " + key, null));
        Assert.Single(await categories.SearchAsync(" " + key + " "));
        Assert.True(await categories.UpdateAsync(category, "No match", key));
        Assert.Single(await categories.SearchAsync(key));
        Assert.NotEmpty(await categories.SearchAsync(" "));
        var tags = new TagRepository(db);
        Assert.Empty(await tags.GetByIdsAsync(Array.Empty<int>()));
    }

    [SqlServerWriteFact]
    public async Task Report_is_start_inclusive_end_exclusive_and_sorts_descending_with_id_ties()
    {
        await using var db = database.CreateContext();
        var (author, category, _) = await Seed(db);
        var start = new DateTime(2001, 2, 3, 17, 0, 0, DateTimeKind.Utc);
        var end = start.AddDays(1);
        // Direct fixture timestamps exercise exact boundaries without changing production clock behavior.
        var rows = new[] { start.AddMilliseconds(-1), start, end.AddMilliseconds(-1), end.AddMilliseconds(-1), end }
            .Select(time => new NewsArticle { NewsTitle = "Report", NewsContent = "Fixture", CategoryId = category,
                CreatedById = author, NewsStatus = 1, CreatedDate = time }).ToArray();
        db.NewsArticles.AddRange(rows);
        await db.SaveChangesAsync();
        var results = await new NewsArticleRepository(db).GetByCreatedDateRangeAsync(start, end);
        Assert.Equal(new[] { rows[3].NewsArticleId, rows[2].NewsArticleId, rows[1].NewsArticleId }, results.Select(x => x.NewsArticleId));
    }

    [SqlServerWriteFact]
    public async Task Missing_ids_are_safe_and_database_constraints_reject_invalid_status_role_and_audit()
    {
        await using var db = database.CreateContext();
        var (author, category, _) = await Seed(db);
        var accounts = new SystemAccountRepository(db);
        var categories = new CategoryRepository(db);
        var news = new NewsArticleRepository(db);
        Assert.False(await accounts.SoftDeleteAsync(-1));
        Assert.False(await accounts.UpdateProfileAsync(new(-1, "Missing", "missing@example.test")));
        Assert.Equal(DeleteResult.NotFound, await accounts.HardDeleteAsync(-1));
        Assert.Equal(DeleteResult.NotFound, await categories.DeleteAsync(-1));
        Assert.Equal(DeleteResult.NotFound, await new TagRepository(db).DeleteAsync(-1));
        Assert.False(await categories.UpdateAsync(-1, "Missing", null));
        Assert.False(await news.DeleteAsync(-1));
        Assert.False(await news.UpdateAsync(new(-1, "Missing", "Body", category, 1, author, DateTime.UtcNow, Array.Empty<int>())));
        await Assert.ThrowsAsync<DbUpdateException>(() => accounts.CreateAsync(new("Bad role", Key() + "@example.test", "opaque", 3)));
        await Assert.ThrowsAsync<DbUpdateException>(() => news.CreateAsync(new("Bad status", "Body", category, 2, author, Array.Empty<int>())));
        var id = await news.CreateAsync(new("Valid", "Body", category, 1, author, Array.Empty<int>()));
        await Assert.ThrowsAsync<DbUpdateException>(() => news.UpdateAsync(new(id, "Bad audit", "Body", category, 1, author,
            new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc), Array.Empty<int>())));
        Assert.Equal("Valid", (await news.GetByIdAsync(id))!.NewsTitle);
    }

    [SqlServerWriteFact]
    public async Task Upgrade_script_rerun_preserves_soft_deleted_account()
    {
        await using var db = database.CreateContext();
        var repo = new SystemAccountRepository(db);
        var id = await Account(db);
        await repo.SoftDeleteAsync(id);
        await database.RunScriptAsync("002-add-system-account-soft-delete.sql");
        Assert.True((await repo.GetByIdAsync(id, includeDeleted: true))!.IsDeleted);
    }
}
