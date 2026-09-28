using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AIVES.BLL.Interfaces;
using AIVES.MVC.ViewModels;

namespace AIVES.MVC.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ReportController : Controller
    {
        private readonly IReportService _reportService;

        public ReportController(IReportService reportService)
        {
            _reportService = reportService;
        }

        public IActionResult AdminReport()
        {
            return View(new AdminReportViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdminReport(AdminReportViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            model.ReportData = await _reportService.GenerateReportAsync(model.StartDate, model.EndDate);
            return View(model);
        }
    }
}