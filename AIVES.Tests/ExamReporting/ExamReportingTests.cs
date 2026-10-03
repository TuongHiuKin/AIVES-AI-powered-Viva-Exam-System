using System.Security.Claims;
using AIVES.BLL.Interfaces.ExamReporting;
using AIVES.BLL.Models.ExamReporting;
using AIVES.BLL.Repositories.ExamReporting;
using AIVES.BLL.Security;
using AIVES.BLL.Services.ExamReporting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StudentNameMVC.Controllers;
using Xunit;

namespace AIVES.Tests.ExamReporting;

public class ExamReportingTests
{
    private readonly IExamReportRepository _repository;
    private readonly StudentReportService _studentService;
    private readonly ClassReportService _classService;

    public ExamReportingTests()
    {
        _repository = new MockExamReportRepository();
        _studentService = new StudentReportService(_repository);
        _classService = new ClassReportService(_repository);
    }

    [Fact]
    public async Task StudentReport_Counts_Only_Completed_Attempts_And_Excludes_Abandoned_DEC01()
    {
        // SV03 has 1 completed & graded, 1 completed & pending, 1 abandoned (total 3 in db)
        var report = await _studentService.GetStudentReportAsync("SV03", "EXAM01");

        Assert.NotNull(report);
        // DEC-01: Chỉ lượt Completed mới tính vào số lượt hoàn thành (1 + 1 = 2), bỏ dở (Abandoned) không được tính
        Assert.Equal(2, report.CompletedAttemptsCount);
        Assert.Equal(3, report.MaxAllowedAttemptsCount);
        Assert.Equal(1, report.GradedAttemptsCount);
        Assert.Equal(1, report.PendingAttemptsCount);
    }

    [Fact]
    public async Task StudentReport_Calculates_Average_Only_Over_Graded_Completed_Attempts_DEC05_DEC10()
    {
        // SV01 has 2 completed and fully graded attempts:
        // Attempt 1: Q1=8, Q2=4, Q3=5 => Total=17/30 => Normalized=5.67
        // Attempt 2: Q1=9, Q2=8, Q3=7 => Total=24/30 => Normalized=8.00
        // Expected Average = (5.67 + 8.00) / 2 = 6.84
        var report = await _studentService.GetStudentReportAsync("SV01", "EXAM01");

        Assert.NotNull(report);
        Assert.Equal(2, report.GradedAttemptsCount);
        Assert.NotNull(report.AverageScore);
        Assert.Equal(6.84m, report.AverageScore.Value);
        // DEC-05: Mẫu số chia là số lượt có kết quả đầy đủ (2), KHÔNG chia cho số lượt tối đa (3)
    }

    [Fact]
    public async Task StudentReport_Evaluates_PassFail_Mode_With_Min_Pass_Percentage_And_Mandatory_Criteria_DEC04()
    {
        var fakeRepo = new FakeExamReportRepository();
        var examSettings = new ExamSettingsSnapshot
        {
            ExamId = "PASSFAIL_EXAM",
            ExamTitle = "Bài thi Pass/Fail",
            EvaluationMode = ExamEvaluationMode.PassFail,
            MinPassPercentage = 66.6m,
            RequiredCriteriaKeys = new List<string> { "MandatoryCrit" }
        };
        fakeRepo.Exams.Add(examSettings);

        // Attempt A: 2/3 questions passed (66.7% >= 66.6%), BUT MandatoryCrit is false => FAIL
        fakeRepo.Attempts.Add(new AttemptSnapshotDto
        {
            AttemptId = "ATT-PF-1",
            StudentId = "SV99",
            ExamId = "PASSFAIL_EXAM",
            AttemptOrdinal = 1,
            ExamState = ExamState.Completed,
            GradingState = GradingState.Graded,
            QuestionResults = new List<QuestionResultDto>
            {
                new() { QuestionId = "Q1", IsPassed = true, CriteriaOutcomes = new() { { "MandatoryCrit", false } } },
                new() { QuestionId = "Q2", IsPassed = true, CriteriaOutcomes = new() },
                new() { QuestionId = "Q3", IsPassed = false, CriteriaOutcomes = new() }
            }
        });

        var service = new StudentReportService(fakeRepo);
        var report = await service.GetStudentReportAsync("SV99", "PASSFAIL_EXAM");

        Assert.NotNull(report);
        Assert.Single(report.Attempts);
        Assert.False(report.Attempts[0].IsPassed, "Lượt thi phải Không đạt vì không thỏa tiêu chí bắt buộc (DEC-04).");
    }

