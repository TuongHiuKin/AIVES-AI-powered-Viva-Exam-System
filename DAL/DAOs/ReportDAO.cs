using AIVES.MVC.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace AIVES.DAL.DAOs
{
    public class ReportDAO
    {
        private static readonly Lazy<ReportDAO> _instance = new(() => new ReportDAO());
        public static ReportDAO Instance => _instance.Value;

        private ReportDAO() { }

        public async Task<List<ReportDataItem>> GetReportDataAsync(AIVESDbContext context, DateTime startDate, DateTime endDate)
        {
            return await context.NewsArticles
                .Where(n => n.CreatedDate >= startDate && n.CreatedDate < endDate.AddDays(1))
                .OrderByDescending(n => n.CreatedDate)
                .ThenByDescending(n => n.NewsArticleId)
                .Select(n => new ReportDataItem
                {
                    NewsArticleId = n.NewsArticleId,
                    Title = n.NewsTitle,
                    Author = n.CreatedBy.AccountName,
                    CreatedDate = n.CreatedDate
                })
                .ToListAsync();
        }
    }
}