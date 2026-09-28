namespace AIVES.BLL.Options;

public sealed class DefaultAdminOptions
{
    public const string SectionName = "DefaultAdmin";

    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