    [Fact]
    public async Task ClassReport_Throws_Unauthorized_When_Teacher_Not_Assigned_To_Class_DEC11()
    {
        // GV02 is assigned to SE1702, NOT SE1701
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _classService.GetClassStatisticsAsync("GV02", "SE1701", "EXAM01"));
    }

    [Fact]
    public async Task ClassReport_Identifies_Hardest_Questions_With_Highest_Fail_Rate_And_Includes_All_Ties_DEC09()
    {
        // For SE1701 on Attempt 1:
        // SV01: Q1=8 (Đạt), Q2=4 (Không đạt), Q3=5 (Không đạt) (ngưỡng 6.0)
        // SV02: Q1=4 (Không đạt), Q2=2 (Không đạt), Q3=3 (Không đạt)
        // SV03: Q1=6 (Đạt), Q2=6 (Đạt), Q3=7 (Đạt)
        // Total Assessed for each: 3
        // Q1: Passed=2, Failed=1 => FailRate = 33.33%
        // Q2: Passed=1, Failed=2 => FailRate = 66.67%
        // Q3: Passed=1, Failed=2 => FailRate = 66.67%
        // DEC-09: Q2 and Q3 are tied for hardest! Both must be present in HardestQuestions list!
        var stats = await _classService.GetClassStatisticsAsync("GV01", "SE1701", "EXAM01", specificAttemptOrdinal: 1);

        Assert.NotNull(stats);
        Assert.Equal(2, stats.HardestQuestions.Count);
        Assert.Contains(stats.HardestQuestions, q => q.QuestionId == "Q2");
        Assert.Contains(stats.HardestQuestions, q => q.QuestionId == "Q3");
        Assert.Equal(66.67m, stats.HardestQuestions[0].FailRate);
        Assert.Equal(66.67m, stats.HardestQuestions[1].FailRate);
    }

    [Fact]
    public async Task ClassReport_Orders_Best_Answered_Questions_By_Pass_Rate_Descending_DEC09()
    {
        var stats = await _classService.GetClassStatisticsAsync("GV01", "SE1701", "EXAM01", specificAttemptOrdinal: 1);

        Assert.NotNull(stats);
        // Q1 has PassRate = 66.67%, Q2 and Q3 have PassRate = 33.33%
        // Rank 1 must be Q1
        Assert.Equal("Q1", stats.QuestionStats[0].QuestionId);
        Assert.Equal(1, stats.QuestionStats[0].Rank);
        Assert.Equal(66.67m, stats.QuestionStats[0].PassRate);
    }

    [Fact]
    public async Task ClassReport_Distribution_Counts_Each_Student_Exactly_Once_On_X_Axis_DEC06()
    {
        // SE1701 has 3 students:
        // SV01 average = (5.67 + 8.00) / 2 = 6.8
        // SV02 average = (3.00) / 1 = 3.0
        // SV03 average = (6.33) / 1 = 6.3
        // DEC-06: Each student counted once, sum of student counts in bins must equal 3
        var stats = await _classService.GetClassStatisticsAsync("GV01", "SE1701", "EXAM01");

        Assert.NotNull(stats);
        Assert.Equal(3, stats.TotalStudentsCount);
        var totalCountInBins = stats.ScoreDistribution.Sum(bin => bin.StudentCount);
        Assert.Equal(3, totalCountInBins);
    }

    [Fact]
    public async Task ClassReport_Exports_Valid_Csv_With_Utf8_Bom_And_Student_Data()
    {
        var csvBytes = await _classService.ExportClassGradeSheetCsvAsync("GV01", "SE1701", "EXAM01");

        Assert.NotNull(csvBytes);
        Assert.NotEmpty(csvBytes);

        // UTF-8 BOM check: 0xEF, 0xBB, 0xBF
        Assert.Equal(0xEF, csvBytes[0]);
        Assert.Equal(0xBB, csvBytes[1]);
        Assert.Equal(0xBF, csvBytes[2]);

        var csvContent = System.Text.Encoding.UTF8.GetString(csvBytes);
        Assert.Contains("Mã sinh viên", csvContent);
        Assert.Contains("SV01", csvContent);
        Assert.Contains("SV02", csvContent);
        Assert.Contains("SV03", csvContent);
        Assert.Contains("Nguyễn Văn A", csvContent);
    }

    [Fact]
    public async Task GetStudentExamDetail_AuthorizedTeacher_ReturnsReportWithAttempts()
    {
        // GV01 is assigned to SE1701, SV01 is enrolled in SE1701
        var report = await _classService.GetStudentExamDetailForTeacherAsync("GV01", "SE1701", "EXAM01", "SV01");

        Assert.NotNull(report);
        Assert.Equal("SV01", report.StudentId);
        Assert.Equal("Nguyễn Văn A", report.StudentName);
        Assert.Equal(2, report.CompletedAttemptsCount);
        Assert.Equal(2, report.Attempts.Count);
        Assert.NotNull(report.AverageScore);
        Assert.Equal(6.84m, report.AverageScore.Value);

        // Check attempt details
        var attempt1 = report.Attempts.FirstOrDefault(a => a.AttemptOrdinal == 1);
        Assert.NotNull(attempt1);
        Assert.Equal(3, attempt1.QuestionResults.Count);
        Assert.Equal("Q1", attempt1.QuestionResults[0].QuestionId);
        Assert.Equal(8.0m, attempt1.QuestionResults[0].TeacherFinalScore);
        Assert.Equal(7.5m, attempt1.QuestionResults[0].AiSuggestedScore);
    }

    [Fact]
    public async Task GetStudentExamDetail_UnauthorizedTeacher_ThrowsUnauthorizedAccessException_DEC11()
    {
        // GV02 is assigned to SE1702, NOT SE1701
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _classService.GetStudentExamDetailForTeacherAsync("GV02", "SE1701", "EXAM01", "SV01"));
    }

    [Fact]
    public async Task GetStudentExamDetail_StudentNotInClass_ThrowsInvalidOperationException()
    {
        // GV01 is authorized for SE1701, but SV999 is not in SE1701
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _classService.GetStudentExamDetailForTeacherAsync("GV01", "SE1701", "EXAM01", "SV999"));
    }

    [Theory]
    [InlineData("", "SE1701", "EXAM01", "SV01")]
    [InlineData("GV01", "", "EXAM01", "SV01")]
    [InlineData("GV01", "SE1701", "", "SV01")]
    [InlineData("GV01", "SE1701", "EXAM01", "")]
    public async Task GetStudentExamDetail_EmptyArguments_ThrowsArgumentException(
        string teacherId, string classId, string examId, string studentId)
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _classService.GetStudentExamDetailForTeacherAsync(teacherId, classId, examId, studentId));
    }

    [Fact]
    public async Task StudentReport_Propagates_EvaluationMode_Correctly_To_Attempts_And_Questions()
    {
        // 1. Score Mode Test (EXAM01)
        var scoreReport = await _studentService.GetStudentReportAsync("SV01", "EXAM01");
        Assert.NotNull(scoreReport);
        Assert.Equal(ExamEvaluationMode.Score, scoreReport.EvaluationMode);
        Assert.All(scoreReport.Attempts, a =>
        {
            Assert.Equal(ExamEvaluationMode.Score, a.EvaluationMode);
            Assert.All(a.QuestionResults, q => Assert.Equal(ExamEvaluationMode.Score, q.EvaluationMode));
        });

        // 2. Pass/Fail Mode Test (EXAM02)
        var passFailReport = await _studentService.GetStudentReportAsync("SV01", "EXAM02");
        Assert.NotNull(passFailReport);
        Assert.Equal(ExamEvaluationMode.PassFail, passFailReport.EvaluationMode);
        Assert.Null(passFailReport.AverageScore); // Pass/Fail mode does not compute numerical average score
        Assert.All(passFailReport.Attempts, a =>
        {
            Assert.Equal(ExamEvaluationMode.PassFail, a.EvaluationMode);
            Assert.All(a.QuestionResults, q => Assert.Equal(ExamEvaluationMode.PassFail, q.EvaluationMode));
        });
    }

    [Fact]
    public async Task StudentReportService_GetAvailableExamsAsync_DelegatesToRepository()
    {
        var exams = await _studentService.GetAvailableExamsAsync();
        Assert.NotNull(exams);
        Assert.NotEmpty(exams);
    }

    [Fact]
    public async Task ClassReportService_GetAvailableExamsAndClasses_DelegatesToRepository()
    {
        var exams = await _classService.GetAvailableExamsAsync();
        var classes = await _classService.GetAvailableClassesForTeacherAsync("GV01");

        Assert.NotNull(exams);
        Assert.NotEmpty(exams);
        Assert.NotNull(classes);
        Assert.NotEmpty(classes);
    }

    [Fact]
    public async Task StudentReportsController_StudentCannotViewOtherStudentReport_ReturnsForbid()
    {
        var controller = new StudentReportsController(_studentService);
        var principal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "SV01"),
            new Claim(ClaimTypes.Email, "student1@aives.test"),
            new Claim(ClaimTypes.Role, ApplicationRoles.Staff)
        }, "TestAuth"));

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        // Sinh viên SV01 cố tình truyền studentId="SV02" để xem bài của bạn
        var result = await controller.Index(studentId: "SV02", examId: "EXAM01");

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task StudentReportsController_StudentCanViewOwnReport_ReturnsViewResult()
    {
        var controller = new StudentReportsController(_studentService);
        var principal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "SV01"),
            new Claim(ClaimTypes.Email, "student1@aives.test"),
            new Claim(ClaimTypes.Role, ApplicationRoles.Staff)
        }, "TestAuth"));

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        // Sinh viên xem bài thi của chính mình (hoặc truyền null)
        var result = await controller.Index(studentId: null, examId: "EXAM01");

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<StudentReportDto>(viewResult.Model);
        Assert.Equal("SV01", model.StudentId);
    }

    [Fact]
    public async Task StudentReportsController_AdminCanViewAnyStudentReport_ReturnsViewResult()
    {
        var controller = new StudentReportsController(_studentService);
        var principal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "admin"),
            new Claim(ClaimTypes.Email, "admin@aives.test"),
            new Claim(ClaimTypes.Role, ApplicationRoles.Admin)
        }, "TestAuth"));

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        // Admin có thể xem bài của bất kỳ sinh viên nào (SV02)
        var result = await controller.Index(studentId: "SV02", examId: "EXAM01");

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<StudentReportDto>(viewResult.Model);
        Assert.Equal("SV02", model.StudentId);
    }

    [Fact]
    public void ReportingControllers_DoNotDependOnExamReportRepository()
    {
        // Kiểm tra kiến trúc: Controller không được nhận IExamReportRepository trong constructor
        var studentCtors = typeof(StudentReportsController).GetConstructors();
        Assert.All(studentCtors, c =>
            Assert.DoesNotContain(c.GetParameters(), p => p.ParameterType == typeof(IExamReportRepository)));

        var classCtors = typeof(ClassReportsController).GetConstructors();
        Assert.All(classCtors, c =>
            Assert.DoesNotContain(c.GetParameters(), p => p.ParameterType == typeof(IExamReportRepository)));
    }

    private class FakeExamReportRepository : IExamReportRepository
    {
        public List<ExamSettingsSnapshot> Exams { get; } = new();
        public List<AttemptSnapshotDto> Attempts { get; } = new();

        public Task<List<AttemptSnapshotDto>> GetStudentAttemptsAsync(string studentId, string examId, CancellationToken ct = default) =>
            Task.FromResult(Attempts.Where(a => a.StudentId == studentId && a.ExamId == examId).ToList());

        public Task<List<AttemptSnapshotDto>> GetClassAttemptsAsync(string classId, string examId, CancellationToken ct = default) =>
            Task.FromResult(Attempts.Where(a => a.ClassId == classId && a.ExamId == examId).ToList());

        public Task<bool> IsTeacherAssignedToClassAsync(string teacherId, string classId, CancellationToken ct = default) =>
            Task.FromResult(true);

        public Task<ExamSettingsSnapshot?> GetExamSettingsAsync(string examId, CancellationToken ct = default) =>
            Task.FromResult(Exams.FirstOrDefault(e => e.ExamId == examId));

        public Task<List<string>> GetAvailableClassesForTeacherAsync(string teacherId, CancellationToken ct = default) =>
            Task.FromResult(new List<string> { "TEST_CLASS" });

        public Task<List<ExamSettingsSnapshot>> GetAvailableExamsAsync(CancellationToken ct = default) =>
            Task.FromResult(Exams);
    }
}
