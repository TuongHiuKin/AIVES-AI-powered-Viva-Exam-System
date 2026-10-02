using AIVES.DAL.Context;
using AIVES.DAL.Entities;
using AIVES.DAL.Repositories.Models;
using Microsoft.EntityFrameworkCore;

namespace AIVES.DAL.DAOs;

public sealed class SystemAccountDAO
{
    private static readonly Lazy<SystemAccountDAO> Singleton = new(() => new SystemAccountDAO());
    public static SystemAccountDAO Instance => Singleton.Value;
    private SystemAccountDAO() { }

    private static string NormalizeEmail(string email)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        return email.Trim().ToLowerInvariant();
    }

    public Task<List<SystemAccount>> SearchAsync(AIVESDbContext db, string? keyword, CancellationToken ct)
    {
        var query = db.SystemAccounts.AsNoTracking().Where(x => !x.IsDeleted);
        var term = keyword?.Trim();
        if (!string.IsNullOrEmpty(term))
            query = query.Where(x => x.AccountName.Contains(term) || x.AccountEmail.Contains(term));
        return query.OrderBy(x => x.AccountId).ToListAsync(ct);
    }

    public Task<SystemAccount?> GetByIdAsync(AIVESDbContext db, int id, bool includeDeleted, CancellationToken ct) =>
        db.SystemAccounts.AsNoTracking().SingleOrDefaultAsync(x => x.AccountId == id && (includeDeleted || !x.IsDeleted), ct);

    public Task<SystemAccount?> GetByEmailAsync(AIVESDbContext db, string email, CancellationToken ct)
    {
        var normalized = NormalizeEmail(email);
        return db.SystemAccounts.AsNoTracking().SingleOrDefaultAsync(x => !x.IsDeleted && x.AccountEmail == normalized, ct);
    }

    public Task<bool> EmailExistsAsync(AIVESDbContext db, string email, int? exceptId, CancellationToken ct)
    {
        var normalized = NormalizeEmail(email);
        return db.SystemAccounts.AnyAsync(x => x.AccountEmail == normalized && (!exceptId.HasValue || x.AccountId != exceptId.Value), ct);
    }

    public Task<bool> IsReferencedAsync(AIVESDbContext db, int id, CancellationToken ct) =>
        db.NewsArticles.AnyAsync(x => x.CreatedById == id || x.UpdatedById == id, ct);

    public async Task<int> CreateAsync(AIVESDbContext db, AccountCreate input, CancellationToken ct)
    {
        var entity = new SystemAccount { AccountName = input.Name, AccountEmail = NormalizeEmail(input.Email),
            AccountPasswordHash = input.PasswordHash, AccountRole = input.Role };
        db.SystemAccounts.Add(entity);
        await Persistence.SaveAsync(db, ct);
        return entity.AccountId;
    }

    public async Task<bool> UpdateAsync(AIVESDbContext db, AccountUpdate input, CancellationToken ct)
    {
        var email = NormalizeEmail(input.Email);
        var entity = await db.SystemAccounts.SingleOrDefaultAsync(x => x.AccountId == input.Id && !x.IsDeleted, ct);
        if (entity is null) return false;
        entity.AccountName = input.Name;
        entity.AccountEmail = email;
        entity.AccountRole = input.Role;
        if (input.NewPasswordHash is not null) entity.AccountPasswordHash = input.NewPasswordHash;
        await Persistence.SaveAsync(db, ct);
        return true;
    }

    public async Task<bool> UpdateProfileAsync(AIVESDbContext db, ProfileUpdate input, CancellationToken ct)
    {
        var email = NormalizeEmail(input.Email);
        var entity = await db.SystemAccounts.SingleOrDefaultAsync(x => x.AccountId == input.Id && !x.IsDeleted, ct);
        if (entity is null) return false;
        entity.AccountName = input.Name;
        entity.AccountEmail = email;
        if (input.NewPasswordHash is not null) entity.AccountPasswordHash = input.NewPasswordHash;
        // This contract cannot change role, identity, or deletion state.
        await Persistence.SaveAsync(db, ct);
        return true;
    }

    public async Task<bool> SoftDeleteAsync(AIVESDbContext db, int id, CancellationToken ct)
    {
        var entity = await db.SystemAccounts.SingleOrDefaultAsync(x => x.AccountId == id, ct);
        if (entity is null) return false;
        entity.IsDeleted = true;
        await Persistence.SaveAsync(db, ct);
        return true;
    }

    public async Task<DeleteResult> HardDeleteAsync(AIVESDbContext db, int id, CancellationToken ct)
    {
        var entity = await db.SystemAccounts.SingleOrDefaultAsync(x => x.AccountId == id, ct);
        if (entity is null) return DeleteResult.NotFound;
        if (await IsReferencedAsync(db, id, ct)) return DeleteResult.InUse;
        return await Persistence.DeleteAsync(db, entity, ct);
    }
}
