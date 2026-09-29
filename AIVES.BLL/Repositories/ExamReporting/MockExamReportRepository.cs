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
        // 1. Cấu hình bài thi vấn đáp (AIVES Learning Platform)
        _exams.Add(new ExamSettingsSnapshot
        {
            ExamId = "EXAM01",
            ExamTitle = "Thi Vấn Đáp - PRN222 .NET Core & Kiến trúc Ba lớp",
            SubjectCode = "PRN222",
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
            SubjectCode = "SWE302",
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
                    CognitiveLevel = "Phân tích",
                    EarnedScore = 8.0m,
                    MaxScore = 10.0m,
                    AiSuggestedScore = 7.5m,
                    TeacherFinalScore = 8.0m,
                    TeacherNotes = "Cộng 0.5 điểm vì giải thích tốt câu hỏi hỏi xoáy của AI về Thread Safety.",
                    IsPassed = true,
                    GradingState = GradingState.Graded,
                    StudentInitialAnswerTranscript = "DbContext có Scoped lifetime, được tạo mới theo mỗi HTTP request. DAO là Singleton thread-safe và nhận DbContext làm tham số qua phương thức.",
                    AiFollowUpQuestion = "AI Giám khảo hỏi xoáy: Tại sao nếu Singleton DAO giữ trực tiếp trường `private AIVESDbContext _context` thì sẽ gây crash hệ thống khi có nhiều request đồng thời?",
                    StudentFollowUpAnswerTranscript = "Vì DbContext không thread-safe. Nếu hai request cùng lúc gọi vào instance Singleton, hai luồng sẽ dùng chung một DbContext dẫn tới xung đột bộ nhớ và exception.",
                    AiFeedback = "Câu trả lời xuất sắc. Nắm vững ranh giới giữa Singleton DAO và Scoped DbContext, giải thích đúng cơ chế Captive Dependency.",
                    Strengths = "Hiểu sâu sắc về DI Lifetime và quản lý tài nguyên bất đồng bộ.",
                    Weaknesses = "Cần nêu thêm từ khóa `lock` hoặc Semaphore nếu DAO xử lý tài nguyên tĩnh.",
                    RubricCriteria = "Tiêu chí 1 (5đ): Vòng đời DI; Tiêu chí 2 (5đ): Cơ chế truyền tham số phương thức."
                },
                new()
                {
                    QuestionId = "Q2",
                    QuestionTitle = "Giải thích luồng dữ liệu 3 lớp từ Razor View đến SQL Server và vai trò của BLL.",
                    CognitiveLevel = "Vận dụng",
                    EarnedScore = 4.0m,
                    MaxScore = 10.0m,
                    AiSuggestedScore = 4.0m,
                    TeacherFinalScore = 4.0m,
                    TeacherNotes = "Sinh viên chưa nắm vững nguyên tắc độc lập giữa View và Data Access.",
                    IsPassed = false, // < 6.0 => Không đạt theo DEC-03
                    GradingState = GradingState.Graded,
                    StudentInitialAnswerTranscript = "View gọi thẳng Repository để lấy dữ liệu nhanh hơn, sau đó đưa vào DbContext.",
                    AiFollowUpQuestion = "AI Giám khảo hỏi xoáy: Nếu View gọi thẳng Repository thì logic kiểm tra nghiệp vụ và phân quyền sẽ được đặt ở đâu?",
                    StudentFollowUpAnswerTranscript = "Dạ đặt ở bên trong Controller ạ.",
                    AiFeedback = "Sai ranh giới kiến trúc. Controller không được gọi trực tiếp Repository/DbContext mà phải đi qua BLL Service để đảm bảo tính toàn vẹn nghiệp vụ.",
                    Strengths = "Nhận thức được luồng truy vấn cơ bản.",
                    Weaknesses = "Bỏ qua tầng BLL (Business Logic Layer), vi phạm quy chuẩn thiết kế 3 lớp.",
                    RubricCriteria = "Tuân thủ nghiêm ngặt View -> Controller -> Service -> Repository -> DAO -> DbContext."
                },
                new()
                {
                    QuestionId = "Q3",
                    QuestionTitle = "Cơ chế bảo mật Cookie Authentication và kiểm tra tài khoản bị xóa (Soft Delete).",
                    CognitiveLevel = "Hiểu",
                    EarnedScore = 5.0m,
                    MaxScore = 10.0m,
                    AiSuggestedScore = 5.0m,
                    TeacherFinalScore = 5.0m,
                    TeacherNotes = "Cần ôn tập kỹ bài giảng Security Middleware.",
                    IsPassed = false, // < 6.0 => Không đạt
                    GradingState = GradingState.Graded,
                    StudentInitialAnswerTranscript = "Cookie lưu token đăng nhập của người dùng. Khi đăng nhập kiểm tra database 1 lần là đủ.",
                    AiFollowUpQuestion = "AI Giám khảo hỏi xoáy: Nếu Admin xóa tài khoản của sinh viên sau khi sinh viên đã đăng nhập, làm sao để hệ thống chặn ngay các request tiếp theo?",
                    StudentFollowUpAnswerTranscript = "Dạ sinh viên tự đăng xuất ra thì mới bị chặn ạ.",
                    AiFeedback = "Thiếu cơ chế ActiveAccountCookieEvents để kiểm tra trạng thái tài khoản trên mỗi request kế tiếp trong sự kiện OnValidatePrincipal.",
                    Strengths = "Biết cách cấu hình CookieAuthenticationScheme cơ bản.",
                    Weaknesses = "Chưa xử lý bài toán thu hồi quyền (Session Revocation) thời gian thực.",
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
                    CognitiveLevel = "Phân tích",
                    EarnedScore = 9.0m,
                    MaxScore = 10.0m,
                    AiSuggestedScore = 9.0m,
                    TeacherFinalScore = 9.0m,
                    IsPassed = true,
                    GradingState = GradingState.Graded,
                    StudentInitialAnswerTranscript = "DbContext là Scoped, DAO là Singleton nhận context qua tham số thực thi phương thức.",
                    AiFollowUpQuestion = "AI Giám khảo hỏi xoáy: Cơ chế này giải quyết vấn đề gì trong lập trình Web đa luồng?",
                    StudentFollowUpAnswerTranscript = "Giải quyết triệt để lỗi ObjectDisposedException và Race Condition giữa các luồng HTTP.",
                    AiFeedback = "Rất chính xác, thể hiện sự tiến bộ vượt bậc so với lượt thi đầu.",
                    Strengths = "Lập luận sắc bén, dẫn chứng thuyết phục.",
                    Weaknesses = "Không có thiếu sót đáng kể.",
                    RubricCriteria = "Hiểu rõ Scoped vs Singleton."
                },
                new()
                {
                    QuestionId = "Q2",
                    QuestionTitle = "Giải thích luồng dữ liệu 3 lớp từ Razor View đến SQL Server và vai trò của BLL.",
                    CognitiveLevel = "Vận dụng",
                    EarnedScore = 8.0m,
                    MaxScore = 10.0m,
                    AiSuggestedScore = 8.0m,
                    TeacherFinalScore = 8.0m,
                    IsPassed = true,
                    GradingState = GradingState.Graded,
                    StudentInitialAnswerTranscript = "View gửi dữ liệu đến Controller, Controller gọi BLL Service. Service thực thi nghiệp vụ rồi gọi Repository.",
                    AiFollowUpQuestion = "AI Giám khảo hỏi xoáy: Tại sao BLL không nên phụ thuộc trực tiếp vào Entity Framework Core?",
                    StudentFollowUpAnswerTranscript = "Để BLL độc lập với công nghệ lưu trữ, có thể test độc lập (Unit Test) bằng Mock Repository mà không cần database thật.",
                    AiFeedback = "Khắc phục triệt để lỗi kiến trúc ở lượt 1, giải thích xuất sắc lợi ích của Loose Coupling.",
                    Strengths = "Hiểu rõ nguyên lý Dependency Inversion.",
                    Weaknesses = "Cần vẽ thêm sơ đồ component để hoàn hảo.",
                    RubricCriteria = "Tuân thủ nghiêm ngặt thứ tự gọi 3 lớp."
                },
                new()
                {
                    QuestionId = "Q3",
                    QuestionTitle = "Cơ chế bảo mật Cookie Authentication và kiểm tra tài khoản bị xóa (Soft Delete).",
                    CognitiveLevel = "Hiểu",
                    EarnedScore = 7.0m,
                    MaxScore = 10.0m,
                    AiSuggestedScore = 7.0m,
                    TeacherFinalScore = 7.0m,
                    IsPassed = true,
                    GradingState = GradingState.Graded,
                    StudentInitialAnswerTranscript = "Dùng sự kiện ValidatePrincipal trong CookieAuthenticationEvents để tra cứu trạng thái IsDeleted từ IAuthService.",
                    AiFollowUpQuestion = "AI Giám khảo hỏi xoáy: Có cần kiểm tra tài khoản Admin mặc định từ file appsettings không?",
                    StudentFollowUpAnswerTranscript = "Không cần kiểm tra database với Admin từ appsettings vì Admin là cấu hình hệ thống.",
                    AiFeedback = "Giải thích rõ ràng cơ chế kiểm tra token/cookie động.",
                    Strengths = "Nắm đúng lifecycle của Authentication Middleware.",
                    Weaknesses = "Cần tối ưu cache để giảm tải truy vấn DB.",
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
                    CognitiveLevel = "Phân tích",
                    EarnedScore = 4.0m,
                    MaxScore = 10.0m,
                    AiSuggestedScore = 4.0m,
                    TeacherFinalScore = 4.0m,
                    IsPassed = false,
                    GradingState = GradingState.Graded,
                    StudentInitialAnswerTranscript = "Em chưa rõ Singleton trong DbContext.",
                    AiFollowUpQuestion = "AI Giám khảo hỏi gợi mở: Bạn có biết Singleton và Transient khác nhau thế nào không?",
                    StudentFollowUpAnswerTranscript = "Dạ Singleton là chỉ có một đối tượng duy nhất thôi ạ.",
                    AiFeedback = "Cần ôn tập kỹ bài giảng về Dependency Injection và DAO pattern.",
                    Strengths = "Biết định nghĩa cơ bản của Singleton.",
                    Weaknesses = "Chưa áp dụng được vào mô hình quản trị kết nối cơ sở dữ liệu.",
                    RubricCriteria = "Hiểu rõ Scoped vs Singleton."
                },
                new()
                {
                    QuestionId = "Q2",
                    QuestionTitle = "Giải thích luồng dữ liệu 3 lớp từ Razor View đến SQL Server và vai trò của BLL.",
                    CognitiveLevel = "Vận dụng",
                    EarnedScore = 2.0m,
                    MaxScore = 10.0m,
                    AiSuggestedScore = 2.0m,
                    TeacherFinalScore = 2.0m,
                    IsPassed = false,
                    GradingState = GradingState.Graded,
                    StudentInitialAnswerTranscript = "Chỉ dùng Controller để query dữ liệu.",
                    AiFollowUpQuestion = "AI Giám khảo hỏi xoáy: Nếu Controller query trực tiếp DB thì kiến trúc 3 lớp có còn ý nghĩa không?",
                    StudentFollowUpAnswerTranscript = "Dạ em không biết ạ.",
                    AiFeedback = "Vi phạm nghiêm trọng nguyên tắc phân tách trách nhiệm 3 lớp.",
                    Strengths = "Không có.",
                    Weaknesses = "Hổng kiến thức nền tảng về Clean Architecture và MVC.",
                    RubricCriteria = "Tuân thủ nghiêm ngặt thứ tự gọi 3 lớp."
                },
                new()
                {
                    QuestionId = "Q3",
                    QuestionTitle = "Cơ chế bảo mật Cookie Authentication và kiểm tra tài khoản bị xóa (Soft Delete).",
                    CognitiveLevel = "Hiểu",
                    EarnedScore = 3.0m,
                    MaxScore = 10.0m,
                    AiSuggestedScore = 3.0m,
                    TeacherFinalScore = 3.0m,
                    IsPassed = false,
                    GradingState = GradingState.Graded,
                    StudentInitialAnswerTranscript = "Cookie lưu ở trình duyệt thôi.",
                    AiFollowUpQuestion = "AI Giám khảo hỏi xoáy: Làm thế nào server biết cookie đó còn hợp lệ?",
                    StudentFollowUpAnswerTranscript = "Dạ trình duyệt tự gửi lên thôi ạ.",
                    AiFeedback = "Thiếu kiến thức về ClaimsIdentity và Authentication Middleware.",
                    Strengths = "Biết cookie lưu ở client.",
                    Weaknesses = "Không hiểu cơ chế mã hóa và xác thực chữ ký của ASP.NET Core.",
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
                    CognitiveLevel = "Phân tích",
                    EarnedScore = 6.0m,
                    MaxScore = 10.0m,
                    AiSuggestedScore = 6.0m,
                    TeacherFinalScore = 6.0m,
                    IsPassed = true,
                    GradingState = GradingState.Graded,
                    StudentInitialAnswerTranscript = "DbContext là Scoped, DAO Singleton thread-safe.",
                    AiFollowUpQuestion = "AI Giám khảo hỏi xoáy: Tại sao DAO không lưu context dạng field?",
                    StudentFollowUpAnswerTranscript = "Để tránh bị lỗi chia sẻ trạng thái không an toàn.",
                    AiFeedback = "Đạt yêu cầu tối thiểu, cần giải thích sâu hơn về Captive dependency.",
                    Strengths = "Trả lời đúng trọng tâm.",
                    Weaknesses = "Chưa đào sâu kỹ thuật đa luồng.",
                    RubricCriteria = "Hiểu rõ Scoped vs Singleton."
                },
                new()
                {
                    QuestionId = "Q2",
                    QuestionTitle = "Giải thích luồng dữ liệu 3 lớp từ Razor View đến SQL Server và vai trò của BLL.",
                    CognitiveLevel = "Vận dụng",
                    EarnedScore = 6.0m,
                    MaxScore = 10.0m,
                    AiSuggestedScore = 6.0m,
                    TeacherFinalScore = 6.0m,
                    IsPassed = true,
                    GradingState = GradingState.Graded,
                    StudentInitialAnswerTranscript = "View gọi Controller, Controller qua BLL, BLL qua DAL.",
                    AiFollowUpQuestion = "AI Giám khảo hỏi xoáy: Tầng nào sẽ quyết định lưu log kiểm toán (Audit Log)?",
                    StudentFollowUpAnswerTranscript = "Tầng BLL phối hợp với DAL ạ.",
                    AiFeedback = "Đúng luồng gọi cơ bản, tư duy logic tốt.",
                    Strengths = "Nắm được sơ đồ tổng thể.",
                    Weaknesses = "Cần phân biệt rõ trách nhiệm giữa Controller và BLL.",
                    RubricCriteria = "Tuân thủ nghiêm ngặt thứ tự gọi 3 lớp."
                },
                new()
                {
                    QuestionId = "Q3",
                    QuestionTitle = "Cơ chế bảo mật Cookie Authentication và kiểm tra tài khoản bị xóa (Soft Delete).",
                    CognitiveLevel = "Hiểu",
                    EarnedScore = 7.0m,
                    MaxScore = 10.0m,
                    AiSuggestedScore = 7.0m,
                    TeacherFinalScore = 7.0m,
                    IsPassed = true,
                    GradingState = GradingState.Graded,
                    StudentInitialAnswerTranscript = "Kiểm tra cookie event trên mỗi request.",
                    AiFollowUpQuestion = "AI Giám khảo hỏi xoáy: Nếu cookie hết hạn (Expired) thì người dùng chuyển về đâu?",
                    StudentFollowUpAnswerTranscript = "Về trang đăng nhập /Account/Login với tham số ReturnUrl.",
                    AiFeedback = "Khá tốt, giải thích đúng trọng tâm và các luồng chuyển hướng.",
                    Strengths = "Hiểu rõ cơ chế ReturnUrl và Cookie Expiration.",
                    Weaknesses = "Cần bổ sung chi tiết về sliding expiration.",
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
                    CognitiveLevel = "Phân tích",
                    EarnedScore = 0m,
                    MaxScore = 10.0m,
                    GradingState = GradingState.Pending,
                    StudentInitialAnswerTranscript = "Đã ghi âm bài nói, hệ thống Speech-to-Text đang hoàn thiện transcript...",
                    AiFollowUpQuestion = "Đang tổng hợp nhận xét từ mô hình ngôn ngữ lớn...",
                    AiFeedback = "Hệ thống AI đang phân tích bài thi và đối chiếu rubric...",
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
