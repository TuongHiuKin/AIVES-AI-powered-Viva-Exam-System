namespace AIVES.DAL.Entities;

public class SystemAccount
{
    public int AccountId { get; set; }
    public string AccountName { get; set; } = string.Empty;
    public string AccountEmail { get; set; } = string.Empty;
    public string AccountPasswordHash { get; set; } = string.Empty;
    // Database roles: 1 = Staff, 2 = Lecturer. Admin is configuration-only.
    public byte AccountRole { get; set; }
    public bool IsDeleted { get; set; }
    public ICollection<NewsArticle> CreatedNewsArticles { get; set; } = new List<NewsArticle>();
    public ICollection<NewsArticle> UpdatedNewsArticles { get; set; } = new List<NewsArticle>();
}
