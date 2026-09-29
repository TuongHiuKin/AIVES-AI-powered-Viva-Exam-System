using AIVES.BLL.Interfaces.ExamReporting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace StudentNameMVC.Controllers;

[Authorize]
public class ClassReportsController : Controller
{
    private readonly IClassReportService _classReportService;
    private readonly IExamReportRepository _examReportRepository;

    public ClassReportsController(
        IClassReportService classReportService,
        IExamReportRepository examReportRepository)
    {
        _classReportService = classReportService ?? throw new ArgumentNullException(nameof(classReportService));
        _examReportRepository = examReportRepository ?? throw new ArgumentNullException(nameof(examReportRepository));
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        string? teacherId = "GV01",
        string? classId = "SE1701",
        string? examId = "EXAM01",
        int? attemptOrdinal = 1,
        CancellationToken ct = default)
    {
        teacherId ??= "GV01";
        classId ??= "SE1701";
        examId ??= "EXAM01";

        var exams = await _examReportRepository.GetAvailableExamsAsync(ct);
        var availableClasses = await _examReportRepository.GetAvailableClassesForTeacherAsync(teacherId, ct);

        ViewBag.AvailableExams = exams;
        ViewBag.AvailableClasses = availableClasses;
        ViewBag.SelectedTeacherId = teacherId;
        ViewBag.SelectedClassId = classId;
        ViewBag.SelectedExamId = examId;
        ViewBag.SelectedAttemptOrdinal = attemptOrdinal;

        try
        {
            var stats = await _classReportService.GetClassStatisticsAsync(
                teacherId,
                classId,
                examId,
                attemptOrdinal,
                ct);

            return View(stats);
        }
        catch (UnauthorizedAccessException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return View(null);
        }
    }
}
