namespace AIVES.DAL.Entities;

public class Category
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? CategoryDescription { get; set; }
    public ICollection<NewsArticle> NewsArticles { get; set; } = new List<NewsArticle>();
}
