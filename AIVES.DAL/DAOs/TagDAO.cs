using AIVES.DAL.Context;
using AIVES.DAL.Entities;
using AIVES.DAL.Repositories.Models;
using Microsoft.EntityFrameworkCore;

namespace AIVES.DAL.DAOs;

public sealed class TagDAO
{
    private static readonly Lazy<TagDAO> Singleton = new(() => new TagDAO());
    public static TagDAO Instance => Singleton.Value;
    private TagDAO() { }

    public Task<List<Tag>> GetAllAsync(AIVESDbContext db, CancellationToken ct) =>
        db.Tags.AsNoTracking().OrderBy(x => x.TagName).ThenBy(x => x.TagId).ToListAsync(ct);

    public Task<Tag?> GetByIdAsync(AIVESDbContext db, int id, CancellationToken ct) =>
        db.Tags.AsNoTracking().SingleOrDefaultAsync(x => x.TagId == id, ct);

    public Task<List<Tag>> GetByIdsAsync(AIVESDbContext db, IReadOnlyCollection<int> ids, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(ids);
        var distinctIds = ids.Distinct().ToArray();
        return db.Tags.AsNoTracking().Where(x => distinctIds.Contains(x.TagId))
            .OrderBy(x => x.TagName).ThenBy(x => x.TagId).ToListAsync(ct);
    }

    public Task<bool> IsUsedAsync(AIVESDbContext db, int id, CancellationToken ct) =>
        db.NewsTags.AnyAsync(x => x.TagId == id, ct);

    public async Task<DeleteResult> DeleteAsync(AIVESDbContext db, int id, CancellationToken ct)
    {
        var entity = await db.Tags.SingleOrDefaultAsync(x => x.TagId == id, ct);
        if (entity is null) return DeleteResult.NotFound;
        if (await IsUsedAsync(db, id, ct)) return DeleteResult.InUse;
        return await Persistence.DeleteAsync(db, entity, ct);
    }
}
