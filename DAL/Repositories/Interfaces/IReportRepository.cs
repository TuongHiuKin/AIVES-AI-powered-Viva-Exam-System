using AIVES.MVC.ViewModels;

namespace AIVES.DAL.Repositories.Interfaces
{
    public interface IReportRepository
    {
        Task<List<ReportDataItem>> GetReportDataAsync(DateTime startDate, DateTime endDate);
    }
}