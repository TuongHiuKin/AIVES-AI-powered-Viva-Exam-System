using AIVES.DAL.Repositories.Interfaces;
using AIVES.DAL.DAOs;
using AIVES.MVC.ViewModels;

namespace AIVES.DAL.Repositories
{
    public class ReportRepository : IReportRepository
    {
        private readonly AIVESDbContext _context;

        public ReportRepository(AIVESDbContext context)
        {
            _context = context;
        }

        public async Task<List<ReportDataItem>> GetReportDataAsync(DateTime startDate, DateTime endDate)
        {
            return await ReportDAO.Instance.GetReportDataAsync(_context, startDate, endDate);
        }
    }
}