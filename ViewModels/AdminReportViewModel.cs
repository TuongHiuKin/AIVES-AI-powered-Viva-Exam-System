using System.ComponentModel.DataAnnotations;

namespace AIVES.MVC.ViewModels
{
    public class AdminReportViewModel
    {
        [Required]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        public List<ReportDataItem> ReportData { get; set; } = new();
    }

    public class ReportDataItem
    {
        public int NewsArticleId { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}