using AIVES.BLL.Interfaces.ExamReporting;
using AIVES.BLL.Models.ExamReporting;

namespace AIVES.BLL.Repositories.ExamReporting;

public class MockExamReportRepository : IExamReportRepository
{
    private readonly List<AttemptSnapshotDto> _attempts = new();
    private readonly List<ExamSettingsSnapshot> _exams = new();
    private readonly Dictionary<string, List<string>> _teacherAssignments = new();

    public MockExamReportRepository()
    {
        SeedMockData();
    }

    private void SeedMockData()
    {
        // 1. Cấu hình bài thi
        _exams.Add(new ExamSettingsSnapshot
        {
            ExamId = "EXAM01",
            ExamTitle = "Thi Vấn Đáp - PRN222 .NET Core & Kiến trúc Ba lớp",
            SettingsVersion = "1.0",
            EvaluationMode = ExamEvaluationMode.Score,
            MaxAllowedAttempts = 3,
            PassThresholdScore = 6.0m, // Ngưỡng đạt từng câu theo rubric GV (DEC-03)
            MinPassPercentage = 60.0m
        });

        _exams.Add(new ExamSettingsSnapshot
        {
            ExamId = "EXAM02",
            ExamTitle = "Thi Vấn Đáp - SWE302 Thiết kế & Kiến trúc Phần mềm",
            SettingsVersion = "1.0",
            EvaluationMode = ExamEvaluationMode.PassFail,
            MaxAllowedAttempts = 2,
            PassThresholdScore = 5.0m,
            MinPassPercentage = 66.6m,
            RequiredCriteriaKeys = new List<string> { "ArchitectureBoundary" }
        });

        // 2. Phân công giảng viên
        _teacherAssignments["GV01"] = new List<string> { "SE1701", "SE1703" };
        _teacherAssignments["GV02"] = new List<string> { "SE1702" };

        // 3. Dữ liệu Lớp SE1701 - Kỳ thi EXAM01
        // SV01: Nguyễn Văn A
        _attempts.Add(new AttemptSnapshotDto
        {
            AttemptId = "ATT-001",
            StudentId = "SV01",
            StudentName = "Nguyễn Văn A",
            ClassId = "SE1701",
            ExamId = "EXAM01",
            AttemptOrdinal = 1,
            ExamState = ExamState.Completed,
            GradingState = GradingState.Graded,
            CompletedAt = DateTime.UtcNow.AddDays(-2),
            QuestionResults = new List<QuestionResultDto>
            {
                new()
                {
                    QuestionId = "Q1",
                    QuestionTitle = "Trình bày vòng đời (Lifetime) của DbContext và cơ chế Singleton DAO thread-safe.",
                    EarnedScore = 8.0m,
                    MaxScore = 10.0m,
                    IsPassed = true,
                    GradingState = GradingState.Graded,
                    StudentAnswer = "DbContext có Scoped lifetime, được tạo mới theo mỗi HTTP request. DAO là Singleton thread-safe và nhận DbContext làm tham số qua phương thức để tránh captive dependency.",
                    AiFeedback = "Câu trả lời xuất sắc. Nắm vững ranh giới giữa Singleton DAO và Scoped DbContext, giải thích đúng cơ chế truyền tham số để tránh lỗi đa luồng.",
                    RubricCriteria = "Hiểu rõ Scoped vs Singleton, nêu đúng giải pháp truyền DbContext vào DAO."
                },
                new()
                {
                    QuestionId = "Q2",
                    QuestionTitle = "Giải thích luồng dữ liệu 3 lớp từ Razor View đến SQL Server và vai trò của BLL.",
                    EarnedScore = 4.0m,
                    MaxScore = 10.0m,
                    IsPassed = false, // < 6.0 => Không đạt theo DEC-03
                    GradingState = GradingState.Graded,
                    StudentAnswer = "View gọi thẳng Repository để lấy dữ liệu nhanh hơn, sau đó đưa vào DbContext.",
                    AiFeedback = "Sai ranh giới kiến trúc. Controller không được gọi trực tiếp Repository/DbContext mà phải đi qua BLL Service để đảm bảo tính toàn vẹn nghiệp vụ.",
                    RubricCriteria = "Tuân thủ nghiêm ngặt View -> Controller -> Service -> Repository -> DAO -> DbContext."
                },
                new()
                {
                    QuestionId = "Q3",
                    QuestionTitle = "Cơ chế bảo mật Cookie Authentication và kiểm tra tài khoản bị xóa (Soft Delete).",
                    EarnedScore = 5.0m,
                    MaxScore = 10.0m,
                    IsPassed = false, // < 6.0 => Không đạt
                    GradingState = GradingState.Graded,
                    StudentAnswer = "Cookie lưu token, khi đăng nhập kiểm tra database 1 lần là đủ.",
                    AiFeedback = "Thiếu cơ chế ActiveAccountCookieEvents để xác thực lại tài khoản trên mỗi request kế tiếp khi Admin thực hiện soft delete.",
                    RubricCriteria = "Trình bày đúng OnValidatePrincipal và xử lý Account.IsDeleted."
                }
            }
        });

        _attempts.Add(new AttemptSnapshotDto
        {
            AttemptId = "ATT-002",
            StudentId = "SV01",
            StudentName = "Nguyễn Văn A",
            ClassId = "SE1701",
            ExamId = "EXAM01",
            AttemptOrdinal = 2,
            ExamState = ExamState.Completed,
            GradingState = GradingState.Graded,
            CompletedAt = DateTime.UtcNow.AddDays(-1),
            QuestionResults = new List<QuestionResultDto>
            {
                new()
                {
                    QuestionId = "Q1",
                    QuestionTitle = "Trình bày vòng đời (Lifetime) của DbContext và cơ chế Singleton DAO thread-safe.",
                    EarnedScore = 9.0m,
                    MaxScore = 10.0m,
                    IsPassed = true,
                    GradingState = GradingState.Graded,
                    StudentAnswer = "DbContext là Scoped, DAO là Singleton nhận context qua tham số thực thi phương thức.",
                    AiFeedback = "Rất chính xác và đầy đủ chi tiết kỹ thuật.",
                    RubricCriteria = "Hiểu rõ Scoped vs Singleton."
                },
                new()
                {
                    QuestionId = "Q2",
                    QuestionTitle = "Giải thích luồng dữ liệu 3 lớp từ Razor View đến SQL Server và vai trò của BLL.",
                    EarnedScore = 8.0m,
                    MaxScore = 10.0m,
                    IsPassed = true,
                    GradingState = GradingState.Graded,
                    StudentAnswer = "View gọi Controller, Controller gọi BLL Service, Service gọi Repository, Repository gọi DAO truy vấn DbContext.",
                    AiFeedback = "Khắc phục tốt lỗi ở lượt trước, trình bày đúng thứ tự các tầng.",
                    RubricCriteria = "Tuân thủ nghiêm ngặt thứ tự gọi 3 lớp."
                },
                new()
                {
                    QuestionId = "Q3",
                    QuestionTitle = "Cơ chế bảo mật Cookie Authentication và kiểm tra tài khoản bị xóa (Soft Delete).",
                    EarnedScore = 7.0m,
                    MaxScore = 10.0m,
                    IsPassed = true,
                    GradingState = GradingState.Graded,
                    StudentAnswer = "Sử dụng CookieValidatePrincipalContext để từ chối cookie khi account đã bị soft delete.",
                    AiFeedback = "Giải thích rõ ràng cơ chế kiểm tra token/cookie động.",
                    RubricCriteria = "Trình bày đúng OnValidatePrincipal."
                }
            }
        });

        // SV02: Trần Thị B
        _attempts.Add(new AttemptSnapshotDto
        {
            AttemptId = "ATT-003",
            StudentId = "SV02",
            StudentName = "Trần Thị B",
            ClassId = "SE1701",
            ExamId = "EXAM01",
            AttemptOrdinal = 1,
            ExamState = ExamState.Completed,
            GradingState = GradingState.Graded,
            CompletedAt = DateTime.UtcNow.AddDays(-2),
            QuestionResults = new List<QuestionResultDto>
            {
                new()
                {
                    QuestionId = "Q1",
                    QuestionTitle = "Trình bày vòng đời (Lifetime) của DbContext và cơ chế Singleton DAO thread-safe.",
                    EarnedScore = 4.0m,
                    MaxScore = 10.0m,
                    IsPassed = false,
                    GradingState = GradingState.Graded,
                    StudentAnswer = "Em chưa rõ Singleton trong DbContext.",
                    AiFeedback = "Cần ôn tập kỹ bài giảng về Dependency Injection và DAO pattern.",
                    RubricCriteria = "Hiểu rõ Scoped vs Singleton."
                },
                new()
                {
                    QuestionId = "Q2",
                    QuestionTitle = "Giải thích luồng dữ liệu 3 lớp từ Razor View đến SQL Server và vai trò của BLL.",
                    EarnedScore = 2.0m,
                    MaxScore = 10.0m,
                    IsPassed = false,
                    GradingState = GradingState.Graded,
                    StudentAnswer = "Chỉ dùng Controller để query dữ liệu.",
                    AiFeedback = "Vi phạm nghiêm trọng nguyên tắc phân tách trách nhiệm 3 lớp.",
                    RubricCriteria = "Tuân thủ nghiêm ngặt thứ tự gọi 3 lớp."
                },
                new()
                {
                    QuestionId = "Q3",
                    QuestionTitle = "Cơ chế bảo mật Cookie Authentication và kiểm tra tài khoản bị xóa (Soft Delete).",
                    EarnedScore = 3.0m,
                    MaxScore = 10.0m,
                    IsPassed = false,
                    GradingState = GradingState.Graded,
                    StudentAnswer = "Cookie lưu ở trình duyệt thôi.",
                    AiFeedback = "Thiếu kiến thức về ClaimsIdentity và Authentication Middleware.",
                    RubricCriteria = "Trình bày đúng OnValidatePrincipal."
                }
            }
        });

        // SV03: Lê Văn C
        _attempts.Add(new AttemptSnapshotDto
        {
            AttemptId = "ATT-004",
            StudentId = "SV03",
            StudentName = "Lê Văn C",
            ClassId = "SE1701",
            ExamId = "EXAM01",
            AttemptOrdinal = 1,
            ExamState = ExamState.Completed,
            GradingState = GradingState.Graded,
            CompletedAt = DateTime.UtcNow.AddDays(-2),
            QuestionResults = new List<QuestionResultDto>
            {
                new()
                {
                    QuestionId = "Q1",
                    QuestionTitle = "Trình bày vòng đời (Lifetime) của DbContext và cơ chế Singleton DAO thread-safe.",
                    EarnedScore = 6.0m,
                    MaxScore = 10.0m,
                    IsPassed = true,
                    GradingState = GradingState.Graded,
                    StudentAnswer = "DbContext là Scoped, DAO Singleton thread-safe.",
                    AiFeedback = "Đạt yêu cầu tối thiểu, cần giải thích sâu hơn về Captive dependency.",
                    RubricCriteria = "Hiểu rõ Scoped vs Singleton."
                },
                new()
                {
                    QuestionId = "Q2",
                    QuestionTitle = "Giải thích luồng dữ liệu 3 lớp từ Razor View đến SQL Server và vai trò của BLL.",
                    EarnedScore = 6.0m,
                    MaxScore = 10.0m,
                    IsPassed = true,
                    GradingState = GradingState.Graded,
                    StudentAnswer = "View gọi Controller, Controller qua BLL, BLL qua DAL.",
                    AiFeedback = "Đúng luồng gọi cơ bản.",
                    RubricCriteria = "Tuân thủ nghiêm ngặt thứ tự gọi 3 lớp."
                },
                new()
                {
                    QuestionId = "Q3",
                    QuestionTitle = "Cơ chế bảo mật Cookie Authentication và kiểm tra tài khoản bị xóa (Soft Delete).",
                    EarnedScore = 7.0m,
                    MaxScore = 10.0m,
                    IsPassed = true,
                    GradingState = GradingState.Graded,
                    StudentAnswer = "Kiểm tra cookie event trên mỗi request.",
                    AiFeedback = "Khá tốt, giải thích đúng trọng tâm.",
                    RubricCriteria = "Trình bày đúng OnValidatePrincipal."
                }
            }
        });

        // SV03 - Lượt 2: Chờ AI chấm (DEC-10: Chưa chấm không được tính vào trung bình)
        _attempts.Add(new AttemptSnapshotDto
        {
            AttemptId = "ATT-005",
            StudentId = "SV03",
            StudentName = "Lê Văn C",
            ClassId = "SE1701",
            ExamId = "EXAM01",
            AttemptOrdinal = 2,
            ExamState = ExamState.Completed,
            GradingState = GradingState.Pending, // Chờ chấm
            CompletedAt = DateTime.UtcNow.AddHours(-1),
            QuestionResults = new List<QuestionResultDto>
            {
                new()
                {
                    QuestionId = "Q1",
                    QuestionTitle = "Trình bày vòng đời (Lifetime) của DbContext và cơ chế Singleton DAO thread-safe.",
                    EarnedScore = 0m,
                    MaxScore = 10.0m,
                    GradingState = GradingState.Pending,
                    StudentAnswer = "Đã nộp bài, đang chờ hệ thống AI đánh giá.",
                    AiFeedback = "Hệ thống AI đang phân tích bài thi...",
                    RubricCriteria = "Chờ chấm."
                }
            }
        });

        // SV03 - Lượt 3: Bỏ dở (DEC-01: Bỏ dở không tính vào số lượt hoàn thành)
        _attempts.Add(new AttemptSnapshotDto
        {
            AttemptId = "ATT-006",
            StudentId = "SV03",
            StudentName = "Lê Văn C",
            ClassId = "SE1701",
            ExamId = "EXAM01",
            AttemptOrdinal = 3,
            ExamState = ExamState.Abandoned, // Bỏ dở
            GradingState = GradingState.Failed,
            CompletedAt = null
        });
    }

