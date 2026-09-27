namespace AIVES.DAL.Entities;

public class NewsTag
{
    public int NewsArticleId { get; set; }
    public int TagId { get; set; }
    public NewsArticle NewsArticle { get; set; } = null!;
    public Tag Tag { get; set; } = null!;
}
