using System.Text.RegularExpressions;
using AIVES.DAL.Context;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace AIVES.Tests.Data;

public sealed class SqlServerWriteFactAttribute : FactAttribute
{
    public SqlServerWriteFactAttribute()
    {
        if (Environment.GetEnvironmentVariable("AIVES_RUN_DATABASE_WRITE_TESTS") != "1" ||
            string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("AIVES_TEST_SERVER_CONNECTION_STRING")))
            Skip = "Opt in with AIVES_RUN_DATABASE_WRITE_TESTS=1 and AIVES_TEST_SERVER_CONNECTION_STRING; writes only to a new temporary database.";
    }
}

/// <summary>Owns ONLY a randomly named test database created by this fixture.</summary>
public sealed class TemporarySqlServerDatabase : IAsyncLifetime
{
    private readonly string databaseName = "AIVES_DAL_Test_" + Guid.NewGuid().ToString("N");
    private string masterConnection = string.Empty;
    private bool created;
    public string ConnectionString { get; private set; } = string.Empty;

    public AIVESDbContext CreateContext() => new(new DbContextOptionsBuilder<AIVESDbContext>()
        .UseSqlServer(ConnectionString).Options);

    public async Task InitializeAsync()
    {
        if (Environment.GetEnvironmentVariable("AIVES_RUN_DATABASE_WRITE_TESTS") != "1") return;
        var source = Environment.GetEnvironmentVariable("AIVES_TEST_SERVER_CONNECTION_STRING");
        if (string.IsNullOrWhiteSpace(source)) return;
        var builder = new SqlConnectionStringBuilder(source) { InitialCatalog = "master", Pooling = false };
        masterConnection = builder.ConnectionString;
        builder.InitialCatalog = databaseName;
        ConnectionString = builder.ConnectionString;
        ValidateName();
        await using var master = new SqlConnection(masterConnection);
        await master.OpenAsync();
        // CREATE (not IF NOT EXISTS): a collision must fail, never adopt someone else's DB.
        await using var command = new SqlCommand($"CREATE DATABASE [{databaseName}]", master);
        await command.ExecuteNonQueryAsync();
        created = true;
        try
        {
            await RunScriptAsync("001-create-schema.sql");
            await RunScriptAsync("002-add-system-account-soft-delete.sql");
        }
        catch
        {
            await DisposeAsync();
            throw;
        }
    }

    public async Task RunScriptAsync(string fileName)
    {
        if (!created) throw new InvalidOperationException("No fixture-owned test DB.");
        if (fileName is not ("001-create-schema.sql" or "002-add-system-account-soft-delete.sql"))
            throw new ArgumentException("Only the two reviewed schema scripts are permitted.", nameof(fileName));
        ValidateName();
        var sql = await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "Sql", fileName));
        // Redirect both USE/database creation and DB_NAME guards to our generated name.
        sql = sql.Replace("[AIVES]", $"[{databaseName}]").Replace("N'AIVES'", $"N'{databaseName}'");
        await using var connection = new SqlConnection(ConnectionString);
        await connection.OpenAsync();
        foreach (var batch in Regex.Split(sql, @"^\s*GO\s*\r?$", RegexOptions.Multiline | RegexOptions.IgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(batch)) continue;
            await using var command = new SqlCommand(batch, connection) { CommandTimeout = 60 };
            await command.ExecuteNonQueryAsync();
        }
    }

    public async Task DisposeAsync()
    {
        if (!created) return;
        ValidateName();
        await using var master = new SqlConnection(masterConnection);
        await master.OpenAsync();
        await using var command = new SqlCommand(
            $"ALTER DATABASE [{databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{databaseName}];", master);
        await command.ExecuteNonQueryAsync();
        created = false;
    }

    private void ValidateName()
    {
        if (!Regex.IsMatch(databaseName, @"\AAIVES_DAL_Test_[0-9a-f]{32}\z"))
            throw new InvalidOperationException("Refusing to modify a non-test database.");
    }
}
