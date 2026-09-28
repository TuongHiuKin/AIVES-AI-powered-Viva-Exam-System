using AIVES.MVC.ViewModels;

namespace AIVES.BLL.Interfaces
{
    public interface IReportService
    {
        Task<List<ReportDataItem>> GenerateReportAsync(DateTime startDate, DateTime endDate);
    }
}