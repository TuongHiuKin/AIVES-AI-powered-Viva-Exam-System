using AIVES.DAL.Context;
using AIVES.DAL.Repositories.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace AIVES.DAL.DAOs;

internal static class Persistence
{
    // Repository writes are complete operations. Never stage unrelated changes on
    // the shared context. After a failed save, discard pending state before reuse.
    public static async Task SaveAsync(AIVESDbContext db, CancellationToken ct)
    {
        try { await db.SaveChangesAsync(ct); }
        catch
        {
            db.ChangeTracker.Clear();
            throw;
        }
    }

    public static async Task<DeleteResult> DeleteAsync(AIVESDbContext db, object entity, CancellationToken ct)
    {
        db.Remove(entity);
        try
        {
            await SaveAsync(db, ct);
            return DeleteResult.Deleted;
        }
        // FK protects against a new reference appearing after the usage check.
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 547 })
        {
            return DeleteResult.InUse;
        }
    }
}
