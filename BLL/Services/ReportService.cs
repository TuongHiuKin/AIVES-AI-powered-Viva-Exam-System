using AIVES.BLL.Interfaces;
using AIVES.DAL.Repositories.Interfaces;
using AIVES.MVC.ViewModels;

namespace AIVES.BLL.Services
{
    public class ReportService : IReportService
    {
        private readonly IReportRepository _reportRepository;

        public ReportService(IReportRepository reportRepository)
        {
            _reportRepository = reportRepository;
        }

        public async Task<List<ReportDataItem>> GenerateReportAsync(DateTime startDate, DateTime endDate)
        {
            if (startDate > endDate)
                throw new ArgumentException("StartDate must be less than or equal to EndDate.");

            return await _reportRepository.GetReportDataAsync(startDate, endDate);
        }
    }
}