namespace AIVES.DAL.Entities;

public class NewsArticle
{
    public int NewsArticleId { get; set; }
    public string NewsTitle { get; set; } = string.Empty;
    public string NewsContent { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    // 0 = Inactive, 1 = Active. The Service must validate explicit user input.
    public byte NewsStatus { get; set; }
    public int CreatedById { get; set; }
    public DateTime CreatedDate { get; set; }
    public int? UpdatedById { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public Category Category { get; set; } = null!;
    public SystemAccount CreatedBy { get; set; } = null!;
    public SystemAccount? UpdatedBy { get; set; }
    public ICollection<NewsTag> NewsTags { get; set; } = new List<NewsTag>();
}