    public Task<List<AttemptSnapshotDto>> GetStudentAttemptsAsync(string studentId, string examId, CancellationToken ct = default)
    {
        var result = _attempts
            .Where(a => a.StudentId == studentId && a.ExamId == examId)
            .ToList();
        return Task.FromResult(result);
    }

    public Task<List<AttemptSnapshotDto>> GetClassAttemptsAsync(string classId, string examId, CancellationToken ct = default)
    {
        var result = _attempts
            .Where(a => a.ClassId == classId && a.ExamId == examId)
            .ToList();
        return Task.FromResult(result);
    }

    public Task<bool> IsTeacherAssignedToClassAsync(string teacherId, string classId, CancellationToken ct = default)
    {
        if (_teacherAssignments.TryGetValue(teacherId, out var classes))
        {
            return Task.FromResult(classes.Contains(classId));
        }
        return Task.FromResult(false);
    }

    public Task<ExamSettingsSnapshot?> GetExamSettingsAsync(string examId, CancellationToken ct = default)
    {
        var exam = _exams.FirstOrDefault(e => e.ExamId == examId);
        return Task.FromResult(exam);
    }

    public Task<List<string>> GetAvailableClassesForTeacherAsync(string teacherId, CancellationToken ct = default)
    {
        if (_teacherAssignments.TryGetValue(teacherId, out var classes))
        {
            return Task.FromResult(classes);
        }
        return Task.FromResult(new List<string>());
    }

    public Task<List<ExamSettingsSnapshot>> GetAvailableExamsAsync(CancellationToken ct = default)
    {
        return Task.FromResult(_exams.ToList());
    }
}
