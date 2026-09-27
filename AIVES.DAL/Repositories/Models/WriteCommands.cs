namespace AIVES.DAL.Repositories.Models;

// Application-facing persistence contracts, not MVC form models.
// BLL validates authorization/input and supplies hashes and trusted audit identity.
public sealed record AccountCreate(string Name, string Email, string PasswordHash, byte Role);
public sealed record AccountUpdate(int Id, string Name, string Email, byte Role, string? NewPasswordHash = null);
public sealed record ProfileUpdate(int Id, string Name, string Email, string? NewPasswordHash = null);
public sealed record NewsCreate(string Title, string Content, int CategoryId, byte Status,
    int CreatedById, IReadOnlyCollection<int> TagIds);
public sealed record NewsUpdate(int Id, string Title, string Content, int CategoryId, byte Status,
    int UpdatedById, DateTime ModifiedUtc, IReadOnlyCollection<int> TagIds);

public enum DeleteResult { Deleted, NotFound, InUse }
