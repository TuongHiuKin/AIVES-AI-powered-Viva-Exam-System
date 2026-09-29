using AIVES.BLL.Interfaces.ExamReporting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace StudentNameMVC.Controllers;

[Authorize]
public class StudentReportsController : Controller
{
    private readonly IStudentReportService _studentReportService;
    private readonly IExamReportRepository _examReportRepository;

    public StudentReportsController(
        IStudentReportService studentReportService,
        IExamReportRepository examReportRepository)
    {
        _studentReportService = studentReportService ?? throw new ArgumentNullException(nameof(studentReportService));
        _examReportRepository = examReportRepository ?? throw new ArgumentNullException(nameof(examReportRepository));
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        string? studentId = "SV01",
        string? examId = "EXAM01",
        CancellationToken ct = default)
    {
        studentId ??= "SV01";
        examId ??= "EXAM01";

        var exams = await _examReportRepository.GetAvailableExamsAsync(ct);
        ViewBag.AvailableExams = exams;
        ViewBag.SelectedExamId = examId;
        ViewBag.SelectedStudentId = studentId;

        var report = await _studentReportService.GetStudentReportAsync(studentId, examId, ct);
        if (report == null)
        {
            TempData["ErrorMessage"] = $"Không tìm thấy thông tin bài thi {examId}.";
            return View();
        }

        return View(report);
    }
}
