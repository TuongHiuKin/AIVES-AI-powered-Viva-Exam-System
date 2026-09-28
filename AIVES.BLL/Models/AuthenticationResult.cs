namespace AIVES.BLL.Models;

public sealed record AuthenticatedUser(
    string SubjectId,
    string DisplayName,
    string Email,
    string Role);

public sealed record AuthenticationResult(bool Succeeded, AuthenticatedUser? User)
{
    public static AuthenticationResult Failure { get; } = new(false, null);

    public static AuthenticationResult Success(
        string subjectId,
        string displayName,
        string email,
        string role) => new(
            true,
            new AuthenticatedUser(subjectId, displayName, email, role));
}
