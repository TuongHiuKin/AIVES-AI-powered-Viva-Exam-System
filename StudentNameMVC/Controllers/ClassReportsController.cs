using System.Security.Claims;
using AIVES.BLL.Interfaces.ExamReporting;
using AIVES.BLL.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace StudentNameMVC.Controllers;

[Authorize(Roles = $"{ApplicationRoles.Admin},{ApplicationRoles.Lecturer}")]
public class ClassReportsController : Controller
{
    private readonly IClassReportService _classReportService;

    public ClassReportsController(IClassReportService classReportService)
    {
        _classReportService = classReportService ?? throw new ArgumentNullException(nameof(classReportService));
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        string? teacherId = null,
        string? classId = null,
        string? examId = null,
        int? attemptOrdinal = 1,
        CancellationToken ct = default)
    {
        var resolvedTeacherId = ResolveTeacherId(teacherId);
        classId = string.IsNullOrWhiteSpace(classId) ? "SE1701" : classId;
        examId = string.IsNullOrWhiteSpace(examId) ? "EXAM01" : examId;

        var exams = await _classReportService.GetAvailableExamsAsync(ct);
        var availableClasses = await _classReportService.GetAvailableClassesForTeacherAsync(resolvedTeacherId, ct);

        ViewBag.AvailableExams = exams;
        ViewBag.AvailableClasses = availableClasses;
        ViewBag.SelectedTeacherId = resolvedTeacherId;
        ViewBag.SelectedClassId = classId;
        ViewBag.SelectedExamId = examId;
        ViewBag.SelectedAttemptOrdinal = attemptOrdinal;

        try
        {
            var stats = await _classReportService.GetClassStatisticsAsync(
                resolvedTeacherId,
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

    [HttpGet]
    public async Task<IActionResult> StudentDetail(
        string studentId,
        string classId,
        string examId,
        int? attemptOrdinal = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(studentId) || string.IsNullOrWhiteSpace(classId) || string.IsNullOrWhiteSpace(examId))
        {
            TempData["ErrorMessage"] = "Thông tin truy vấn không hợp lệ.";
            return RedirectToAction(nameof(Index));
        }

        var teacherId = ResolveTeacherId();
        ViewBag.SelectedTeacherId = teacherId;
        ViewBag.SelectedClassId = classId;
        ViewBag.SelectedExamId = examId;
        ViewBag.SelectedAttemptOrdinal = attemptOrdinal;

        try
        {
            var report = await _classReportService.GetStudentExamDetailForTeacherAsync(
                teacherId,
                classId,
                examId,
                studentId,
                ct);

            return View(report);
        }
        catch (UnauthorizedAccessException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Index), new { classId, examId, attemptOrdinal });
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Index), new { classId, examId, attemptOrdinal });
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Lỗi khi tải chi tiết bài thi: {ex.Message}";
            return RedirectToAction(nameof(Index), new { classId, examId, attemptOrdinal });
        }
    }

    [HttpGet]
    public async Task<IActionResult> ExportGradeSheet(
        string? teacherId = null,
        string? classId = null,
        string? examId = null,
        CancellationToken ct = default)
    {
        var resolvedTeacherId = ResolveTeacherId(teacherId);
        classId = string.IsNullOrWhiteSpace(classId) ? "SE1701" : classId;
        examId = string.IsNullOrWhiteSpace(examId) ? "EXAM01" : examId;

        try
        {
            var fileBytes = await _classReportService.ExportClassGradeSheetCsvAsync(resolvedTeacherId, classId, examId, ct);
            var fileName = $"BangDiem_Lop_{classId}_{examId}.csv";
            return File(fileBytes, "text/csv; charset=utf-8", fileName);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Lỗi khi xuất bảng điểm: {ex.Message}";
            return RedirectToAction(nameof(Index), new { classId, examId });
        }
    }

    private string ResolveTeacherId(string? requestedTeacherId = null)
    {
        // Admin có thể xem thay mặt giảng viên nếu có requestedTeacherId
        if (User.IsInRole(ApplicationRoles.Admin) && !string.IsNullOrWhiteSpace(requestedTeacherId))
        {
            return requestedTeacherId;
        }

        // Lấy định danh từ Claims của tài khoản đăng nhập (DEC-11)
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        if (!string.IsNullOrWhiteSpace(email))
        {
            if (email.Contains("lecturer2", StringComparison.OrdinalIgnoreCase) || email.Contains("gv02", StringComparison.OrdinalIgnoreCase))
            {
                return "GV02";
            }
            if (email.Contains("lecturer", StringComparison.OrdinalIgnoreCase) || email.Contains("gv01", StringComparison.OrdinalIgnoreCase))
            {
                return "GV01";
            }
        }

        var subId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrWhiteSpace(subId))
        {
            if (subId.StartsWith("GV", StringComparison.OrdinalIgnoreCase))
            {
                return subId.ToUpperInvariant();
            }
        }

        // Nếu là Admin và không khớp claim giảng viên cụ thể
        if (User.IsInRole(ApplicationRoles.Admin) && !string.IsNullOrWhiteSpace(requestedTeacherId))
        {
            return requestedTeacherId;
        }

        // Mặc định cho mock/development
        return "GV01";
    }
}
