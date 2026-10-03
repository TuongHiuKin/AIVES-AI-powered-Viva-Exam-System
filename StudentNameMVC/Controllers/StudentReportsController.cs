using System.Security.Claims;
using AIVES.BLL.Interfaces.ExamReporting;
using AIVES.BLL.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace StudentNameMVC.Controllers;

[Authorize(Roles = $"{ApplicationRoles.Admin},{ApplicationRoles.Staff}")]
public class StudentReportsController : Controller
{
    private readonly IStudentReportService _studentReportService;

    public StudentReportsController(IStudentReportService studentReportService)
    {
        _studentReportService = studentReportService ?? throw new ArgumentNullException(nameof(studentReportService));
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        string? studentId = null,
        string? examId = "EXAM01",
        CancellationToken ct = default)
    {
        examId = string.IsNullOrWhiteSpace(examId) ? "EXAM01" : examId;

        var resolvedStudentId = ResolveStudentId(studentId);

        // DEC-11 & Security: Chặn sinh viên (Staff) tự ý đổi studentId trên URL để xem bài của người khác (chống IDOR)
        if (!User.IsInRole(ApplicationRoles.Admin) && !string.IsNullOrWhiteSpace(studentId) &&
            !string.Equals(studentId, resolvedStudentId, StringComparison.OrdinalIgnoreCase))
        {
            return Forbid();
        }

        var exams = await _studentReportService.GetAvailableExamsAsync(ct);
        ViewBag.AvailableExams = exams;
        ViewBag.SelectedExamId = examId;
        ViewBag.SelectedStudentId = resolvedStudentId;

        var report = await _studentReportService.GetStudentReportAsync(resolvedStudentId, examId, ct);
        if (report == null)
        {
            TempData["ErrorMessage"] = $"Không tìm thấy thông tin bài thi {examId}.";
            return View();
        }

        return View(report);
    }

    private string ResolveStudentId(string? requestedStudentId = null)
    {
        // Admin có thể xem thay mặt sinh viên bất kỳ
        if (User.IsInRole(ApplicationRoles.Admin) && !string.IsNullOrWhiteSpace(requestedStudentId))
        {
            return requestedStudentId;
        }

        // Lấy định danh từ Claims của tài khoản sinh viên đang đăng nhập
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        if (!string.IsNullOrWhiteSpace(email))
        {
            if (email.Contains("sv02", StringComparison.OrdinalIgnoreCase) || email.Contains("student2", StringComparison.OrdinalIgnoreCase))
                return "SV02";
            if (email.Contains("sv03", StringComparison.OrdinalIgnoreCase) || email.Contains("student3", StringComparison.OrdinalIgnoreCase))
                return "SV03";
            if (email.Contains("sv01", StringComparison.OrdinalIgnoreCase) || email.Contains("student", StringComparison.OrdinalIgnoreCase))
                return "SV01";
        }

        var subId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrWhiteSpace(subId) && subId.StartsWith("SV", StringComparison.OrdinalIgnoreCase))
        {
            return subId.ToUpperInvariant();
        }

        if (User.IsInRole(ApplicationRoles.Admin))
        {
            return !string.IsNullOrWhiteSpace(requestedStudentId) ? requestedStudentId : "SV01";
        }

        return "SV01";
    }
}
