using AIVES.DAL.Context;
using Microsoft.EntityFrameworkCore;

namespace AIVES.Tests.Data;

/// <summary>Opt-in only: CI/model tests do not depend on a developer's SQL Server.</summary>
public sealed class SqlServerReadOnlyFactAttribute : FactAttribute
{
    public SqlServerReadOnlyFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("AIVES_TEST_CONNECTION_STRING")))
            Skip = "Set AIVES_TEST_CONNECTION_STRING explicitly to run read-only SQL Server checks.";
    }
}

public class SqlServerReadOnlyTests
{
    [SqlServerReadOnlyFact]
    [Trait("Category", "SqlServerReadOnly")]
    public async Task Existing_database_supports_all_entity_and_relationship_queries()
    {
        var connectionString = Environment.GetEnvironmentVariable("AIVES_TEST_CONNECTION_STRING")
            ?? throw new InvalidOperationException("Explicit test database configuration is required.");
        var options = new DbContextOptionsBuilder<AIVESDbContext>()
            .UseSqlServer(connectionString, sql => sql.CommandTimeout(15)).Options;
        await using var context = new AIVESDbContext(options);
        Assert.True(await context.Database.CanConnectAsync());

        // Read each mapped scalar column even on an empty DB. Do not print credentials/hashes.
        await context.SystemAccounts.AsNoTracking().Take(1).ToListAsync();
        await context.Categories.AsNoTracking().Take(1).ToListAsync();
        await context.Tags.AsNoTracking().Take(1).ToListAsync();
        await context.NewsTags.AsNoTracking().Take(1).ToListAsync();
        await context.NewsArticles.AsNoTracking()
            .Include(x => x.Category).Include(x => x.CreatedBy).Include(x => x.UpdatedBy)
            .Include(x => x.NewsTags).ThenInclude(x => x.Tag)
            .OrderByDescending(x => x.CreatedDate).Take(1).ToListAsync();
        Assert.Empty(context.ChangeTracker.Entries());
        // Never call EnsureCreated, Migrate, SaveChanges, seed, or execute write SQL here.
    }
}
