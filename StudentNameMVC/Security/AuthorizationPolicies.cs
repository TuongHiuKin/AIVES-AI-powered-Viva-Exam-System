namespace StudentNameMVC.Security;

public static class AuthorizationPolicies
{
    public const string AdminOnly = "AdminOnly";
    public const string StaffOnly = "StaffOnly";
    public const string LecturerOnly = "LecturerOnly";
}
