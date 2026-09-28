using System.ComponentModel.DataAnnotations;

namespace StudentNameMVC.ViewModels;

public class ReportItemViewModel
{
    public int NewsArticleId { get; set; }
    public string NewsTitle { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public byte NewsStatus { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; } // UTC
    public DateTime CreatedDateLocal => CreatedDate.AddHours(7); // Giờ Việt Nam UTC+7
    public List<string> TagNames { get; set; } = new();
}

public class ReportIndexViewModel
{
    [Display(Name = "Từ ngày")]
    [DataType(DataType.Date)]
    public DateTime? StartDate { get; set; }

    [Display(Name = "Đến ngày")]
    [DataType(DataType.Date)]
    public DateTime? EndDate { get; set; }

    public List<ReportItemViewModel> Articles { get; set; } = new();

    public int TotalArticles => Articles.Count;
    public int ActiveArticles => Articles.Count(a => a.NewsStatus == 1);
    public int InactiveArticles => Articles.Count(a => a.NewsStatus == 0);
}
