namespace AIVES.BLL.Security;

public static class ApplicationRoles
{
    public const string Admin = "Admin";
    public const string Staff = "Staff";
    public const string Lecturer = "Lecturer";
    public const string AdminSubjectId = "admin";

    public static string? FromDatabaseRole(byte role) => role switch
    {
        1 => Staff,
        2 => Lecturer,
        _ => null
    };
}
